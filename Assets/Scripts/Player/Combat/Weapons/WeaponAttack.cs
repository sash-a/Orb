﻿using UnityEngine;
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

    public GameObject grenadeSpawn;
    public GameObject grenadePrefab;

    private ResourceManager resourceManager;

    void Start()
    {
        resourceManager = GetComponent<ResourceManager>();
        weapons = new List<WeaponType>();
        //damage, range, rate of fire
        WeaponType pistol = new WeaponType(5, 60, 5, PistolMuzzleFlash);
        WeaponType assault = new WeaponType(3, 70, 8, AssaultMuzzleFlash);
        WeaponType shotgun = new WeaponType(12, 30, 2, ShotgunMuzzleFlash);
        WeaponType sniper = new WeaponType(12, 350, 1, SniperMuzzleFlash);
        WeaponType grenade = new WeaponType(20, 30, 1, 3, 5);
        //needs to be added in the exact same order as the prefabs under player camera to work
        weapons.Add(pistol);
        weapons.Add(assault);
        weapons.Add(shotgun);
        weapons.Add(sniper);
        weapons.Add(grenade);
    }

    private void Update()
    {
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
    }

    [Command]
    public void CmdthrowGrenade()
    {
        float force = 40;
        GameObject grenade =
            Instantiate(grenadePrefab, grenadeSpawn.transform.position, grenadeSpawn.transform.rotation);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.AddForce(cam.transform.forward * force, ForceMode.VelocityChange);
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

    [Client]
    public override void attack()
    {
        if (!weapons[selectedWeapon].isExplosive)
        {
            //only works when the particle effect is dragged in directly from the gun's children for some reason 
            // its because: it can't be prefab needs to specifically be particle effect - will modify this later
            weapons[selectedWeapon].muzzleFlash.Play();

            //Relevant to ammo
            resourceManager.usePrimary(1);

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
                                                                                              //This just isn't called
                    if (hit.collider.GetComponent<NetHealth>().getHealth() <= 0)
                    {
                        CmdVoxelDestructionEffect(hit.point, hit.normal);
                    }


                    hit.collider.gameObject.GetComponent<Voxel>().lastHitRay = new Ray(cam.transform.position, cam.transform.forward);
                    hit.collider.gameObject.GetComponent<Voxel>().lastHitPosition = hit.point;


                }

            }
        }
        else
        {
            CmdthrowGrenade();
        }
    }


    public override void endAttack()
    {
        //        throw new System.NotImplementedException();
    }

    public override void secondaryAttack()
    {
        throw new System.NotImplementedException();
    }

    public override void endSecondaryAttack()
    {
        throw new System.NotImplementedException();
    }
}