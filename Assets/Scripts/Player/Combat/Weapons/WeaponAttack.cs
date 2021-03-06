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
    private DestructionEffectSpawner destructionEffectSpawner;

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

    public float camAngle = 60;
    float camZoomSpeed = 0.4f;

    float grenadeHoldLength = 0;

    //Sound:
    public AudioSource audioSource;
    public AudioClip gunShotClip;
    public AudioClip reloadClip;
    public AudioClip pullingPinClip;
    public AudioClip throwingGrenadeClip;

    public WeaponWheel weaponWheel;

    public Scope sniperScope;
    public int idealCamAngle;
    public Camera weaponCamera;

    void Start()
    {
        resourceManager = GetComponent<ResourceManager>();
        destructionEffectSpawner = GetComponent<DestructionEffectSpawner>();
        //energyBlockEffectSpawner = GetComponent<EnergyBlockEffectSpawner>();
        player = GetComponent<PlayerController>();
        equippedWeapons.Add(weapons[0]); // Digger
        equippedWeapons.Add(weapons[1]); // Pistol
        equippedWeapons.Add(weapons[2]); // Assault rifle
        equippedWeapons.Add(weapons[6]); // Empty special

        if (sniperScope != null)
        {
            sniperScope.isLocalPlayer = isLocalPlayer;
        }
        else
        {
            Debug.LogError("no sniper scope script attached to gunner weapon attack");
        }
    }

    private void Update()
    {
        if (PlayerUI.isPaused) return;
        if (!isReloading)
        {
            SelectWeapon();
        }
        
        //Attacking
        if (Input.GetButton("Fire1") && Time.time >= weapons[selectedWeapon].nextTimeToFire && !isReloading &&
            !isThrowingGrenade && !Input.GetKey(KeyCode.LeftShift))
        {
            weapons[selectedWeapon].nextTimeToFire = Time.time + 1f / weapons[selectedWeapon].fireRate;
            attack();
        }

        //Aiming
        float idealLookSensitivity;

        if (Input.GetButton("Fire2") && !isReloading && !Input.GetKey(KeyCode.LeftShift))
        {
            if (weapons[selectedWeapon].name != WeaponType.SNIPER)
            {
                isAiming = true;
            }

            idealCamAngle = sniperScope.scopedIn ? 25 : 40;
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
        weaponCamera.fieldOfView = camAngle;
        player.lookSens += (idealLookSensitivity - player.lookSens) * camZoomSpeed;

        //Debug.Log("cam angle: " + cam.fieldOfView);

        //Reload
        if ((Input.GetKey(KeyCode.R) && weapons[selectedWeapon].name != WeaponType.DIGGING_TOOL) || weapons[selectedWeapon].ammunition.getMagAmmo() == 0 && !isReloading)
        {
            //Debug.Log("Reload!");
            StartCoroutine(Reload(weapons[selectedWeapon].ammunition));
        }

        //NB: This method will only work if grenades is last item in weapons array
        if (!isShooting && !isReloading && grenade.ammunition.getNumGrenades() > 0 && isThrowingGrenade == false)
        {
            //ready to throw grenade
            if (Input.GetKey(KeyCode.G)) //hold
            {
                grenadeHoldLength += Time.deltaTime;
                grenadeHoldLength = Mathf.Min(grenadeHoldLength, 2);
            }

            if (Input.GetKeyUp(KeyCode.G))
            {
                //throw
                StartCoroutine(throwGrenadeSound());

                //wait for grenade animation to reach apex of throw
                //Debug.Log("throwing grenade with hold length = " + grenadeHoldLength);
                StartCoroutine(throwGrenade(150 + 215 * grenadeHoldLength));
                grenadeHoldLength = 0;
            }
        }
        else
        {
            grenadeHoldLength = 0;
        }

        Animation();
    }

    IEnumerator throwGrenadeSound()
    {
        audioSource.PlayOneShot(pullingPinClip);
        yield return new WaitForSecondsRealtime(1.0f);
        audioSource.PlayOneShot(throwingGrenadeClip);
    }

    private void SelectWeapon()
    {
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
    }

    IEnumerator throwGrenade(float throwForce)
    {
        isThrowingGrenade = true;
        animator.SetTrigger("throwGrenade");
        //Debug.Log(isThrowingGrenade);
        yield return new WaitForSecondsRealtime(1.80f);
        //Spawn Grenade
        CmdthrowGrenade(throwForce);
        resourceManager.useGrenade(1, grenade.ammunition);
        //Wait for rest of animation to finish
        yield return new WaitForSecondsRealtime(1.80f);
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

        if (Input.GetButton("Fire1") && weapons[selectedWeapon].ammunition.getMagAmmo() != 0 && !isReloading &&
            !Input.GetKey(KeyCode.LeftShift))
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

    private AudioClip MakeSubclip(AudioClip clip, float start, float stop)
    {
        //Create a new audio clip
        int frequency = clip.frequency;
        float timeLength = stop - start;
        int samplesLength = (int) (frequency * timeLength);
        AudioClip newClip = AudioClip.Create(clip.name + "-sub", samplesLength, 1, frequency, false);
        //Create a temporary buffer for the samples
        float[] data = new float[samplesLength];
        //Get the data from the original clip
        clip.GetData(data, (int) (frequency * start));
        //Transfer the data to the new clip
        newClip.SetData(data, 0);
        //Return the sub clip
        return newClip;
    }

    [Command]
    public void CmdthrowGrenade(float throwForce)
    {
        //Debug.Log("throwing grenade with throwForce: " + throwForce);
        GameObject grenade =
            Instantiate(grenadePrefab, grenadeSpawn.transform.position, Camera.main.transform.rotation);
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
            if (!isReloading)
            {
                audioSource.PlayOneShot(reloadClip, 0.7f);
            }
            
  
            isReloading = true;
            //wait length of animation (3.3 seconds)
            yield return new WaitForSeconds(3.3f);
            isReloading = false;

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

       //Debug.Log("attacking");

        //Crossbow
        if (weapons[selectedWeapon].name == WeaponType.EX_CROSSBOW && weapons[selectedWeapon].ammunition.getMagAmmo() > 0)
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

                //Add some random pitch and volum to each gunshot:
                audioSource.volume = UnityEngine.Random.Range(0.8f, 1);
                audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(gunShotClip);

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
            {
                Debug.Log("ray cast from cam in cam dir failed");
                return;
            }

            RaycastHit hitFromGun;
            if (!Physics.Linecast(weapons[selectedWeapon].shootPos.position,
                hitFromCam.point + 2 * cam.transform.forward, out hitFromGun, mask))
            {
                Debug.Log("linecast from shootpos to hitCam point failed");
                return; // Should never return
            }


            // if we hit a player
            var rootTransform = hitFromGun.collider.transform.root;
            if (rootTransform.CompareTag(PLAYER_TAG) && !hitFromGun.collider.CompareTag("Shield"))
            {
                float damage = hitFromGun.collider.name == "Head"
                    ? weapons[selectedWeapon].damage * WeaponType.headshotMultiplier
                    : weapons[selectedWeapon].damage;

                createDamageText(rootTransform, damage, hitFromGun.collider.name == "Head");
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

                // Playing the destruction effect
                bool random = Random.Range(0, 7) == 1;
                if (!voxel.hasEnergy &&
                    voxel.GetComponent<NetHealth>().getHealth() <= weapons[selectedWeapon].envDamage && random)
                    destructionEffectSpawner.play(hitFromGun.point, voxel);

                CmdVoxelDamaged(hitFromGun.collider.gameObject, weapons[selectedWeapon].envDamage);

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
                createDamageText(hitFromGun.transform, weapons[selectedWeapon].damage, false, true);
                CmdShieldHit(hitFromGun.collider.gameObject, weapons[selectedWeapon].damage);
            }
        }
        else
        {
            //can play a sound or something (empty mag)
        }

        //reset to normal
        audioSource.volume = 1.0f;
        audioSource.pitch = 1.0f;
    }

    private void createDamageText(Transform hit, float damage, bool isHeadShot, bool isShield = false)
    {
        float posUp = isShield ? 12 : 10;

        Instantiate
        (
            damageTextIndicatorEffect,
            hit.position + hit.up * posUp,
            hit.rotation
        ).GetComponent<TextDamageIndicator>().setUp((int) damage, false, isHeadShot, isShield);
    }

    public override void endAttack()
    {
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
    }
}