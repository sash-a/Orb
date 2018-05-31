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
    public float delay;
    public float countdown;
    public float blastRadius;
    public bool hasExploded;
    public bool isExplosive;

    //public int weaponLevel = 1;
    

    public WeaponType(float damage, float range, float fireRate, ParticleSystem muzzleFlash)
    {
        this.damage = damage;
        this.range = range;
        this.fireRate = fireRate;
        this.muzzleFlash = muzzleFlash;
        this.isExplosive = false;
    }

    //grenade constructor
    public WeaponType(float damage, float range, float fireRate, float delay, float blastRadius)
    {
        this.damage = damage;
        this.range = range;
        this.fireRate = fireRate;
        this.delay = delay;
        this.countdown = delay;
        this.blastRadius = blastRadius;
        this.isExplosive = true;
        hasExploded = false;
    }

    // isExplosive = false; float blast radius; or some implementation like this

    // These will likely be stored as a list in player
    // Store effects in here too like bullet impact gun smoke and that kinda shit
    // Impact effect and impact force and muzzle flash etc?
    // Great tutorial for effects and shit: https://www.youtube.com/watch?v=THnivyG0Mvo
}