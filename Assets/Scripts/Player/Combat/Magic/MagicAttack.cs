using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(ResourceManager))]
public class MagicAttack : AAttackBehaviour
{
    [SerializeField] private MagicType attackStats;

    private bool isAttacking;

    // Digger tool
    private bool isDigging;

    // Damage
    private bool isDamaging;

    // Shield
    private Shield currentShield; // The current instance of shield=
    [SerializeField] private GameObject shield;
    [SerializeField] private bool shieldUp; // True if the player is currently using a shield

    // Telekenesis
    private bool isTelekening;
    [SerializeField] private GameObject telekenObjectPos;
    [SerializeField] private GameObject currentTelekeneticVoxel;

    // Force push
    [SerializeField] private bool canCastPush; // True once player can recast forcePush
    [SerializeField] private float force;

    // Effects
    [SerializeField] private ParticleSystem attackEffect;
    [SerializeField] private ParticleSystem diggerEffect;
    private EnergyBlockEffectSpawner energyBlockEffectSpawner;
    private DestructionEffectSpawner destructionEffectSpawner;

    /// <summary>
    /// 0 = Digger
    /// 1 = Damage/Heal
    /// 2 = Telekinesis
    /// 3 = push? (not implemented)
    /// </summary>
    public int currentWeapon;

    void Start()
    {
        resourceManager = GetComponent<ResourceManager>();
        energyBlockEffectSpawner = GetComponent<EnergyBlockEffectSpawner>();
        destructionEffectSpawner = GetComponent<DestructionEffectSpawner>();

        currentWeapon = 0;
        shieldUp = false;
        isAttacking = false;
        force = 100;

        // Initial weapon selection
        attackStats.changeToDigger();

        // Stopping all effects
        attackEffect.Stop();
        diggerEffect.Stop();
    }

    void Update()
    {
        // Checks when attack related keys are pressed
        base.Update();

        cycleWeapons();
        energyUser();

        // Ends the shield if no energy remaining
        if (!resourceManager.hasEnergy() && shieldUp) endSecondaryAttack();

        // Digging
        if (isDigging && resourceManager.hasEnergy()) dig();

        // Damaging
        if (isDamaging && resourceManager.hasEnergy()) damage();
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

        if (Input.GetKey(KeyCode.Alpha1))
        {
            currentWeapon = 0;
        }

        if (Input.GetKey(KeyCode.Alpha2))
        {
            currentWeapon = 1;
        }

        if (Input.GetKey(KeyCode.Alpha3))
        {
            currentWeapon = 2;
        }
    }

    private void changeWeapon()
    {
        if (currentWeapon == 0) attackStats.changeToDigger();
        else if (currentWeapon == 1) attackStats.changeToDamage();
        else if (currentWeapon == 2) attackStats.changeToTeleken();
    }

    [Client]
    public override void attack()
    {
        if (!MapManager.manager.mapDoneLocally)
        {
            Debug.LogError("Attacking before map finished");
            return;
        }

        isAttacking = true;
        if (attackStats.isDamage)
        {
            isDamaging = true;
            attackEffect.Play();
        }
        else if (attackStats.isTelekenetic)
        {
            RaycastHit hit;
            if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, attackStats.telekenRange,
                mask))
                return;

            if (!hit.collider.CompareTag(VOXEL_TAG)) return;

            var voxel = hit.collider.gameObject.GetComponent<Voxel>();
            if (voxel.shatterLevel >= 1) // need to change this to accomodate the teleken artifact
            {
                isTelekening = true;
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
        else if (attackStats.isForcePush) // This is not yet working
        {
        }
        else if (attackStats.isDigger)
        {
            isDigging = true;
            diggerEffect.Play();
        }
    }

    /// <summary>
    /// Called when mouse 1 released
    /// </summary>
    [Client]
    public override void endAttack()
    {
        if (attackStats.isTelekenetic)
        {
            isTelekening = false;
            CmdEndTeleken();
        }
        else if (attackStats.isForcePush)
        {
            canCastPush = true;
        }
        else if (attackStats.isDamage)
        {
            isDamaging = false;
            attackEffect.Stop();
        }
        else if (attackStats.isDigger)
        {
            isDigging = false;
            diggerEffect.Stop();
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

        if (resourceManager.getEnergy() > attackStats.initialShieldMana && attackStats.isShield && !shieldUp)
        {
            resourceManager.useEnergy(attackStats.initialShieldMana);

            CmdSpawnShield(GetComponent<Identifier>().id);
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
        if (attackStats.isShield)
        {
            Debug.LogWarning("Ending secondary attack");
            CmdDestroyShield();
            shieldUp = false;
        }
    }

    /// <summary>
    /// Drains and gains energy depending on active spells
    /// </summary>
    private void energyUser()
    {
        // Mana gain
        if (!shieldUp && !isAttacking) resourceManager.gainEnery(attackStats.manaRegen * Time.deltaTime);

        // Mana drain
        // Dig
        if (isDigging) resourceManager.useEnergy(attackStats.diggerMana * Time.deltaTime);

        // Attacker
        if (isDamaging) resourceManager.useEnergy(attackStats.attackMana * Time.deltaTime);

        // Shield
        if (shieldUp) resourceManager.useEnergy(attackStats.shieldMana * Time.deltaTime);

        // Teleken
        if (isTelekening) resourceManager.useEnergy(attackStats.telekenMana * Time.deltaTime);
    }

    /// <summary>
    /// Orients the digger effect and damages relevent objects while mouse 1 held down
    /// </summary>
    [Client]
    void dig()
    {
        RaycastHit hit;
        if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 1000, mask)) return;

        // Rotating the effect
        diggerEffect.transform.LookAt(hit.point);

        // Checking range, need high distance in raycast to orient effect
        if (Mathf.Abs(Vector3.Distance(hit.point, transform.position)) > attackStats.diggerRange) return;

        if (hit.collider.gameObject.CompareTag(VOXEL_TAG))
        {
            var voxel = hit.collider.gameObject.GetComponent<Voxel>();

            // If voxel is about to die
            if (!voxel.hasEnergy && voxel.GetComponent<NetHealth>().getHealth() <=
                attackStats.diggerEnvDamage * Time.deltaTime)
                destructionEffectSpawner.play(hit.point, voxel);


            CmdVoxelDamaged(hit.collider.gameObject, attackStats.diggerEnvDamage * Time.deltaTime);

            if (voxel.hasEnergy)
            {
                energyBlockEffectSpawner.setVoxel(voxel.gameObject);
                energyBlockEffectSpawner.spawnBlock();
            }
        }
        else if (hit.collider.CompareTag(PLAYER_TAG) || hit.collider.CompareTag("Shield"))
        {
            CmdVoxelDamaged(hit.collider.gameObject, attackStats.diggerDamage * Time.deltaTime);
        }
    }

    /// <summary>
    /// Controls which voxel gets damaged each frame and the orientation of the effect
    /// </summary>
    [Client]
    void damage()
    {
        RaycastHit hit;
        if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 1000, mask))
            return;

        // Pointing the effect in the correct direction
        attackEffect.transform.LookAt(hit.point);
        var angles = attackEffect.transform.rotation.eulerAngles;
        attackEffect.transform.rotation = Quaternion.Euler(angles.x + 90, angles.y, angles.z + 180);

        // Checking range, need high distance in raycast to orient effect
        if (Mathf.Abs(Vector3.Distance(hit.point, transform.position)) > attackStats.attackRange) return;

        if (hit.collider.CompareTag(PLAYER_TAG))
        {
            var character = hit.collider.gameObject.GetComponent<Identifier>().typePrefix;
            if (character == Identifier.magicianType) // Heal
            {
                CmdPlayerAttacked(hit.collider.name, -attackStats.heal * Time.deltaTime);
            }
            else // Damage
            {
                CmdPlayerAttacked(hit.collider.name, attackStats.attackDamage * Time.deltaTime);
            }
        }
        else if (hit.collider.CompareTag(VOXEL_TAG))
            CmdVoxelDamaged(hit.collider.gameObject, attackStats.attackEnvDamage * Time.deltaTime);
        else if (hit.collider.CompareTag("Shield"))
            CmdShieldHit(hit.collider.gameObject, attackStats.attackShieldDamage * Time.deltaTime);
    }

    public void setUpShield(GameObject shieldInst)
    {
        currentShield = shieldInst.GetComponent<Shield>();
        currentShield.GetComponent<NetHealth>().setInitialHealth(attackStats.shieldHealth);
        // Setting the caster to this magician and setting up UI
        currentShield.setCaster(GetComponent<Identifier>(), attackStats.shieldHealth);

        // Allowing it to move with the player
        currentShield.transform.parent = transform;
    }

    /// <summary>
    /// Called on the server to spawn a shield for the local player
    /// </summary>
    [Command]
    public void CmdSpawnShield(string casterID)
    {
        var shieldInst = Instantiate(shield, transform.position, Quaternion.identity);
        NetworkServer.Spawn(shieldInst);
        // Servers current shield is not neccaserily the servers instance of shield (is likely local clients instance)
        setUpShield(shieldInst);
        RpcSetUpShieldUI(casterID, shieldInst.GetComponent<Identifier>().id);
    }

    /// <summary>
    /// Makes the shield a child of the local player on all clients
    /// </summary>
    /// <param name="shieldInst">The game object to be the child</param>
    /// <param name="parent">The game object to be the parent</param>
    [ClientRpc]
    private void RpcSetUpShieldUI(string casterID, string shieldID)
    {
        GameManager.getObject(shieldID).GetComponent<Shield>().setCaster
        (
            GameManager.getObject(casterID), 
            attackStats.shieldHealth
        );
        GameManager.getObject(shieldID).transform.parent = GameManager.getObject(casterID).transform;
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

    /// <summary>
    /// Sets <code>shieldUp</code> to false
    /// </summary>
    public void shieldDown()
    {
        shieldUp = false;
    }
}