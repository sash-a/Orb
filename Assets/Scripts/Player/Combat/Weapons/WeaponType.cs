using UnityEngine;
using UnityEngine.Networking;

[System.Serializable] // Allows unity to show in inspector
public class WeaponType
{
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
    int primaryAmmo;
    int magazineAmmo;
    int maxAmmo;
    public Ammo ammunition;

    //Special specific
    public bool isSpecial;

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
    public WeaponType(string name, float damage, float envDamage, float range, float fireRate, ParticleSystem muzzleFlash, int prA, int mgA, int mxA, int mgSz)
    {
        this.name = name;
        this.damage = damage;
        this.envDamage = envDamage;
        this.range = range;
        this.fireRate = fireRate;
        this.muzzleFlash = muzzleFlash;
        isExplosive = false;
        ammunition = new Ammo(prA, mgA, mxA, mgSz);
    }

    //grenade constructor (other attributes assigned in grenade script)
    public WeaponType(string name, int numGr, int maxNum)
    {
        this.name = name;
        isExplosive = true;
        ammunition = new Ammo(numGr, maxNum);
        fireRate = 1;
    }

    //crossbow constructor (damage attributes assigned in grenade script)
    public WeaponType(string name, float range, float fireRate, int prA, int mgA, int mxA, int mgSz)
    {
        this.name = name;
        this.range = range;
        this.fireRate = fireRate;
        isExplosive = true;
        ammunition = new Ammo(prA, mgA, mxA, mgSz);
        isSpecial = true;
    }

}