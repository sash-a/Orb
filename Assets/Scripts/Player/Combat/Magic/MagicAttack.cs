using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(ResourceManager))]
public class MagicAttack : AAttackBehaviour
{
    [SerializeField] private MagicType type;
    [SerializeField] private GameObject shield;
    [SerializeField] private GameObject telekenObjectPos;

    [SerializeField] private GameObject currentTelekeneticVoxel;

    [SerializeField] private bool shieldUp; // True if the player is currently using a shield
    [SerializeField] private bool canCastPush; // True once player can recast forcePush
    [SerializeField] private float force;

    private ResourceManager resourceManager;
    private Shield currentShield; // The current instance of shield


    void Start()
    {
        resourceManager = GetComponent<ResourceManager>();

        shieldUp = false;
    }

    void Update()
    {
        // Checks when attack related keys are pressed
        base.Update();

        // Ends the shield if no energy remaining
        if (!resourceManager.hasEnergy() && currentShield != null && shieldUp) endSecondaryAttack();

        // Energy gain/drain
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
                if (hit.collider.CompareTag(PLAYER_TAG))
                    CmdPlayerAttacked(hit.collider.name, 50);
                else if (hit.collider.CompareTag(VOXEL_TAG))
                    CmdVoxelDamaged(hit.collider.gameObject, 50); // weapontype.envDamage?
                else if (hit.collider.CompareTag("Shield"))
                    CmdShieldHit(hit.collider.gameObject, 50);
            }
        }
        else if (type.isTelekenetic)
        {
            RaycastHit hit;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 1000, mask))
            {
                if (hit.collider.CompareTag(VOXEL_TAG))
                {
                    var voxel = hit.collider.gameObject.GetComponent<Voxel>();
                    CmdVoxelTeleken(voxel.columnID, voxel.layer);
                }
            }
        }
        else if (type.isForcePush)
        {
            Debug.Log("Pushing");
//            force.setUp(transform.position, 50);
            if (!canCastPush) return;

            var myColl = GetComponent<Collider>();
            foreach (var coll in Physics.OverlapSphere(transform.position, 15))
            {
                Debug.Log(coll.name);
                if (coll.CompareTag(PLAYER_TAG) && coll != myColl)
                {
                    Debug.LogWarning(coll.gameObject.GetComponent<Identifier>().id);

                    var direction = coll.transform.position - transform.position;

                    if (coll.gameObject.GetComponent<Rigidbody>() == null)
                    {
                        Debug.LogError("Null");
                    }

                    coll.gameObject.GetComponent<Rigidbody>()
                        .AddForce(direction.normalized * force  /* * (1 / direction.sqrMagnitude)*/,
                            ForceMode.Impulse);
//                    CmdPush(coll.gameObject.GetComponent<Identifier>().id);
                }
            }

            canCastPush = false;
        }
    }

    /// <summary>
    /// Called when mouse 1 released
    /// </summary>
    [Client]
    public override void endAttack()
    {
        // TODO huge bug if player changes weapons while holding down attack!
        if (type.isTelekenetic)
        {
            CmdEndTeleken();
        }
        else if (type.isForcePush)
        {
            canCastPush = true;
        }
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
        if (type.isShield)
        {
            CmdDestroyShield();
            shieldUp = false;
        }
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
        RpcPrepShield(shieldInst.GetComponent<Identifier>().id, GetComponent<Identifier>().id);
    }

    /// <summary>
    /// Makes the shield a child of the local player on all clients
    /// </summary>
    /// <param name="shieldInst">The game object to be the child</param>
    /// <param name="parent">The game object to be the parent</param>
    [ClientRpc]
    private void RpcPrepShield(string shieldID, string parentID)
    {
        GameManager.getObject(shieldID).transform.parent =
            GameManager.getObject(parentID).GetComponentInChildren<Camera>().transform;
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
    /// Allows the player to control a voxel
    /// </summary>
    [Command]
    private void CmdVoxelTeleken(int col, int layer)
    {
        currentTelekeneticVoxel = MapManager.manager.voxels[layer][col].gameObject;
        // Prepare the voxel for telekenisis
        RpcPrepVoxel(col, layer, GetComponent<Identifier>().id);
    }


    [ClientRpc]
    private void RpcPrepVoxel(int col, int layer, string playerID)
    {
        // Add networktransform and rigidbody so that it can be moved on network
        var voxel = MapManager.manager.voxels[layer][col];

        // Needs to be true to work with a rigid body
        voxel.gameObject.GetComponent<MeshCollider>().convex = true; // Still throws error
        var rb = voxel.gameObject.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.useGravity = false;

        voxel.gameObject.AddComponent<NetworkTransform>();
        voxel.transform.parent = MapManager.manager.Map.transform;
        voxel.gameObject.name = playerID + "_teleken_voxel";

        // I think best solution is to find a way to add collider to player that stops 
        voxel.gameObject.AddComponent<Telekenises>().setUp(telekenObjectPos.transform, Telekenises.VOXEL, playerID);

        // Creating the voxels below
        voxel.showNeighbours(true); // TODO sometimes not creating voxels below (because of double shooting)
    }

    [Command]
    void CmdEndTeleken()
    {
        currentTelekeneticVoxel.GetComponent<Telekenises>().throwObject(cam.transform.forward);
    }

    [Command]
    void CmdPush(string id)
    {
        RpcPush(id);
    }

    [ClientRpc]
    void RpcPush(string id)
    {
        Debug.LogWarning("in rpc");
        var direction = GameManager.getObject(id).transform.position - transform.position;
        Debug.LogError(direction);

        GameManager.getObject(id).gameObject.GetComponent<Rigidbody>()
            .AddForce(direction.normalized * force * 1000000 /* * (1 / direction.sqrMagnitude)*/, ForceMode.Impulse);
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