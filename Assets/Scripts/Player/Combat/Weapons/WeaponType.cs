using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable] // Allows unity to show in inspector
public class WeaponType
{
    #region WeaponNames

    public static readonly string DIGGING_TOOL = "digging tool";
    public static readonly string PISTOL = "pistol";
    public static readonly string RIFLE = "assault rifle";
    public static readonly string SHOTGUN = "shotgun";
    public static readonly string SNIPER = "sniper";
    public static readonly string EX_CROSSBOW = "Ex_crossbow";
    public static readonly string GRENADE = "grenade";
    public static readonly string EMPTY_SPECIAL = "Empty_Special";

    #endregion

    #region WeaponStats

    public Transform shootPos;

    public string name;
    public float damage;
    public float envDamage;

    public float range;

    //the greater the fireRate the less time between shots
    public float fireRate;

    //so we can fire immediatly when the game starts
    public float nextTimeToFire = 0f;

    public ParticleSystem muzzleFlash;
    public ParticleSystem digBeam;

    //grenade specific
    public bool isExplosive;
    int numGrenades;

    //ammo
    public Ammo ammunition;

    //Special specific
    public bool isSpecial;

    // UI
    public Button upgradeButton;
    public Button ammoButton;

    // Upgrade
    public int level;
    public int maxLevel = 3;
    public float modifier = 1;
    public int upgradeCost;
    public int baseCost;

    public static float headshotMultiplier = 1.5f;

    #endregion

    public Sprite uiEquippedBarImage;
    public Sprite uiWhealSprite;
    public Image uiWhealImage;

    //digging tool constructor 
    public WeaponType(string name, float damage, float envDamage, float range, float fireRate, ParticleSystem digBeam)
    {
        this.name = name;
        this.damage = damage;
        this.envDamage = envDamage;
        this.range = range;
        this.fireRate = fireRate;
        this.digBeam = digBeam;
        isSpecial = false;
    }

    //weapon constructor
    public WeaponType(string name, float damage, float envDamage, float range, float fireRate, int baseCost,
        int upgradeCost, ParticleSystem muzzleFlash, int prA, int mgA, int mxA, int mgSz, int ammoCost,
        int ammoPerPurchase)
    {
        this.name = name;
        this.damage = damage;
        this.envDamage = envDamage;
        this.range = range;
        this.fireRate = fireRate;
        this.baseCost = baseCost;
        this.upgradeCost = upgradeCost;
        this.muzzleFlash = muzzleFlash;
        isExplosive = false;
        ammunition = new Ammo(prA, mgA, mxA, mgSz, ammoCost, ammoPerPurchase);
    }

    //grenade constructor (other attributes assigned in grenade script)
    public WeaponType(string name, int numGr, int maxNum, int ammoCost, int ammoPerPurchase)
    {
        this.name = name;
        isExplosive = true;
        ammunition = new Ammo(numGr, maxNum, ammoCost, ammoPerPurchase);
        fireRate = 1;
    }

    //crossbow constructor (damage attributes assigned in grenade script)
    public WeaponType(string name, float range, float fireRate, int prA, int mgA, int mxA, int mgSz, int ammoCost,
        int ammoPerPurchase)
    {
        this.name = name;
        this.range = range;
        this.fireRate = fireRate;
        isExplosive = true;
        ammunition = new Ammo(prA, mgA, mxA, mgSz, ammoCost, ammoPerPurchase);
        isSpecial = true;
    }

    public void upgrade()
    {
        if (level >= maxLevel)
            return;

        level++;

        modifier *= Mathf.Pow(1.2f, level);
        upgradeCost *= (int) Mathf.Pow(2, level);


        // TODO all relevant variables * modifier
    }
}