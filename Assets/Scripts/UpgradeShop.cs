using UnityEngine;
using TMPro;

public class UpgradeShop : MonoBehaviour
{
    [SerializeField] private UpgradeManager upgrades;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private GameObject panel;

    [Header("Cost labels")]
    [SerializeField] private TextMeshProUGUI damageCostText;
    [SerializeField] private TextMeshProUGUI fireRateCostText;
    [SerializeField] private TextMeshProUGUI baseHpCostText;

    private void Start()
    {
        if (panel != null) panel.SetActive(false);
    }

    public void Open()
    {
        if (panel != null) panel.SetActive(true);
        GlobalContext.Pause = true;
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Refresh();
    }

    public void Continue()
    {
        if (panel != null) panel.SetActive(false);
        GlobalContext.Pause = false;
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (waveManager != null) waveManager.ContinueToNextWave();
    }

    public void BuyDamage()
    {
        if (upgrades.BuyDamage()) Refresh();
    }

    public void BuyFireRate()
    {
        if (upgrades.BuyFireRate()) Refresh();
    }

    public void BuyBaseHp()
    {
        if (upgrades.BuyBaseHp()) Refresh();
    }

    private void Refresh()
    {
        if (damageCostText != null) damageCostText.text = upgrades.DamageCost.ToString();
        if (fireRateCostText != null) fireRateCostText.text = upgrades.FireRateCost.ToString();
        if (baseHpCostText != null) baseHpCostText.text = upgrades.BaseHpCost.ToString();
    }
}
