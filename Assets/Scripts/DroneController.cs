using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(Life))]
public class DroneController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;          // object with Life that takes damage
    [SerializeField] private string targetTag = "Base";
    [SerializeField] private Transform aimOverride;     // point to orbit; parent it to the moving base
    [SerializeField] private string aimTag = "BaseAim";

    [Header("Flight")]
    [SerializeField] private float moveSpeed = 24f;
    [SerializeField] private float turnSpeed = 6f;

    [Header("Orbit (relative to the planet, so it works under 3-axis spin)")]
    [SerializeField] private float standoffHeight = 16f;        // how far out from the base, along the planet's outward normal
    [SerializeField] private float orbitRadius = 7f;            // small fast loop radius
    [SerializeField] private float orbitDegreesPerSecond = 80f; // speed of the small loop
    [SerializeField] private float wanderRadius = 13f;          // the small loop itself drifts around this larger radius
    [SerializeField] private float wanderDegreesPerSecond = 22f;// speed of that slow drift
    [SerializeField] private float surfaceClearance = 5f;       // min distance kept from the asteroid surface
    [SerializeField] private float shieldClearance = 5f;        // min distance kept from the shield dome (covers the model's nose)

    [Header("Attack")]
    [SerializeField] private float attackRange = 30f;       // max distance to base at which the drone can attack
    [SerializeField] private float attackDamage = 5f;
    [SerializeField] private float attackInterval = 1.5f;
    [SerializeField] private string attackSoundKey;
    [SerializeField] private UnityEvent onAttack;

    [Header("Refs")]
    [SerializeField] private Life life;
    [SerializeField] private NavMeshAgent agent;            // disabled at runtime; we fly manually in 3D space
    [SerializeField] private Drop drop;
    [SerializeField] private EnemyRandomizator randomizator;

    [Header("Death")]
    [SerializeField] private string deathEffectKey;
    [SerializeField] private string deathSoundKey;
    [SerializeField] private float despawnDelay = 0.1f;
    [SerializeField] private UnityEvent onDeath;

    private Life targetLife;
    private Collider shieldCollider;
    private float lastAttackTime;
    private bool dead;
    private float orbitAngle;
    private float wanderAngle;

    private void Awake()
    {
        if (life == null) life = GetComponent<Life>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;           // flight is manual, NavMesh would fight it
    }

    private void Start()
    {
        if (target == null && !string.IsNullOrEmpty(targetTag))
        {
            GameObject t = GameObject.FindGameObjectWithTag(targetTag);
            if (t != null) target = t.transform;
        }
        if (target != null) target.TryGetComponent(out targetLife);

        if (aimOverride == null && !string.IsNullOrEmpty(aimTag))
        {
            try
            {
                GameObject a = GameObject.FindGameObjectWithTag(aimTag);
                if (a != null) aimOverride = a.transform;
            }
            catch (UnityException) { }                       // tag not defined — fall back to target
        }

        if (target == null)
            Debug.LogWarning($"[DroneController] {name}: target not found by tag '{targetTag}'");

        // the shield dome carries a ForceShield component + collider; use it to push drones out of the dome
        ForceShield shield = FindObjectOfType<ForceShield>();
        if (shield != null) shieldCollider = shield.GetComponent<Collider>();

        // start the orbit from whatever side the drone spawned, so multiple drones spread out
        Transform aim = aimOverride != null ? aimOverride : target;
        if (aim != null)
        {
            Vector3 flat = transform.position - aim.position;
            orbitAngle = Mathf.Atan2(flat.z, flat.x) * Mathf.Rad2Deg;
        }

        if (randomizator != null) randomizator.Randomize();
        if (life != null) life.OnDie.AddListener(Die);
    }

    private void Update()
    {
        if (dead || GlobalContext.Pause || target == null) return;

        // aim follows the base every frame, so the drone tracks it even as the asteroid rotates/moves
        Transform aim = aimOverride != null ? aimOverride : target;
        Vector3 center = aim.position;

        // everything is computed relative to the planet center, so it stays correct under 3-axis spin
        MeshCollider asteroid = GlobalContext.AsteriodCollider;
        Vector3 planetCenter = asteroid != null ? asteroid.bounds.center : center - Vector3.up;
        Vector3 radialOut = (center - planetCenter).normalized;      // "up" direction at the base
        if (radialOut.sqrMagnitude < 0.001f) radialOut = Vector3.up;

        // two axes spanning the plane tangent to the planet surface at the base
        Vector3 tangentA = Vector3.Cross(radialOut, Vector3.up);
        if (tangentA.sqrMagnitude < 0.01f) tangentA = Vector3.Cross(radialOut, Vector3.right);
        tangentA.Normalize();
        Vector3 tangentB = Vector3.Cross(radialOut, tangentA).normalized;

        // orbit slot: a small fast loop whose center itself drifts around a larger slow loop,
        // so the drone sweeps a whole disk above the base instead of one fixed circle
        orbitAngle += orbitDegreesPerSecond * Time.deltaTime;
        wanderAngle += wanderDegreesPerSecond * Time.deltaTime;
        float a = orbitAngle * Mathf.Deg2Rad;
        float w = wanderAngle * Mathf.Deg2Rad;
        Vector3 fastLoop = (tangentA * Mathf.Cos(a) + tangentB * Mathf.Sin(a)) * orbitRadius;
        Vector3 slowDrift = (tangentA * Mathf.Cos(w) + tangentB * Mathf.Sin(w)) * wanderRadius;
        Vector3 orbitSlot = center + radialOut * standoffHeight + fastLoop + slowDrift;

        transform.position = Vector3.MoveTowards(transform.position, orbitSlot, moveSpeed * Time.deltaTime);

        // safety net: never sink into the asteroid or the shield dome
        PushOutOf(asteroid, surfaceClearance);
        PushOutOf(shieldCollider, shieldClearance);

        // belly toward the planet center: up = outward normal at the drone, nose toward the base
        Vector3 up = (transform.position - planetCenter).normalized;
        if (up.sqrMagnitude < 0.001f) up = Vector3.up;
        Vector3 forward = Vector3.ProjectOnPlane(center - transform.position, up);
        if (forward.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(forward.normalized, up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
        }

        // engaged = the drone has reached the shield (hugging it). This is independent of dome size,
        // so it works no matter how big the shield is. Falls back to base distance if no shield.
        bool engaged;
        if (shieldCollider != null)
        {
            Vector3 cp = shieldCollider.ClosestPoint(transform.position);
            engaged = Vector3.Distance(transform.position, cp) <= shieldClearance + 2f;
        }
        else
        {
            engaged = Vector3.Distance(transform.position, center) <= attackRange;
        }

        if (engaged && Time.time - lastAttackTime >= attackInterval)
        {
            lastAttackTime = Time.time;
            Attack();
        }
    }

    // keep the drone at least 'clearance' units away from the given collider's surface
    private void PushOutOf(Collider col, float clearance)
    {
        if (col == null) return;
        Vector3 closest = col.ClosestPoint(transform.position);
        Vector3 away = transform.position - closest;
        float d = away.magnitude;
        if (d < clearance)
        {
            Vector3 n = d > 0.001f ? away / d : Vector3.up;
            transform.position = closest + n * clearance;
        }
    }

    private void Attack()
    {
        if (targetLife != null) targetLife.TakeDamage(attackDamage);
        if (!string.IsNullOrEmpty(attackSoundKey)) DJ.Play(attackSoundKey);
        onAttack.Invoke();
    }

    private void Die()
    {
        if (dead) return;
        dead = true;

        if (drop != null) drop.DropItem();
        if (!string.IsNullOrEmpty(deathEffectKey) && ObjectPool.TryGet(deathEffectKey, out PooledObject fx))
            fx.transform.position = transform.position;
        if (!string.IsNullOrEmpty(deathSoundKey)) DJ.Play(deathSoundKey);

        onDeath.Invoke();
        Destroy(gameObject, despawnDelay);
    }

    // called by WaveManager to scale difficulty per wave
    public void Configure(float damageMultiplier, float speedMultiplier)
    {
        attackDamage *= damageMultiplier;
        moveSpeed *= speedMultiplier;
    }
}
