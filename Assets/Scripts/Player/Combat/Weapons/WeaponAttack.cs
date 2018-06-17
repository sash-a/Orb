using UnityEngine;
using UnityEngine.Networking;

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(ResourceManager))]
public class WeaponAttack : AAttackBehaviour
{
    //[SerializeField] private WeaponType WeaponType;
    //[SerializeField] private GameObject gunModel;

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
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
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
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
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
            if (!weapons[selectedWeapon].isExplosive)
            {
                attack();
            }
            else
            {
                throwGrenade();
            }

            
            
        }
    }

    public void throwGrenade()
    {
        float force = 40;
        GameObject grenade = Instantiate(grenadePrefab, grenadeSpawn.transform.position, grenadeSpawn.transform.rotation);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.AddForce(cam.transform.forward * force, ForceMode.VelocityChange);
    }

    [Client]
    public override void attack()
    {
        if (!weapons[selectedWeapon].isExplosive)
        {
            //only works when the particle effect is dragged in directly from the gun's children for some reason 
            weapons[selectedWeapon].muzzleFlash.Play();

            //Relevant to ammo
            resourceManager.usePrimary(1);

            //hit is the object that is hit (or not hit)
            RaycastHit hit;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, weapons[selectedWeapon].range, mask))
            {
                //if we hit a player
                if (hit.collider.tag == PLAYER_TAG)
                {
                    CmdPlayerAttacked(hit.collider.name, weapons[selectedWeapon].damage);
                }
                else //if not a player
                {
                    if (isServer)
                    {
                        GameObject hitParticle = Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                        NetworkServer.Spawn(hitParticle);
                    }
                }


                // Only add this if we are sure that voxels are getting damaged by guns otherwise check gun type before damaging
                if (hit.collider.tag == VOXEL_TAG)
                {
                    CmdVoxelDamaged(hit.collider.gameObject, weapons[selectedWeapon].damage); // weapontype.envDamage?

                    //This just isn't called
                    if (hit.collider.GetComponent<NetHealth>().getHealth() <= 0)
                    {
                        if (isServer)
                        {
                            GameObject VoxelParticle = Instantiate(VoxelDestroyEffect, hit.point, Quaternion.LookRotation(hit.normal));
                            NetworkServer.Spawn(VoxelParticle);
                        }
                    }
                }

            }
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