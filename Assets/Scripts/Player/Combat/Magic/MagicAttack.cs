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

    [SerializeField] private ParticleSystem attackEffect;

    /// <summary>
    /// 0 = Damage/Heal
    /// 1 = Push
    /// 2 = Telekinesis
    /// </summary>
    public int currentWeapon;

    private bool isAttacking;

    void Start()
    {
        resourceManager = GetComponent<ResourceManager>();

        shieldUp = false;
        isAttacking = false;
        force = 100;

        attackEffect.Stop();
    }

    void Update()
    {
        // Checks when attack related keys are pressed
        base.Update();

        cycleWeapons();
        // Ends the shield if no energy remaining
        if (!resourceManager.hasEnergy() && currentShield != null && shieldUp) endSecondaryAttack();

        // Energy gain/drain
        if (!shieldUp) resourceManager.gainEnery(Shield.energyGainRate * Time.deltaTime);
        if (shieldUp) resourceManager.useEnergy(Shield.energyDrainRate * Time.deltaTime);
    }

    private void cycleWeapons()
    {
        if (!isLocalPlayer || isAttacking) return;

        var scroll = Input.GetAxis("Mouse ScrollWheel");

        // I understand the direction makes no sense, but it works better for the UI
        if (scroll < 0f)
        {
            currentWeapon = (++currentWeapon) % 3;
            changeWeapon();
        }
        else if (scroll > 0f)
        {
            if (currentWeapon == 0) 
                currentWeapon = 2;
            else 
                currentWeapon = --currentWeapon % 3;

            changeWeapon();
        }
    }

    private void changeWeapon()
    {
        if (currentWeapon == 0) type.changeToDamage();
        else if (currentWeapon == 1) type.changeToTeleken();
        else if (currentWeapon == 2) type.changeToPush();
    }

    [Client]
    public override void attack()
    {
        if (!MapManager.manager.mapDoneLocally)
        {
            Debug.LogError("attacking before map finished");
            return;
        }

        isAttacking = true;
        if (type.isDamage)
        {
            RaycastHit hit;
            attackEffect.Play();

            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 1000, mask))
            {
                if (hit.collider.CompareTag(PLAYER_TAG))
                {
                    var character = hit.collider.gameObject.GetComponent<Identifier>().typePrefix;
                    if (character == "Magician") // Heal
                    {
                        CmdPlayerAttacked(hit.collider.name, -20);
                    }
                    else // Damage
                    {
                        CmdPlayerAttacked(hit.collider.name, 50);
                    }
                }
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
                    if (voxel.shatterLevel >= 1) // need to change this to accomodate the teleken artifact
                    {
                        CmdVoxelTeleken(voxel.columnID, voxel.layer, voxel.subVoxelID);
                        // Just run telekenisis locally network transform will sync movements
                        var tele = currentTelekeneticVoxel.GetComponent<Telekinesis>();
                        tele.enabled = true;
                        tele.setUp(telekenObjectPos.transform, Telekinesis.VOXEL, GetComponent<Identifier>().id);
                    }
                    else
                    {
                        // Play some effect to show that voxel is too big
                    }
                }
            }
        }
        else if (type.isForcePush) // This is not yet working
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

//                    coll.gameObject.GetComponent<Rigidbody>()
//                         .AddForce(direction.normalized * force /* * (1 / direction.sqrMagnitude)*/,
//                            ForceMode.Impulse);
                    CmdPush(coll.gameObject.GetComponent<Identifier>().id, direction);
                }
            }

            canCastPush = false;
        }
    }

    [Command]
    void CmdPush(String id, Vector3 direction)
    {
        Debug.Log("CMD: " + id);
//        GameManager.getObject(id).GetComponent<Rigidbody>()
//            .AddForce(transform.forward.normalized * force /* * (1 / direction.sqrMagnitude)*/,
//                ForceMode.Impulse);
        RpcPush(id);
    }

    /// <summary>
    /// Called when mouse 1 released
    /// </summary>
    [Client]
    public override void endAttack()
    {
        if (type.isTelekenetic)
        {
            CmdEndTeleken();
        }
        else if (type.isForcePush)
        {
            canCastPush = true;
        }
        else if (type.isDamage)
        {
            attackEffect.Stop();
        }

        isAttacking = false;
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
        var shieldInst = GameManager.getObject(shieldID);
        // This might only need to be done server and local player side, not on all machines
        currentShield = shieldInst.GetComponent<Shield>();
        currentShield.setCaster(GetComponent<Identifier>());

        shieldInst.GetComponent<NetHealth>().setInitialHealth(currentShield.shieldHealth);
        // Up to here

        shieldInst.transform.parent =
            GameManager.getObject(parentID).GetComponentInChildren<Camera>().transform;
        shieldInst.GetComponent<Shield>()
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
    private void CmdVoxelTeleken(int col, int layer, string subID)
    {
        currentTelekeneticVoxel = MapManager.manager.getSubVoxelAt(layer, col, subID).gameObject;

        // Prepare the voxel for telekenisis
        RpcPrepVoxel(col, layer, subID, GetComponent<Identifier>().id);
    }


    /// <summary>
    /// Adds and enables the necessarry components to the voxels on every client
    /// </summary>
    /// <param name="col"></param>
    /// <param name="layer"></param>
    /// <param name="subID"></param>
    /// <param name="playerID"></param>
    [ClientRpc]
    private void RpcPrepVoxel(int col, int layer, string subID, string playerID)
    {
        prepVoxel(col, layer, subID, playerID);
    }

    /// <summary>
    /// Adds a rigidbody and enables the network transform of a given voxel.
    /// Creates the necessary voxels below the current telekenetic one.
    /// </summary>
    /// <param name="col"></param>
    /// <param name="layer"></param>
    /// <param name="subID"></param>
    /// <param name="playerID"></param>
    private void prepVoxel(int col, int layer, string subID, string playerID)
    {
        currentTelekeneticVoxel = MapManager.manager.getSubVoxelAt(layer, col, subID).gameObject;
        var voxel = MapManager.manager.getSubVoxelAt(layer, col, subID);

        // Setting up rigidbody
        // Needs to be true to work with a rigid body
        voxel.gameObject.GetComponent<MeshCollider>().convex = true; // Still throws error sometimes

        if (voxel.gameObject.GetComponent<Rigidbody>() == null)
        {
            var rb = voxel.gameObject.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.useGravity = false;
        }

        // Enabling network transform
        currentTelekeneticVoxel.GetComponent<NetworkTransform>().enabled = true;

        voxel.transform.parent = MapManager.manager.Map.transform;
        voxel.gameObject.name = playerID + "_teleken_voxel";

        // Creating the voxels below
        voxel.showNeighbours(false);
    }

    [Command]
    void CmdEndTeleken()
    {
        if (currentTelekeneticVoxel != null)
        {
            currentTelekeneticVoxel.GetComponent<Telekinesis>().throwObject(cam.transform.forward);
        }
    }

//    [Command]
//    void CmdPush(string id)
//    {
//        RpcPush(id);
//    }

    [ClientRpc]
    void RpcPush(string id)
    {
        var direction = GameManager.getObject(id).transform.position - transform.position;

        GameManager.getObject(id).gameObject.GetComponent<Rigidbody>()
            .AddForce(transform.forward.normalized * force /* * (1 / direction.sqrMagnitude)*/);
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