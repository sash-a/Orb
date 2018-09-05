using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GunnerUI : PlayerUI
{
    #region Variables

    // Components
    private NetHealth health;
    private ResourceManager resourceManager;
    private WeaponAttack weapons;

    // Bars
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private RectTransform shieldBar;

    // Gun icons
    [SerializeField] private Image gunSlotBorder0;
    [SerializeField] private Image gunSlotBorder1;
    [SerializeField] private Image gunSlotBorder2;

    [SerializeField] private Image gunIcon1;
    [SerializeField] private Image gunIcon2;

    public Sprite pistolIcon;
    public Sprite rifleIcon;
    public Sprite shotgunIcon;
    public Sprite sniperIcon;


    [SerializeField] private Sprite borderEquipped;
    [SerializeField] private Sprite borderUnequipped;


    // Ammo count
    [SerializeField] private TextMeshProUGUI ammoCount;

    // Game state indicators
    [SerializeField] private TextMeshProUGUI energyCount;
    [SerializeField] private TextMeshProUGUI livingPlayerCount;
    [SerializeField] private TextMeshProUGUI killCount;

    // Other classes UIs
    public GameObject magicianUI;
    public GameObject weaponWheel;

    #endregion

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
        if (player != null && player.GetComponent<Identifier>().typePrefix != Identifier.gunnerType)
        {
            Debug.LogError("Displaying incorrect HUD");
            return;
        }

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
        setShield(health.getArmourPercent());
        setEnergy((int) resourceManager.getEnergy());
        showEquipped();
        displayWeaponWheel();

        if (weapons.weapons[weapons.selectedWeapon].name == "digging tool")
        {
            setAmmo(true);
        }
        else
        {
            setAmmo
            (
                false,
                weapons.weapons[weapons.selectedWeapon].ammunition.getMagAmmo(),
                weapons.weapons[weapons.selectedWeapon].ammunition.getPrimaryAmmo()
            );
        }
    }

    void displayWeaponWheel()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            weaponWheel.SetActive(true);
            togglePauseMenu();
        }

        if (Input.GetKeyUp(KeyCode.Q))
        {
            weaponWheel.SetActive(false);
            togglePauseMenu();
        }
    }

    void showEquipped()
    {
        if (weapons.equippedWeapon == 0)
        {
            gunSlotBorder0.sprite = borderEquipped;
            gunSlotBorder1.sprite = borderUnequipped;
            gunSlotBorder2.sprite = borderUnequipped;
        }
        else if (weapons.equippedWeapon == 1)
        {
            gunSlotBorder0.sprite = borderUnequipped;
            gunSlotBorder1.sprite = borderEquipped;
            gunSlotBorder2.sprite = borderUnequipped;
        }
        else if (weapons.equippedWeapon == 2)
        {
            gunSlotBorder0.sprite = borderUnequipped;
            gunSlotBorder1.sprite = borderUnequipped;
            gunSlotBorder2.sprite = borderEquipped;
        }
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

    void setAmmo(bool isDigger, int clip = 0, int total = 0)
    {
        if (isDigger)
        {
            ammoCount.text = "inf";
        }
        else
        {
            ammoCount.text = clip + "|" + total;
        }
    }
}