using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(ResourceManager))]
public class MagicAttack : AAttackBehaviour
{
    [SerializeField] private MagicType type;
    [SerializeField] private GameObject shield;

    private ResourceManager resourceManager;
    [SerializeField] private bool shieldUp; // True if the player is currently using a shield
    private Shield currentShield; // The current instance of shield

    void Start()
    {
        resourceManager = GetComponent<ResourceManager>();
        shieldUp = false;
    }

    void Update()
    {
        base.Update();

        if (!resourceManager.hasEnergy() && currentShield != null && shieldUp) endSecondaryAttack();

        if (!shieldUp) resourceManager.gainEnery(Shield.energyGainRate * Time.deltaTime);
        if (shieldUp) resourceManager.useEnergy(Shield.energyDrainRate * Time.deltaTime);
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

    /// <summary>
    /// Called when mouse 1 released
    /// </summary>
    [Client]
    public override void endAttack()
    {
    }

    /// <summary>
    /// Called when mouse2 pressed
    /// Spawns a shield if the player has enough energy
    /// </summary>
    [Client]
    public override void secondaryAttack()
    {
        if (!isLocalPlayer) return;

        if (resourceManager.getEnergy() > Shield.initialEnergyUsage && type.isShield && !shieldUp)
        {
            resourceManager.useEnergy(Shield.initialEnergyUsage);

            CmdSpawnShield();
//            shieldEnergyDrain = resourceManager.beginEnergyDrain(currentShield.energyDrainRate);
            shieldUp = true;
        }
    }

    /// <summary>
    /// Called on mouse 2 released
    /// Ends the players shield and its energy drain
    /// </summary>
    [Client]
    public override void endSecondaryAttack()
    {
        if (!type.isShield) return;

        CmdDestroyShield();
        shieldUp = false;
    }

    /// <summary>
    /// Called on the server to spawn a shield for the local player
    /// </summary>
    [Command]
    public void CmdSpawnShield()
    {
        var shieldInst = Instantiate(shield, transform.position, Quaternion.identity);
        NetworkServer.Spawn(shieldInst);

        currentShield = shieldInst.GetComponent<Shield>();
        currentShield.setCaster(GetComponent<Identifier>());

        shieldInst.GetComponent<NetHealth>().setInitialHealth(currentShield.shieldHealth);
        shieldInst.transform.parent = transform;
        RpcMakeShieldChild(shieldInst.GetComponent<Identifier>().id, GetComponent<Identifier>().id);
    }

    /// <summary>
    /// Makes the shield a child of the local player on all clients
    /// </summary>
    /// <param name="shieldInst">The game object to be the child</param>
    /// <param name="parent">The game object to be the parent</param>
    [ClientRpc]
    private void RpcMakeShieldChild(string shieldID, string parentID)
    {
        Debug.Log("Shield id: " + shieldID + " Parent id: " + parentID);
        GameManager.getObject(shieldID).transform.parent = GameManager.getObject(parentID).transform;
        GameManager.getObject(shieldID).GetComponent<Shield>()
            .setCaster(GameManager.getObject(parentID).GetComponent<Identifier>());
    }

    /// <summary>
    /// Destroys the currently active shield for the local player on all clients
    /// </summary>
    [Command]
    public void CmdDestroyShield()
    {
        if (currentShield == null) return;

        Destroy(currentShield.gameObject);
        NetworkServer.Destroy(currentShield.gameObject);
    }

    /// <summary>
    /// Sets <code>shieldUp</code> to false
    /// </summary>
    public void shieldDown()
    {
        shieldUp = false;
    }

    /// <summary>
    /// Returns the resource manager for this player
    /// </summary>
    /// <returns>The <code>ResourceManager</code> of the local player</returns>
    public ResourceManager getResourceManager()
    {
        return resourceManager;
    }
}