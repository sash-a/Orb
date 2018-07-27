using UnityEngine;
using UnityEngine.Networking;

[System.Serializable] // Allows unity to show in inspector
public class WeaponType : Item
{

    public float damage;
    public float range;
    //the greater the fireRate the less time between shots
    public float fireRate;
    //so we can fire immediatly when the game starts
    public float nextTimeToFire = 0f;
    public ParticleSystem muzzleFlash;

    //grenade specific
    public bool isExplosive;
    int numGrenades;

    //ammo
    int primaryAmmo;
    int magazineAmmo;
    int maxAmmo;
    public Ammo ammunition;
    

    public WeaponType(float damage, float range, float fireRate, ParticleSystem muzzleFlash, int prA, int mgA, int mxA, int mgSz)
    {
        this.damage = damage;
        this.range = range;
        this.fireRate = fireRate;
        this.muzzleFlash = muzzleFlash;
        isExplosive = false;
        ammunition = new Ammo(prA, mgA, mxA, mgSz);
    }

    //grenade constructor (attributes assigned in grenade script)
    public WeaponType(int numGr, int maxNum)
    {
        isExplosive = true;
        ammunition = new Ammo(numGr, maxNum);
        fireRate = 1;
    }

    // These will likely be stored as a list in player
    // Store effects in here too like bullet impact gun smoke and that kinda shit
    // Impact effect and impact force and muzzle flash etc?
    // Great tutorial for effects and shit: https://www.youtube.com/watch?v=THnivyG0Mvo
}