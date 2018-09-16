using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponWheel : MonoBehaviour
{
    #region Variables

    public WeaponAttack weapons;
    public ResourceManager rm;
    public NetHealth playerHealth;
    public GunnerUI ui;

    public int healthCost;
    public int armorCost;

    #region Buttons

    public Button upg1;
    public Button ammo1;
    public Button upg2;
    public Button ammo2;
    public Button specialAmmo;
    public Button grenadeAmmo;
    public Button armour;
    public Button health;
    public Button buyLeft;
    public Button buyRight;

    #endregion

    #region Images

    public Image upgLeftImage;
    public Image ammoLeftImage;
    public Image upgRightImage;
    public Image ammoRightImage;
    public Image specialAmmoImage;

    public Image armourAmmoImage;
    public Image grenageAmmoImage;
    public Image healthImage;
    public Image buyLeftImage;
    public Image buyRightImage;

    public Image buyLeftIcon;
    public Image buyRightIcon;
    public Image equipLeftIcon;
    public Image equipRightIcon;

    #endregion

    #region Sprites

    public Sprite assaultRifleSprite;
    public Sprite pistolSprite;
    public Sprite shotGunSprite;
    public Sprite sniperSprite;

    #endregion

    #endregion

    float minThresh = 0.1f;

    private void Start()
    {
        weapons.equippedWeapons[1].ammoButton = ammo1;
        weapons.equippedWeapons[1].upgradeButton = upg1;
        weapons.equippedWeapons[1].uiWhealImage = equipLeftIcon;
        weapons.equippedWeapons[2].ammoButton = ammo2;
        weapons.equippedWeapons[2].upgradeButton = upg2;
        weapons.equippedWeapons[2].uiWhealImage = equipRightIcon;

        weapons.weapons[3].uiWhealImage = buyLeftIcon;
        weapons.weapons[4].uiWhealImage = buyRightIcon;

        weapons.equippedWeapons[1].ammoButton.GetComponentInChildren<TextMeshProUGUI>().text =
            "$" + weapons.equippedWeapons[1].ammunition.cost +
            " (" + weapons.equippedWeapons[1].ammunition.ammoPerPurchase + ")";

        weapons.equippedWeapons[1].upgradeButton.GetComponentInChildren<TextMeshProUGUI>().text =
            "$" + weapons.equippedWeapons[1].upgradeCost;


        weapons.equippedWeapons[2].ammoButton.GetComponentInChildren<TextMeshProUGUI>().text =
            "$" + weapons.equippedWeapons[2].ammunition.cost +
            " (" + weapons.equippedWeapons[2].ammunition.ammoPerPurchase + ")";

        weapons.equippedWeapons[2].upgradeButton.GetComponentInChildren<TextMeshProUGUI>().text =
            "$" + weapons.equippedWeapons[2].upgradeCost;

        health.GetComponentInChildren<TextMeshProUGUI>().text = "$" + healthCost;
        armour.GetComponentInChildren<TextMeshProUGUI>().text = "$" + armorCost;
        grenadeAmmo.GetComponentInChildren<TextMeshProUGUI>().text =
            "$" + weapons.grenade.ammunition.cost + "(" + weapons.grenade.ammunition.ammoPerPurchase + ")";


        specialAmmo.interactable = false;

        upgLeftImage.alphaHitTestMinimumThreshold = minThresh;
        ammoLeftImage.alphaHitTestMinimumThreshold = minThresh;
        upgRightImage.alphaHitTestMinimumThreshold = minThresh;
        ammoRightImage.alphaHitTestMinimumThreshold = minThresh;
        specialAmmoImage.alphaHitTestMinimumThreshold = minThresh;

        armourAmmoImage.alphaHitTestMinimumThreshold = minThresh;
        grenageAmmoImage.alphaHitTestMinimumThreshold = minThresh;
        healthImage.alphaHitTestMinimumThreshold = minThresh;
        buyLeftImage.alphaHitTestMinimumThreshold = minThresh;
        buyRightImage.alphaHitTestMinimumThreshold = minThresh;
    }

    /// <summary>
    /// Makes the special weapon button interactible.
    /// </summary>
    /// <param name="specialWeapon">The special weapon recieved</param>
    public void onRecieveSpecialWeapon(WeaponType specialWeapon)
    {
        specialAmmo.interactable = true;
        specialAmmo.GetComponentInChildren<TextMeshProUGUI>().text = specialWeapon.name + " ammo";
        specialAmmo.onClick.RemoveAllListeners();
        specialAmmo.onClick.AddListener(delegate { purchaseAmmo(specialWeapon.name); });
    }

    public void purchaseGun(string gunName)
    {
        WeaponType newWeapon = weapons.weapons.Find(w => w.name == gunName);
        var currentWeapon = weapons.equippedWeapons[weapons.equippedWeapon];

        if (rm.getEnergy() >= newWeapon.baseCost && currentWeapon.name != WeaponType.DIGGING_TOOL)
        {
            rm.useEnergy(newWeapon.baseCost);
            swapGun(newWeapon, currentWeapon);
            ui.onWeaponPurchased(weapons.equippedWeapon, newWeapon);
        }
    }

    /// <summary>
    /// Swaps old gun with new gun in the UI and in the gunners hands
    /// </summary>
    public void swapGun(WeaponType newWeapon, WeaponType oldWeapon)
    {
        if (weapons.equippedWeapons[weapons.equippedWeapon].name != oldWeapon.name)
        {
            Debug.LogError("Can't replace weapon as it is not equipped");
            return;
        }

        // Can't have 2 of the same weapon and can't replace digging tool or special weapon
        if (weapons.equippedWeapons.Contains(newWeapon) || oldWeapon.name == WeaponType.DIGGING_TOOL ||
            oldWeapon.isSpecial)
        {
            return;
        }

        // Equip new weapon
        weapons.equippedWeapons[weapons.equippedWeapon] = newWeapon;

        // Set the purchase button to purchase the old weapon
        Button b = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        var oldWeaponImg = oldWeapon.uiWhealImage;
        oldWeapon.uiWhealImage = newWeapon.uiWhealImage;
        oldWeapon.uiWhealImage.sprite = oldWeapon.uiWhealSprite;
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(delegate { purchaseGun(oldWeapon.name); });

        b.GetComponentInChildren<TextMeshProUGUI>().text = "$" + oldWeapon.baseCost; // this should be a tooltip

        // Setting the ammo/upgrade buttons
        newWeapon.ammoButton = oldWeapon.ammoButton;
        newWeapon.upgradeButton = oldWeapon.upgradeButton;
        newWeapon.uiWhealImage = oldWeaponImg;
        oldWeapon.ammoButton = null;
        oldWeapon.upgradeButton = null;

        newWeapon.ammoButton.GetComponentInChildren<TextMeshProUGUI>().text =
            "$" + newWeapon.ammunition.cost + "(" + newWeapon.ammunition.ammoPerPurchase + ")";
        newWeapon.uiWhealImage.sprite = newWeapon.uiWhealSprite;
        newWeapon.ammoButton.onClick.RemoveAllListeners();
        newWeapon.ammoButton.onClick.AddListener(delegate { purchaseAmmo(newWeapon.name); });

        newWeapon.ammoButton.GetComponentInChildren<TextMeshProUGUI>().text = "$" + newWeapon.upgradeCost;
        newWeapon.upgradeButton.onClick.RemoveAllListeners();
        newWeapon.upgradeButton.onClick.AddListener(delegate { purchaseUpgrade(newWeapon.name); });
    }

    /// <summary>
    /// Fills the health of the gunner.
    /// </summary>
    public void purchaseHealth()
    {
        if (rm.getEnergy() >= healthCost && playerHealth.getHealth() != playerHealth.maxHealth)
        {
            rm.useEnergy(healthCost);
            playerHealth.heal(playerHealth.maxHealth);
        }
    }

    /// <summary>
    /// Fills the armor of the gunner
    /// </summary>
    public void purchaseArmor()
    {
        if (rm.getEnergy() > armorCost)
        {
            rm.useEnergy(armorCost);
            playerHealth.RpcGetArmour(50);
        }
    }

    public void purchaseGrenade()
    {
        if (rm.getEnergy() >= weapons.grenade.ammunition.cost)
        {
            rm.useEnergy(weapons.grenade.ammunition.cost);
            weapons.grenade.ammunition.setNumGrenades
            (
                Mathf.Min
                (
                    weapons.grenade.ammunition.getMaxNumGrenades(),
                    weapons.grenade.ammunition.getNumGrenades() + weapons.grenade.ammunition.ammoPerPurchase
                )
            );
        }
    }

    /// <summary>
    /// Gives 2 magazines of ammo.
    /// </summary>
    /// <param name="gunName">Name of the gun</param>
    public void purchaseAmmo(string gunName)
    {
        WeaponType weapon = weapons.equippedWeapons.Find(w => w.name == gunName);
        if (weapon == null)
        {
            Debug.LogError("Gun is not equipped");
            return;
        }

        if (rm.getEnergy() >= weapon.ammunition.cost)
        {
            rm.useEnergy(weapon.ammunition.cost);
            weapon.ammunition.setPrimaryAmmo(weapon.ammunition.getPrimaryAmmo() + weapon.ammunition.ammoPerPurchase);
        }
    }

    public void purchaseUpgrade(string gunName)
    {
        WeaponType weapon = weapons.equippedWeapons.Find(w => w.name == gunName);
        if (weapon == null)
        {
            Debug.LogError("Gun is not equipped");
            return;
        }

        if (rm.getEnergy() >= weapon.upgradeCost)
        {
            rm.useEnergy(weapon.upgradeCost);
            weapon.upgrade();
        }
    }
}