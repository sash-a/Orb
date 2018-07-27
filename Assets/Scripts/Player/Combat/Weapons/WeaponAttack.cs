using UnityEngine;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(ResourceManager))]
public class WeaponAttack : AAttackBehaviour
{
    public ParticleSystem PistolMuzzleFlash;
    public ParticleSystem AssaultMuzzleFlash;
    public ParticleSystem ShotgunMuzzleFlash;
    public ParticleSystem SniperMuzzleFlash;

    public GameObject hitEffect;
    public GameObject VoxelDestroyEffect;
    public GameObject explosionEffect;

    private int selectedWeapon = 0;

    List<WeaponType> weapons;

    //grenade specific
    public GameObject grenadeSpawn;
    public GameObject grenadePrefab;
    public float throwForce = 40;

    private ResourceManager resourceManager;

    void Start()
    {
        resourceManager = GetComponent<ResourceManager>();
        weapons = new List<WeaponType>();
        //damage, range, fireRate, muzzleFlashEffect, primaryAmmo, currentMagAmmo, maxAmmo, MagSize
        WeaponType pistol = new WeaponType(5, 60, 5, PistolMuzzleFlash, 20, 12, 300, 12);
        WeaponType assault = new WeaponType(3, 70, 8, AssaultMuzzleFlash, 30000, 30, 500, 30); //pA origonally 300
        WeaponType shotgun = new WeaponType(12, 30, 2, ShotgunMuzzleFlash, 1000, 6, 300, 6); //pA origonally 100
        WeaponType sniper = new WeaponType(12, 350, 1, SniperMuzzleFlash, 60, 12, 100, 12);
        //number of current grenades, grenade capacity
        WeaponType grenade = new WeaponType(3, 5);
        //needs to be added in the exact same order as the prefabs under player camera to work NB!!!
        weapons.Add(pistol);
        weapons.Add(assault);
        weapons.Add(shotgun);
        weapons.Add(sniper);
        weapons.Add(grenade);
    }

    private void Update()
    {
        if (PlayerUI.isPaused) return;

        if (Input.GetKey(KeyCode.Alpha1))
        {
            selectedWeapon = 0;
        }

        if (Input.GetKey(KeyCode.Alpha2))
        {
            selectedWeapon = 1;
        }

        if (Input.GetKey(KeyCode.Alpha3))
        {
            selectedWeapon = 2;
        }

        if (Input.GetKey(KeyCode.Alpha4))
        {
            selectedWeapon = 3;
        }

        if (Input.GetKey(KeyCode.Alpha5))
        {
            selectedWeapon = 4;
        }

        //scroll up changes weapons
        if (Input.GetAxis("Mouse ScrollWheel") > 0f && isLocalPlayer) //need to prevent weapon switching when aiming!
        {
            if (selectedWeapon >= weapons.Count - 1)
            {
                selectedWeapon = 0;
            }
            else
            {
                selectedWeapon++;
            }
        }

        //scroll down changes weapons
        if (Input.GetAxis("Mouse ScrollWheel") < 0f && isLocalPlayer)
        {
            if (selectedWeapon <= 0)
            {
                selectedWeapon = weapons.Count - 1;
            }
            else
            {
                selectedWeapon--;
            }
        }

        if (Input.GetButton("Fire1") && Time.time >= weapons[selectedWeapon].nextTimeToFire)
        {
            weapons[selectedWeapon].nextTimeToFire = Time.time + 1f / weapons[selectedWeapon].fireRate;
            attack();
        }

        if (Input.GetKey(KeyCode.R))
        {
            //Debug.Log("Reload!");
            Reload(weapons[selectedWeapon].ammunition);
        }
    }

    [Command]
    public void CmdthrowGrenade()
    {
        GameObject grenade =
            Instantiate(grenadePrefab, grenadeSpawn.transform.position, grenadeSpawn.transform.rotation);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.AddForce(cam.transform.forward * throwForce, ForceMode.VelocityChange);
        NetworkServer.Spawn(grenade);
    }

    [Command]
    private void CmdVoxelDestructionEffect(Vector3 position, Vector3 normal)
    {
        GameObject VoxelParticle = Instantiate(VoxelDestroyEffect, position,
            Quaternion.LookRotation(normal));
        NetworkServer.Spawn(VoxelParticle);
    }

    [Command]
    private void CmdObjectHitEffect(Vector3 position, Vector3 normal)
    {
        GameObject hitParticle = Instantiate(hitEffect, position, Quaternion.LookRotation(normal));
        NetworkServer.Spawn(hitParticle);
    }

    public void Reload(Ammo A)
    {
        if (A.getMagAmmo() != A.getMagSize() && A.getPrimaryAmmo() != 0)
        {
            resourceManager.reloadMagazine(A.getMagSize() - A.getMagAmmo(), A);
        }

        Debug.Log("Primary Ammo: " + A.getPrimaryAmmo());
    }

    [Client]
    public override void attack()
    {
        if (!MapManager.manager.mapDoneLocally)
        {
            Debug.LogError("attacking before map finished");
            return;
        }

        if (!weapons[selectedWeapon].isExplosive && weapons[selectedWeapon].ammunition.getMagAmmo() > 0)
        {
            //only works when the particle effect is dragged in directly from the gun's children for some reason 
            //its because: it can't be prefab needs to specifically be particle effect - will modify this later
            weapons[selectedWeapon].muzzleFlash.Play();

            //Relevant to ammo
            resourceManager.useMagazineAmmo(1, weapons[selectedWeapon].ammunition);
            //Debug.Log(weapons[selectedWeapon].ammunition.getMagAmmo());


            //hit is the object that is hit (or not hit)
            RaycastHit hit;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, weapons[selectedWeapon].range,
                mask))
            {
                //Debug.Log("weapon hit something");
                //if we hit a player
                if (hit.collider.tag == PLAYER_TAG)
                {
                    CmdPlayerAttacked(hit.collider.name, weapons[selectedWeapon].damage);
                }
                else //if not a player
                {
                    CmdObjectHitEffect(hit.point, hit.normal);
                }


                // Only add this if we are sure that voxels are getting damaged by guns otherwise check gun type before damaging
                if (hit.collider.gameObject.tag == VOXEL_TAG)
                {
                    //Debug.Log("weapon hit voxel ("+ hit.collider.gameObject .name+ ") at layer " + hit.collider.gameObject.GetComponent<Voxel>().layer);
                    CmdVoxelDamaged(hit.collider.gameObject, weapons[selectedWeapon].damage); // weapontype.envDamage?

                    if (hit.collider.GetComponent<NetHealth>().getHealth() <= 0)
                    {
                        CmdVoxelDestructionEffect(hit.point, hit.normal);
                    }


                    hit.collider.gameObject.GetComponent<Voxel>().lastHitRay =
                        new Ray(cam.transform.position, cam.transform.forward);
                    hit.collider.gameObject.GetComponent<Voxel>().lastHitPosition = hit.point;
                }
            }
        }
        else if (weapons[selectedWeapon].isExplosive && weapons[selectedWeapon].ammunition.getNumGrenades() > 0)
        {
            //Can only throw one grenade now!? <- HAVE NO IDEA WHY!?!?!? 
            Debug.Log("Grenade Thrown");
            CmdthrowGrenade();
            resourceManager.useGrenade(1, weapons[selectedWeapon].ammunition);
        }
        else
        {
            Debug.Log("Soz, out of ammo bud!");
            //can play a sound or something (empty mag)
        }

        //even this only prints once, WHAT IS GOING ON!!!
        if (selectedWeapon == 4)
        {
            Debug.Log("G");
        }
    }


    public override void endAttack()
    {
        //throw new System.NotImplementedException();
    }

    public override void secondaryAttack()
    {
        if (!MapManager.manager.mapDoneLocally)
        {
            Debug.LogError("attacking before map finished");
            return;
        }
    }

    public override void endSecondaryAttack()
    {
        throw new System.NotImplementedException();
    }
}