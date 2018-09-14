﻿using UnityEngine;
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
    public GameObject damageTextIndicatorEffect;
    public PlayerController player;


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

    // Special Weapon specific:
    public GameObject Ex_boltSpawn;
    public GameObject boltPrefab;
    public float boltForce = 200;

    // Animation:
    public Animator animator;
    private bool isCarryingPistol = true;
    private bool isReloading = false;
    private bool isThrowingGrenade = false;
    private bool isShooting = false;
    private bool isAiming = false;

    float camAngle = 60;
    float camZoomSpeed = 0.5f;

    float grenadeHoldLength = 0;

    //Sound:
    public AudioSource audioSource;

    public WeaponWheel weaponWheel;

    void Start()
    {
        resourceManager = GetComponent<ResourceManager>();
        //energyBlockEffectSpawner = GetComponent<EnergyBlockEffectSpawner>();
        player = GetComponent<PlayerController>();
        equippedWeapons.Add(weapons[0]); // Digger
        equippedWeapons.Add(weapons[1]); // Pistol
        equippedWeapons.Add(weapons[2]); // Assalt rifle
        equippedWeapons.Add(weapons[6]); // Empty special
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
        if (scroll < 0f && isLocalPlayer) 
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

        //Find the correct selected weapon in weapons
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i].name == equippedWeapons[equippedWeapon].name)
            {
                selectedWeapon = i;
                break;
            }
        }

        //Attacking
        if (Input.GetButton("Fire1") && Time.time >= weapons[selectedWeapon].nextTimeToFire && !isReloading &&
            !isThrowingGrenade && !Input.GetKey(KeyCode.LeftShift))
        {
            weapons[selectedWeapon].nextTimeToFire = Time.time + 1f / weapons[selectedWeapon].fireRate;
            attack();
        }

        //Aiming
        int idealCamAngle;
        float idealLookSensitivity;

        if (Input.GetButton("Fire2") && !isReloading && !Input.GetKey(KeyCode.LeftShift) && weapons[selectedWeapon].name != WeaponType.SNIPER)
        {
            isAiming = true;
            idealCamAngle = 40;
            idealLookSensitivity = player.lookSensitivityBase * 0.28f;

        }
        else
        {
            isAiming = false;
            idealCamAngle = 65;
            idealLookSensitivity = player.lookSensitivityBase;
        }

        camAngle = camAngle + (idealCamAngle - camAngle) * camZoomSpeed;
        Camera.main.fieldOfView = camAngle;
        player.lookSens += (idealLookSensitivity - player.lookSens) * camZoomSpeed;

        //Debug.Log("cam angle: " + cam.fieldOfView);

        //Reload
        if (Input.GetKey(KeyCode.R) && weapons[selectedWeapon].name != WeaponType.DIGGING_TOOL)
        {
            //Debug.Log("Reload!");
            StartCoroutine(Reload(weapons[selectedWeapon].ammunition));
        }

        //NB: This method will only work if grenades is last item in weapons array
        if (!isShooting && !isReloading && grenade.ammunition.getNumGrenades() > 0 && isThrowingGrenade == false)
        {//ready to throw grenade
            if (Input.GetKey(KeyCode.G))//hold
            {
                grenadeHoldLength += Time.deltaTime;
                grenadeHoldLength = Mathf.Min(grenadeHoldLength, 2);
            }
            if (Input.GetKeyUp(KeyCode.G))
            {//throw

                //wait for grenade animation to reach apex of throw
                //Debug.Log("throwing grenade with hold length = " + grenadeHoldLength);
                StartCoroutine(throwGrenade(70 + 250 * grenadeHoldLength));
                grenadeHoldLength = 0;
            }


        }
        else
        {
            grenadeHoldLength = 0;
        }


        Animation();
    }

    IEnumerator throwGrenade(float throwForce) {
        isThrowingGrenade = true;
        animator.SetTrigger("throwGrenade");
        //Debug.Log(isThrowingGrenade);
        yield return new WaitForSecondsRealtime(1.80f);
        //StartCoroutine(wait(1.50f));
        //Spawn Grenade
        CmdthrowGrenade(throwForce);
        resourceManager.useGrenade(1, grenade.ammunition);
        //Wait for rest of animation to finish
        yield return new WaitForSecondsRealtime(1.80f);
        //StartCoroutine(wait(1.80f));
        isThrowingGrenade = false;
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

        if (Input.GetButton("Fire1") && weapons[selectedWeapon].ammunition.getMagAmmo() != 0 && !isReloading && !Input.GetKey(KeyCode.LeftShift))
        {
            isShooting = true;
        }
        else
        {
            isShooting = false;
        }

        if (weapons[selectedWeapon].ammunition.getMagAmmo() == 0)
        {
            isShooting = false;
        }



        animator.SetBool("isCarryingPistol", isCarryingPistol);
        animator.SetBool("isReloading", isReloading);
        animator.SetBool("isThrowingGrenade", isThrowingGrenade);
        animator.SetBool("isShooting", isShooting);
        animator.SetBool("isAiming", isAiming);
    }

    [Command]
    public void CmdthrowGrenade(float throwForce)
    {
        Debug.Log("throwing grenade with throwForce: " + throwForce);
        GameObject grenade =  Instantiate(grenadePrefab, grenadeSpawn.transform.position, Camera.main.transform.rotation);
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
        if (isShooting == true)
        {
            audioSource.Play();
        }

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
            var rootTransform = hitFromGun.collider.transform.root;
            if (rootTransform.CompareTag(PLAYER_TAG))
            {
                createDamageText(rootTransform, weapons[selectedWeapon].damage);
                CmdPlayerAttacked(rootTransform.name, weapons[selectedWeapon].damage);
            }
            else //if not a player
            {
                if (weapons[selectedWeapon].isSpecial != true)
                {
                    CmdObjectHitEffect(hitFromGun.point, hitFromGun.normal);
                }
            }


            if (hitFromGun.collider.CompareTag(VOXEL_TAG))
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
                createDamageText(hitFromGun.transform, weapons[selectedWeapon].damage);
                CmdShieldHit(hitFromGun.collider.gameObject, weapons[selectedWeapon].damage);
            }
        }
        else
        {
            //can play a sound or something (empty mag)
        }
    }

    private void createDamageText(Transform hit, float damage)
    {
        Instantiate
        (
            damageTextIndicatorEffect,
            hit.position + hit.up * 10,
            hit.rotation
        ).GetComponent<TextDamageIndicator>().setUp((int)damage);
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