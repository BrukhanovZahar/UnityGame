using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static float BulletDamageBonus { get; private set; }

    [Header("Refs")]
    [SerializeField] private CrystallCollector crystals;
    [SerializeField] private Gun gun;
    [SerializeField] private Life baseLife;

    [Header("Bullet damage")]
    [SerializeField] private int damageBaseCost = 5;
    [SerializeField] private float damagePerLevel = 5f;

    [Header("Fire rate")]
    [SerializeField] private int fireRateBaseCost = 5;
    [SerializeField] private float fireRateMultiplier = 0.9f;

    [Header("Base HP")]
    [SerializeField] private int baseHpBaseCost = 5;
    [SerializeField] private float baseHpPerLevel = 50f;

    [Header("Cost")]
    [SerializeField] private int costGrowth = 3;

    private int damageLevel;
    private int fireRateLevel;
    private int baseHpLevel;

    private void Awake()
    {
        BulletDamageBonus = 0f;
    }

    public int DamageCost => damageBaseCost + damageLevel * costGrowth;
    public int FireRateCost => fireRateBaseCost + fireRateLevel * costGrowth;
    public int BaseHpCost => baseHpBaseCost + baseHpLevel * costGrowth;

    public bool BuyDamage()
    {
        if (crystals == null || !crystals.TryGetCrystals(DamageCost)) return false;
        damageLevel++;
        BulletDamageBonus += damagePerLevel;
        return true;
    }

    public bool BuyFireRate()
    {
        if (crystals == null || !crystals.TryGetCrystals(FireRateCost)) return false;
        fireRateLevel++;
        if (gun != null) gun.MultiplyCooldown(fireRateMultiplier);
        return true;
    }

    public bool BuyBaseHp()
    {
        if (crystals == null || !crystals.TryGetCrystals(BaseHpCost)) return false;
        baseHpLevel++;
        if (baseLife != null) baseLife.SetMaxHP(baseLife.MaxHP + baseHpPerLevel);
        return true;
    }
}
