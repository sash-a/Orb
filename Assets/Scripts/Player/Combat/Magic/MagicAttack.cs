﻿using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(ResourceManager))]
public class MagicAttack : AAttackBehaviour
{
    [SerializeField] private MagicType type;
    [SerializeField] private GameObject shield;

    private ResourceManager resourceManager;
    [SerializeField] private bool shieldUp; // True if the player is currently using a shield
    private GameObject currentShield; // The current instance of shield
    private Coroutine shieldEnergyDrain; // Corutine resposible for the energy drain of the current shield

    void Start()
    {
        resourceManager = GetComponent<ResourceManager>();
        shieldUp = false;
    }

    void Update()
    {
        base.Update();

        if (!resourceManager.hasEnergy() && currentShield != null)
        {
            resourceManager.endEnergyDrain(shieldEnergyDrain);
            CmdDestroyShield();
        }

        if (shieldUp && resourceManager.hasEnergy())
        {
            // Drain energy
        }
    }

    [Client]
    public override void attack()
    {
        if (type.isDamage)
        {
            RaycastHit hit;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 1000, mask))
            {
                Debug.Log(hit.collider.tag);
                if (hit.collider.tag == PLAYER_TAG)
                    CmdPlayerAttacked(hit.collider.name, 50);
                else if (hit.collider.tag == VOXEL_TAG)
                    CmdVoxelDamaged(hit.collider.gameObject, 50); // weapontype.envDamage?
                else if (hit.collider.tag == "Shield")
                {
                    CmdShieldHit(hit.collider.gameObject, 50);
                }
            }
        }
    }

    [Client]
    public override void endAttack()
    {
    }

    public override void secondaryAttack()
    {
        if (type.isShield && resourceManager.hasEnergy() && !shieldUp)
        {
            CmdSpawnShield();
            shieldEnergyDrain = resourceManager.beginEnergyDrain(1);
            shieldUp = true;
        }
        
        Debug.Log("Attack");
        debugVals();
    }

    public override void endSecondaryAttack()
    {
        if (!type.isShield) return;

        CmdDestroyShield();
        resourceManager.endEnergyDrain(shieldEnergyDrain);
        shieldUp = false;

        Debug.Log("End Attack");
        debugVals();
    }

    [Command]
    public void CmdSpawnShield()
    {
        currentShield = Instantiate(shield, transform.position, Quaternion.identity);
        NetworkServer.Spawn(currentShield);

        currentShield.GetComponent<ShieldSetUp>().setCaster(GetComponent<Identifier>());
        currentShield.transform.parent = transform;
    }

    [Command]
    public void CmdDestroyShield()
    {
        if (currentShield == null) return;

        Destroy(currentShield);
        NetworkServer.Destroy(currentShield);
    }

    public void shieldDown()
    {
        shieldUp = false;
    }

    public ResourceManager getResourceManager()
    {
        return resourceManager;
    }

    public Coroutine getShieldEnergyDrain()
    {
        return shieldEnergyDrain;
    }

    private void debugVals()
    {
        Debug.Log("Shield up: " + shieldUp);
    }
}