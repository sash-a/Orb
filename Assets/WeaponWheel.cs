using UnityEngine;
using UnityEngine.UI;

public class WeaponWheel : MonoBehaviour
{
    public WeaponAttack weapons;
    public ResourceManager rm;
    public NetHealth playerHealth;

    public int healthCost;
    public int armorCost;

    public Button upg1;
    public Button ammo1;
    public Button upg2;
    public Button ammo2;

    private void Start()
    {
        weapons.equippedWeapons[1].ammoButton = ammo1;
        weapons.equippedWeapons[1].upgradeButton = upg1;
        weapons.equippedWeapons[2].ammoButton = ammo2;
        weapons.equippedWeapons[2].upgradeButton = upg2;

    }

    public void purchaseGun(string gunName)
    {
        WeaponType newWeapon = weapons.weapons.Find(w => w.name == gunName);
        swapGun(newWeapon, weapons.equippedWeapons[weapons.equippedWeapon]);

        // TODO Cost etc
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

        // Can't have 2 of the same weapon
        if (weapons.equippedWeapons.Contains(newWeapon))
            return;

        // Set weapon to purchasable position in weapon wheel
        weapons.equippedWeapons[weapons.equippedWeapon] = newWeapon;

        // Set the purchase button to purchase the old weapon
        Button b = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        
        // Setting the purchase button
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
        if (rm.getEnergy() > healthCost)
        {
            rm.useEnergy(healthCost);
            playerHealth.setInitialHealth(playerHealth.maxHealth);
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
            // TODO give armor
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
        
        // TODO price
        weapon.ammunition.setPrimaryAmmo(weapon.ammunition.getPrimaryAmmo() + weapon.ammunition.getMagSize() * 2);
    }

    public void purchaseUpgrade(string gunName)
    {
        WeaponType weapon = weapons.equippedWeapons.Find(w => w.name == gunName);
        if (weapon == null)
        {
            Debug.LogError("Gun is not equipped");
            return;
        }
        
        // weapon.upgrade();
    }
    
}