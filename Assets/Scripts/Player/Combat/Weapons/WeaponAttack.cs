using UnityEngine;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
//collab is a cunt

[RequireComponent(typeof(ResourceManager))]
public class WeaponAttack : AAttackBehaviour
{
    public ParticleSystem PistolMuzzleFlash;
    public ParticleSystem AssaultMuzzleFlash;
    public ParticleSystem ShotgunMuzzleFlash;
    public ParticleSystem SniperMuzzleFlash;
    public ParticleSystem DiggingBeam;

    public GameObject hitEffect;
    public GameObject explosionEffect;

    public int selectedWeapon = 0;
    public int equippedWeapon = 0;
    //List of all weapons in the game
    public List<WeaponType> weapons;
    //List of all currently equipped weapons
    public List<WeaponType> equippedWeapons;

    //grenade specific
    public GameObject grenadeSpawn;
    public GameObject grenadePrefab;
    public float throwForce = 40;

    //Special Weapon specific:
    public GameObject Ex_boltSpawn;
    public GameObject boltPrefab;
    public float boltForce = 200;

    void Start()
    {
        resourceManager = GetComponent<ResourceManager>();

        //List of all weapons in the game
        weapons = new List<WeaponType>();
        //name, damage, envDamage, range, fireRate, muzzleFlashEffect, primaryAmmo, currentMagAmmo, maxAmmo, MagSize
        //normal weapons:
        WeaponType diggingTool = new WeaponType("digging tool", 1, 15, 20, 30, DiggingBeam);
        WeaponType pistol = new WeaponType("pistol", 5, 5, 60, 5, PistolMuzzleFlash, 20, 12, 300, 12);
        WeaponType assault = new WeaponType("assault rifle", 3, 3, 70, 8, AssaultMuzzleFlash, 30000, 30, 500, 30); //pA origonally 300
        WeaponType shotgun = new WeaponType("shotgun", 12, 12, 30, 2, ShotgunMuzzleFlash, 1000, 6, 300, 6); //pA origonally 100
        WeaponType sniper = new WeaponType("sniper", 12, 12, 350, 1, SniperMuzzleFlash, 60, 12, 100, 12);
        //Special weapons:
        WeaponType Ex_crossbow = new WeaponType("Ex_crossbow", 60, 1, 8, 20, 40, 20);
        //number of current grenades, grenade capacity
        WeaponType grenade = new WeaponType("grenade", 6, 6);
        //needs to be added in the exact same order as the prefabs under player camera to work NB!!!
        weapons.Add(diggingTool);
        weapons.Add(pistol);
        weapons.Add(assault);
        weapons.Add(shotgun);
        weapons.Add(sniper);
        weapons.Add(Ex_crossbow);
        weapons.Add(grenade);

        equippedWeapons.Add(diggingTool);
        equippedWeapons.Add(pistol);
        //equippedWeapons.Add(assault);
        //equippedWeapons.Add(shotgun);
        //equippedWeapons.Add(sniper);
        equippedWeapons.Add(Ex_crossbow);
        //equippedWeapons.Add(grenade);
    }

    private void Update()
    {
        if (PlayerUI.isPaused) return;

        if (Input.GetKey(KeyCode.Alpha1))
        {
            equippedWeapon = 0;
        }

        if (Input.GetKey(KeyCode.Alpha2))
        {
            equippedWeapon = 1;
        }

        if (Input.GetKey(KeyCode.Alpha3))
        {
            equippedWeapon = 2;
        }

        var scroll = Input.GetAxis("Mouse ScrollWheel");
        //scroll up changes weapons
        if (scroll > 0f && isLocalPlayer) //need to prevent weapon switching when aiming!
        {
            if (equippedWeapon >= equippedWeapons.Count - 1)
            {
                equippedWeapon = 0;
            }
            else
            {
                equippedWeapon++;
            }
        }

        //scroll down changes weapons
        if (scroll < 0f && isLocalPlayer)
        {
            if (equippedWeapon <= 0)
            {
                equippedWeapon = weapons.Count - 1;
            }
            else
            {
                equippedWeapon--;
            }
        }

        //Find the correct selected weapon in weapons (causing errors?)
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i] == equippedWeapons[equippedWeapon])
            {
                selectedWeapon = i;
            }
        }

        if (Input.GetButton("Fire1") && Time.time >= weapons[selectedWeapon].nextTimeToFire)
        {
            weapons[selectedWeapon].nextTimeToFire = Time.time + 1f / weapons[selectedWeapon].fireRate;
            attack();
        }

        if (Input.GetKey(KeyCode.R) && weapons[selectedWeapon].name != "digging tool")
        {
            //Debug.Log("Reload!");
            Reload(weapons[selectedWeapon].ammunition);
        }

        //NB: This method will only work if grenades is last item in weapons array
        if (Input.GetKeyUp(KeyCode.G))
        {
            if (weapons[weapons.Count - 1].ammunition.getNumGrenades() > 0)
            {
                CmdthrowGrenade();
                resourceManager.useGrenade(1, weapons[weapons.Count - 1].ammunition);
            }
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
    public void CmdShootBolt()
    {
        GameObject Ex_bolt =
            Instantiate(boltPrefab, Ex_boltSpawn.transform.position, Ex_boltSpawn.transform.rotation);
        Rigidbody rb = Ex_bolt.GetComponent<Rigidbody>();
        rb.AddForce(cam.transform.forward * boltForce, ForceMode.VelocityChange);
        NetworkServer.Spawn(Ex_bolt);
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
    }

    [Client]
    public override void attack()
    {
        if (!MapManager.manager.mapDoneLocally)
        {
            Debug.LogError("attacking before map finished");
            return;
        }

        if (!weapons[selectedWeapon].isExplosive && weapons[selectedWeapon].name == "digging tool" || weapons[selectedWeapon].ammunition.getMagAmmo() > 0)
        {
            if (weapons[selectedWeapon].name != "digging tool" && !weapons[selectedWeapon].isExplosive)
            {
                //only works when the particle effect is dragged in directly from the gun's children for some reason 
                //its because: it can't be prefab needs to specifically be particle effect - will modify this later
                weapons[selectedWeapon].muzzleFlash.Play();

                //Relevant to ammo
                resourceManager.useMagazineAmmo(1, weapons[selectedWeapon].ammunition);
            }
            else if (weapons[selectedWeapon].name == "digging tool")
            {
                // this isnt working for some reason
                // weapons[selectedWeapon].digBeam.Play();
            }


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
                    if (weapons[selectedWeapon].isSpecial != true)
                    {
                        CmdObjectHitEffect(hit.point, hit.normal);
                    }   
                }


                if (hit.collider.gameObject.tag == VOXEL_TAG)
                {
                    //Debug.Log("weapon hit voxel ("+ hit.collider.gameObject .name+ ") at layer " + hit.collider.gameObject.GetComponent<Voxel>().layer);
                    CmdVoxelDamaged(hit.collider.gameObject, weapons[selectedWeapon].envDamage); // envDamage = environment damage

                    if (hit.collider.GetComponent<NetHealth>().getHealth() <= 0)
                    {
                        //Play voxel destruction effect    
                    }


                    hit.collider.gameObject.GetComponent<Voxel>().lastHitRay =
                        new Ray(cam.transform.position, cam.transform.forward);
                    hit.collider.gameObject.GetComponent<Voxel>().lastHitPosition = hit.point;
                }
            }
        }
        else
        {
            //can play a sound or something (empty mag)
        }

        //Crossbow
        if (weapons[selectedWeapon].name == "Ex_crossbow")
        {
            CmdShootBolt();
            resourceManager.useMagazineAmmo(1, weapons[selectedWeapon].ammunition);
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