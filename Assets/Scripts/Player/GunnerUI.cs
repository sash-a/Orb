using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GunnerUI : MonoBehaviour
{
    #region Variables

    private GameObject player;

    // Components
    private NetHealth health;
    private ResourceManager resourceManager;
    private WeaponAttack weapons;

    // Bars
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private RectTransform shieldBar;

    // Gun icons
    [SerializeField] private Image gunSlot0;
    [SerializeField] private Image gunSlot1;
    [SerializeField] private Image gunSlot2;
    [SerializeField] private Image gunSlot3;

    // Ammo count
    [SerializeField] private TextMeshProUGUI ammoCount;

    // Game state indicators
    [SerializeField] private TextMeshProUGUI energyCount;
    [SerializeField] private TextMeshProUGUI livingPlayerCount;
    [SerializeField] private TextMeshProUGUI killCount;

    #endregion

    public static bool isPaused;

    private void Start()
    {
        if (player != null && player.GetComponent<Identifier>().typePrefix != Identifier.gunnerType)
        {
            Debug.LogError("Displaying incorrect HUD");
        }
    }

    public void setUp(GameObject localPlayer)
    {
        player = localPlayer;
        health = player.GetComponent<NetHealth>();
        resourceManager = player.GetComponent<ResourceManager>();
        weapons = player.GetComponent<WeaponAttack>();

        setHealth(1);
        setShield(1);

        isPaused = false;
    }

    void Update()
    {
        setHealth(health.getHealthPercent());
        setShield(resourceManager.getShieldPercent());
        setEnergy((int) (resourceManager.getEnergy() / resourceManager.getMaxEnergy()));
        setAmmo
        (
            weapons.weapons[weapons.selectedWeapon].ammunition.getMagAmmo(),
            weapons.weapons[weapons.selectedWeapon].ammunition.getPrimaryAmmo()
        );

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            togglePauseMenu();
        }
    }

    void togglePauseMenu()
    {
        isPaused = !isPaused;
    }

    void setHealth(float h)
    {
        healthBar.localScale = new Vector3(1f, h, 1f);
    }

    void setShield(float amount)
    {
        shieldBar.localScale = new Vector3(1f, amount, 1f);
    }

    void setEnergy(int amount)
    {
        energyCount.text = amount + "";
    }

    void setAmmo(int clip, int total)
    {
        ammoCount.text = clip + "|" + total;
    }
}