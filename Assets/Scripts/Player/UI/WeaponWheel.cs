using UnityEngine;
using UnityEngine.UI;

public class WeaponWheel : MonoBehaviour
{
    #region Variables

    public WeaponAttack weapons;
    public ResourceManager rm;
    public NetHealth playerHealth;

    public int healthCost;
    public int armorCost;

    #region Buttons

    public Button upg1;
    public Button ammo1;
    public Button upg2;
    public Button ammo2;
    public Button specialAmmo;

    #endregion

    #endregion
    
    private void Start()
    {
        weapons.weaponWheel = this;
        
        weapons.equippedWeapons[1].ammoButton = ammo1;
        weapons.equippedWeapons[1].upgradeButton = upg1;
        weapons.equippedWeapons[2].ammoButton = ammo2;
        weapons.equippedWeapons[2].upgradeButton = upg2;

        specialAmmo.interactable = false;
    }

    /// <summary>
    /// Makes the special weapon button interactible.
    /// </summary>
    /// <param name="specialWeapon">The special weapon recieved</param>
    public void onRecieveSpecialWeapon(WeaponType specialWeapon)
    {
        specialAmmo.interactable = true;
        specialAmmo.GetComponentInChildren<Text>().text = specialWeapon.name + " ammo";
        specialAmmo.onClick.RemoveAllListeners();
        specialAmmo.onClick.AddListener(delegate { purchaseAmmo(specialWeapon.name); });
    }
    
    public void purchaseGun(string gunName)
    {
        WeaponType newWeapon = weapons.weapons.Find(w => w.name == gunName);
        if (rm.getEnergy() >= newWeapon.baseCost)
        {
            rm.useEnergy(newWeapon.baseCost);
            swapGun(newWeapon, weapons.equippedWeapons[weapons.equippedWeapon]);
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
        if (weapons.equippedWeapons.Contains(newWeapon)
            || oldWeapon.name == WeaponType.DIGGING_TOOL
            || oldWeapon.isSpecial)
        {
            return;
        }

        // Set weapon to purchasable position in weapon wheel
        weapons.equippedWeapons[weapons.equippedWeapon] = newWeapon;

        // Set the purchase button to purchase the old weapon
        Button b = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        b.GetComponentInChildren<Text>().text = oldWeapon.name;
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(delegate { purchaseGun(oldWeapon.name); });
        
        // Setting the ammo/upgrade buttons
        newWeapon.ammoButton = oldWeapon.ammoButton;
        newWeapon.upgradeButton = oldWeapon.upgradeButton;
        oldWeapon.ammoButton = null;
        oldWeapon.upgradeButton = null;
        
        newWeapon.ammoButton.GetComponentInChildren<Text>().text = newWeapon.name + " ammo";
        newWeapon.ammoButton.onClick.RemoveAllListeners();
        newWeapon.ammoButton.onClick.AddListener(delegate { purchaseAmmo(newWeapon.name); });
        
        newWeapon.upgradeButton.GetComponentInChildren<Text>().text = "Upgrade " + newWeapon.name;
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