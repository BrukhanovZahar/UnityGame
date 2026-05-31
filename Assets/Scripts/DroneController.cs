using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(Life))]
public class DroneController : MonoBehaviour
{
    private static readonly List<DroneController> All = new List<DroneController>();

    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private string targetTag = "Base";
    [SerializeField] private Transform aimOverride;
    [SerializeField] private string aimTag = "BaseAim";

    [Header("Flight")]
    [SerializeField] private float moveSpeed = 24f;
    [SerializeField] private float turnSpeed = 6f;

    [Header("Orbit")]
    [SerializeField] private float standoffHeight = 16f;
    [SerializeField] private float orbitRadius = 7f;
    [SerializeField] private float orbitDegreesPerSecond = 80f;
    [SerializeField] private float wanderRadius = 13f;
    [SerializeField] private float wanderDegreesPerSecond = 22f;
    [SerializeField] private float surfaceClearance = 5f;
    [SerializeField] private float shieldClearance = 5f;

    [Header("Separation")]
    [SerializeField] private float separationRadius = 9f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 30f;
    [SerializeField] private float attackDamage = 5f;
    [SerializeField] private float attackInterval = 1.5f;
    [SerializeField] private string attackSoundKey;
    [SerializeField] private Transform muzzle;
    [SerializeField] private string projectileKey;
    [SerializeField] private UnityEvent onAttack;

    [Header("Refs")]
    [SerializeField] private Life life;
    [SerializeField] private NavMeshAgent agent;
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
        if (agent != null) agent.enabled = false;
        All.Add(this);
    }

    private void OnDestroy()
    {
        All.Remove(this);
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
            catch (UnityException) { }
        }

        ForceShield shield = FindObjectOfType<ForceShield>();
        if (shield != null) shieldCollider = shield.GetComponent<Collider>();

        orbitAngle = Random.Range(0f, 360f);
        wanderAngle = Random.Range(0f, 360f);
        standoffHeight *= Random.Range(0.85f, 1.25f);
        orbitRadius *= Random.Range(0.8f, 1.3f);
        wanderRadius *= Random.Range(0.8f, 1.3f);
        if (Random.value < 0.5f) orbitDegreesPerSecond = -orbitDegreesPerSecond;
        if (Random.value < 0.5f) wanderDegreesPerSecond = -wanderDegreesPerSecond;

        if (randomizator != null) randomizator.Randomize();
        if (life != null) life.OnDie.AddListener(Die);
    }

    private void Update()
    {
        if (dead || GlobalContext.Pause || target == null) return;

        Transform aim = aimOverride != null ? aimOverride : target;
        Vector3 center = aim.position;

        MeshCollider asteroid = GlobalContext.AsteriodCollider;
        Vector3 planetCenter = asteroid != null ? asteroid.bounds.center : center - Vector3.up;
        Vector3 radialOut = (center - planetCenter).normalized;
        if (radialOut.sqrMagnitude < 0.001f) radialOut = Vector3.up;

        Vector3 tangentA = Vector3.Cross(radialOut, Vector3.up);
        if (tangentA.sqrMagnitude < 0.01f) tangentA = Vector3.Cross(radialOut, Vector3.right);
        tangentA.Normalize();
        Vector3 tangentB = Vector3.Cross(radialOut, tangentA).normalized;

        orbitAngle += orbitDegreesPerSecond * Time.deltaTime;
        wanderAngle += wanderDegreesPerSecond * Time.deltaTime;
        float a = orbitAngle * Mathf.Deg2Rad;
        float w = wanderAngle * Mathf.Deg2Rad;
        Vector3 fastLoop = (tangentA * Mathf.Cos(a) + tangentB * Mathf.Sin(a)) * orbitRadius;
        Vector3 slowDrift = (tangentA * Mathf.Cos(w) + tangentB * Mathf.Sin(w)) * wanderRadius;
        Vector3 orbitSlot = center + radialOut * standoffHeight + fastLoop + slowDrift;

        transform.position = Vector3.MoveTowards(transform.position, orbitSlot, moveSpeed * Time.deltaTime);

        ResolveSeparation();

        PushOutOf(asteroid, surfaceClearance);
        PushOutOf(shieldCollider, shieldClearance);

        Vector3 up = (transform.position - planetCenter).normalized;
        if (up.sqrMagnitude < 0.001f) up = Vector3.up;
        Vector3 forward = Vector3.ProjectOnPlane(center - transform.position, up);
        if (forward.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(forward.normalized, up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
        }

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
            Attack(center);
        }
    }

    private void ResolveSeparation()
    {
        for (int i = 0; i < All.Count; i++)
        {
            DroneController other = All[i];
            if (other == null || other == this) continue;
            Vector3 diff = transform.position - other.transform.position;
            float d = diff.magnitude;
            if (d < separationRadius && d > 0.0001f)
                transform.position += (diff / d) * (separationRadius - d) * 0.5f;
        }
    }

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

    private void Attack(Vector3 aimPos)
    {
        if (targetLife != null) targetLife.TakeDamage(attackDamage);
        if (!string.IsNullOrEmpty(attackSoundKey)) DJ.Play(attackSoundKey);
        FireProjectile(aimPos);
        onAttack.Invoke();
    }

    private void FireProjectile(Vector3 aimPos)
    {
        if (string.IsNullOrEmpty(projectileKey)) return;

        Transform from = muzzle != null ? muzzle : transform;
        if (ObjectPool.TryGet(projectileKey, out PooledObject projectile))
        {
            projectile.transform.position = from.position;
            Vector3 dir = aimPos - from.position;
            if (dir.sqrMagnitude > 0.0001f)
                projectile.transform.rotation = Quaternion.LookRotation(dir);
            projectile.Action();
        }
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

    public void Configure(float damageMultiplier, float speedMultiplier, float hpMultiplier)
    {
        attackDamage *= damageMultiplier;
        moveSpeed *= speedMultiplier;
        if (life != null) life.SetMaxHP(life.MaxHP * hpMultiplier);
    }
}
