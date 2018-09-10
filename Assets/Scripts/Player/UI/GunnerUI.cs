using System.Collections.Generic;
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
    public DamageIndicator damageIndicator;

    // Bars
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private RectTransform shieldBar;

    // Guns
    [SerializeField] private Sprite borderEquipped;
    [SerializeField] private Sprite borderUnequipped;
    [SerializeField] private List<Image> gunSlotBorders;
    [SerializeField] private List<Image> gunIcons;


    // Ammo count
    [SerializeField] private TextMeshProUGUI ammoCount;

    // Game state indicators
    [SerializeField] private TextMeshProUGUI energyCount;
    [SerializeField] private TextMeshProUGUI livingPlayerCount;
    [SerializeField] private TextMeshProUGUI killCount;

    // Other classes UIs
    public GameObject magicianUI;
    public GameObject weaponWheel;

    bool weaponWheelToggle = false;

    #endregion

    private void Start()
    {
        if (player != null && player.GetComponent<Identifier>().typePrefix != Identifier.gunnerType)
        {
            Debug.LogError("Displaying incorrect HUD");
        }

        weaponWheel.GetComponent<WeaponWheel>().ui = this;

        // Setting initial gun icons
        for (int i = 0; i < weapons.equippedWeapons.Count; i++)
        {
            gunIcons[i].sprite = weapons.equippedWeapons[i].uiEquippedBarImage;
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
        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    weaponWheel.SetActive(true);
        //    togglePauseMenu();
        //}
        if (Input.GetKeyDown(KeyCode.Q))
        {
            weaponWheelToggle = !weaponWheelToggle;
            weaponWheel.SetActive(weaponWheelToggle);
            togglePauseMenu();
        }

        //if (Input.GetKeyUp(KeyCode.Q))
        //{
        //    weaponWheel.SetActive(false);
        //    togglePauseMenu();
        //}
    }

    void showEquipped()
    {
        for (int i = 0; i < weapons.equippedWeapons.Count; i++)
        {
            gunSlotBorders[i].sprite = i == weapons.equippedWeapon ? borderEquipped : borderUnequipped;
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

    public void onWeaponPurchased(int oldWeaponEquippedIndex, WeaponType newWeapon)
    {
        gunIcons[oldWeaponEquippedIndex].sprite = newWeapon.uiEquippedBarImage;
    }
}