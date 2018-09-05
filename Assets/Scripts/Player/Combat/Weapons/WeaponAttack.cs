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
    public GameObject hitEffect;
    public GameObject explosionEffect;
    public EnergyBlockEffectSpawner energyBlockEffectSpawner;

    public int selectedWeapon = 0;

    public int equippedWeapon = 0;

    //List of all weapons in the game
    public List<WeaponType> weapons;
    public WeaponType grenade;

    //List of all currently equipped weapons
    public List<WeaponType> equippedWeapons;

    // Grenade specific
    public GameObject grenadeSpawn;
    public GameObject grenadePrefab;
    public float throwForce = 40;

    // Special Weapon specific:
    public GameObject Ex_boltSpawn;
    public GameObject boltPrefab;
    public float boltForce = 200;

    // Animation
    public Animator animator;
    private bool isCarryingPistol = true;
    private bool isReloading = false;
    private bool isThrowingGrenade = false;
    private bool isShooting = false;

    public WeaponWheel weaponWheel;

    void Start()
    {
        resourceManager = GetComponent<ResourceManager>();
        energyBlockEffectSpawner = GetComponent<EnergyBlockEffectSpawner>();

        equippedWeapons.Add(weapons[0]);
        equippedWeapons.Add(weapons[1]);

        equippedWeapons.Add(weapons[2]);
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
        if (scroll < 0f && isLocalPlayer) //need to prevent weapon switching when aiming!
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
        if (scroll > 0f && isLocalPlayer)
        {
            if (equippedWeapon <= 0)
            {
                equippedWeapon = equippedWeapons.Count - 1;
            }
            else
            {
                equippedWeapon--;
            }
        }

        //Find the correct selected weapon in weapons (causing errors?)
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i].name == equippedWeapons[equippedWeapon].name)
            {
                selectedWeapon = i;
                break;
            }
        }

        if (Input.GetButton("Fire1") && Time.time >= weapons[selectedWeapon].nextTimeToFire && !isReloading &&
            !isThrowingGrenade)
        {
            weapons[selectedWeapon].nextTimeToFire = Time.time + 1f / weapons[selectedWeapon].fireRate;
            attack();
            isShooting = true;
        }
        else
        {
            isShooting = false;
        }

        if (Input.GetKey(KeyCode.R) && weapons[selectedWeapon].name != WeaponType.DIGGING_TOOL)
        {
            //Debug.Log("Reload!");
            StartCoroutine(Reload(weapons[selectedWeapon].ammunition));
        }

        //NB: This method will only work if grenades is last item in weapons array
        if (Input.GetKeyUp(KeyCode.G) && !isShooting && !isReloading)
        {
            if (grenade.ammunition.getNumGrenades() > 0)
            {
                //wait for grenade animation to reach apex of throw
                isThrowingGrenade = true;
                StartCoroutine(wait(1.50f));
                //Spawn Grenade
                CmdthrowGrenade();
                resourceManager.useGrenade(1, grenade.ammunition);
                //Wait for rest of animation to finish
                StartCoroutine(wait(1.80f));
                isThrowingGrenade = false;
            }
        }

        Animation();
        //Debug.Log(isThrowingGrenade);
    }

    void Animation()
    {
        if (weapons[selectedWeapon].name == WeaponType.DIGGING_TOOL ||
            weapons[selectedWeapon].name == WeaponType.PISTOL)
        {
            isCarryingPistol = true;
        }
        else
        {
            isCarryingPistol = false;
        }

        animator.SetBool("isCarryingPistol", isCarryingPistol);
        animator.SetBool("isReloading", isReloading);
        animator.SetBool("isThrowingGrenade", isThrowingGrenade);
        animator.SetBool("isShooting", isShooting);
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

    IEnumerator wait(float time)
    {
        yield return new WaitForSeconds(time);
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

    IEnumerator Reload(Ammo A)
    {
        if (A.getMagAmmo() != A.getMagSize() && A.getPrimaryAmmo() != 0)
        {
            resourceManager.reloadMagazine(A.getMagSize() - A.getMagAmmo(), A);

            isReloading = true;
            //wait length of animation (3.3 seconds)
            yield return new WaitForSeconds(3.3f);
            isReloading = false;
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

        //Crossbow
        if (weapons[selectedWeapon].name == WeaponType.EX_CROSSBOW)
        {
            CmdShootBolt();
            resourceManager.useMagazineAmmo(1, weapons[selectedWeapon].ammunition);
            return;
        }

        if (!weapons[selectedWeapon].isExplosive && weapons[selectedWeapon].name == WeaponType.DIGGING_TOOL ||
            weapons[selectedWeapon].ammunition.getMagAmmo() > 0)
        {
            // FX
            if (weapons[selectedWeapon].name != WeaponType.DIGGING_TOOL && !weapons[selectedWeapon].isExplosive)
            {
                //only works when the particle effect is dragged in directly from the gun's children for some reason 
                //its because: it can't be prefab needs to specifically be particle effect
                weapons[selectedWeapon].muzzleFlash.Play();

                //Relevant to ammo
                resourceManager.useMagazineAmmo(1, weapons[selectedWeapon].ammunition);
            }
            else if (weapons[selectedWeapon].name == WeaponType.DIGGING_TOOL)
            {
                // this isnt working for some reason
                // weapons[selectedWeapon].digBeam.Play();
            }

            // Shooting
            // Shooting ray from camera
            RaycastHit hitFromCam;
            if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out hitFromCam,
                weapons[selectedWeapon].range, mask))
                return;

            RaycastHit hitFromGun;
            if (!Physics.Linecast(weapons[selectedWeapon].shootPos.position, hitFromCam.point, out hitFromGun, mask))
                return; // Should never return

            // if we hit a player
            if (hitFromGun.collider.tag == PLAYER_TAG)
            {
                CmdPlayerAttacked(hitFromGun.collider.name, weapons[selectedWeapon].damage);
            }
            else //if not a player
            {
                if (weapons[selectedWeapon].isSpecial != true)
                {
                    CmdObjectHitEffect(hitFromGun.point, hitFromGun.normal);
                }
            }


            if (hitFromGun.collider.gameObject.tag == VOXEL_TAG)
            {
                var voxel = hitFromGun.collider.GetComponent<Voxel>();

                CmdVoxelDamaged(hitFromGun.collider.gameObject, weapons[selectedWeapon].envDamage);

                if (!voxel.hasEnergy && hitFromGun.collider.GetComponent<NetHealth>().getHealth() <= 0)
                {
                    // TODO: Play voxel destruction effect    
                }

                // Spawn energy blocks if shooting energy voxel
                if (voxel.hasEnergy)
                {
                    energyBlockEffectSpawner.setVoxel(voxel.gameObject);
                    energyBlockEffectSpawner.spawnBlock();
                }

                // What is this!? (Sasha) <- dont know either, shane coded this (liron)
                voxel.lastHitRay = new Ray(cam.transform.position, cam.transform.forward);
                voxel.lastHitPosition = hitFromGun.point;
            }

            if (hitFromGun.collider.CompareTag("Shield"))
            {
                CmdShieldHit(hitFromGun.collider.gameObject, weapons[selectedWeapon].damage);
            }
        }
        else
        {
            //can play a sound or something (empty mag)
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