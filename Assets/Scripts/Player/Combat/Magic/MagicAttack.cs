using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(ResourceManager))]
public class MagicAttack : AAttackBehaviour
{
    [SerializeField] private MagicType attackStats;
    [SerializeField] private Transform rightHand;

    [SyncVar] private bool isAttacking;

    // Digger tool
    [SyncVar] private bool isDigging;

    // Damage
    [SyncVar] private bool isDamaging;

    // Shield
    private Shield currentShield; // The current instance of shield=
    [SerializeField] private GameObject shield;
    private bool shieldUp; // True if the player is currently using a shield

    // Telekenesis
    private bool isTelekening;
    [SerializeField] private GameObject telekenObjectPos;
    private GameObject currentTelekeneticVoxel;

    // Force push
    private bool canCastPush; // True once player can recast forcePush
    [SerializeField] private float force;

    // Effects
    [SerializeField] private ParticleSystem damageFX;
    [SerializeField] private ParticleSystem damageHandFX;

    [SerializeField] private ParticleSystem diggerFX;
    [SerializeField] private ParticleSystem diggerHandFX;
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
        if (!isLocalPlayer) return;

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
        damageFX.Stop();

        diggerFX.Stop();
    }

    void Update()
    {
        // Needs to be done for non-local clients
        // Orient effects
        RaycastHit hit = new RaycastHit();
        if (isAttacking)
        {
            if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 1000, mask)) return;

            if (isDigging)
            {
                diggerFX.transform.LookAt(hit.point);
            }

            if (isDamaging)
            {
                damageFX.transform.LookAt(hit.point);
                var angles = damageFX.transform.rotation.eulerAngles;
                damageFX.transform.rotation = Quaternion.Euler(angles.x + 90, angles.y, angles.z + 180);
            }
        }

        if (!isLocalPlayer) return;
        // Checks when attack related keys are pressed
        base.Update();

        cycleWeapons();
        energyUser();

        // Ends the shield if no energy remaining
        if (!resourceManager.hasEnergy() && shieldUp) endSecondaryAttack();

        // Digging
        if (isDigging && resourceManager.hasEnergy()) dig(hit);

        // Damaging
        if (isDamaging && resourceManager.hasEnergy()) damage(hit);
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
            playDamageEffect(true);
        }
        else if (attackStats.isTelekenetic)
        {
            // Shoot ray from the camera to center of screen
            RaycastHit hitFromCam;
            if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out hitFromCam,
                attackStats.telekenRange, mask))
                return;

            if (!hitFromCam.collider.CompareTag(VOXEL_TAG)) return;

            // Shoot ray from hand to hit position
            RaycastHit hitFromHand;
            if (Physics.Linecast(rightHand.position, hitFromCam.point, out hitFromHand, mask))
            {
                Debug.Log("hit: " + hitFromHand.collider.name);
            }

            var voxel = hitFromHand.collider.gameObject.GetComponent<Voxel>();
            if (voxel.shatterLevel >= 1) // need to change this to accomodate the teleken artifact
            {
                isTelekening = true;
                CmdVoxelTeleken(voxel.columnID, voxel.layer, voxel.subVoxelID);
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
            playDiggerEffect(true);
            isDigging = true;
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
            playDamageEffect(false);
        }
        else if (attackStats.isDigger)
        {
            playDiggerEffect(false);
            isDigging = false;
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

    #region weaponSwitching

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

    /// <summary>
    /// Changes the weapon selected bool and starts or stops the relevant hand FX
    /// </summary>
    private void changeWeapon()
    {
        if (currentWeapon == 0)
        {
            if (damageHandFX.isPlaying)
                damageHandFX.Stop();

            diggerHandFX.Play();
            attackStats.changeToDigger();
        }
        else if (currentWeapon == 1)
        {
            if (diggerHandFX.isPlaying)
                diggerHandFX.Stop();

            damageHandFX.Play();

            attackStats.changeToDamage();
        }
        else if (currentWeapon == 2)
        {
            if (diggerHandFX.isPlaying)
                diggerFX.Stop();

            if (damageHandFX.isPlaying)
                damageHandFX.Stop();
            attackStats.changeToTeleken();
        }
    }

    #endregion

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
    void dig(RaycastHit hitFromCam)
    {
        // Checking range, need high distance in raycast to orient effect
        if (Mathf.Abs(Vector3.Distance(hitFromCam.point, transform.position)) > attackStats.diggerRange) return;

        // Shoot ray from hand to hit position
        RaycastHit hitFromHand;
        if (!Physics.Linecast(rightHand.position, hitFromCam.point, out hitFromHand, mask))
            return; // this should never return

        if (hitFromHand.collider.gameObject.CompareTag(VOXEL_TAG))
        {
            var voxel = hitFromHand.collider.gameObject.GetComponent<Voxel>();

            // If voxel is about to die
            if (!voxel.hasEnergy &&
                voxel.GetComponent<NetHealth>().getHealth() <= attackStats.diggerEnvDamage * Time.deltaTime)
                destructionEffectSpawner.play(hitFromHand.point, voxel);


            CmdVoxelDamaged(hitFromHand.collider.gameObject, attackStats.diggerEnvDamage * Time.deltaTime);

            // Spawn energy blocks
            if (voxel.hasEnergy)
            {
                energyBlockEffectSpawner.setVoxel(voxel.gameObject);
                energyBlockEffectSpawner.spawnBlock();
            }
        }
        else if (hitFromHand.collider.CompareTag(PLAYER_TAG) || hitFromHand.collider.CompareTag("Shield"))
        {
            CmdVoxelDamaged(hitFromHand.collider.gameObject, attackStats.diggerDamage * Time.deltaTime);
        }
    }

    /// <summary>
    /// Controls which voxel gets damaged each frame and the orientation of the effect
    /// </summary>
    [Client]
    void damage(RaycastHit hitFromCam)
    {
        // Checking range, need high distance in raycast to orient effect
        if (Mathf.Abs(Vector3.Distance(hitFromCam.point, transform.position)) > attackStats.attackRange) return;

        // Shoot ray from hand to hit position
        RaycastHit hitFromHand;
        if (!Physics.Linecast(rightHand.position, hitFromCam.point, out hitFromHand, mask))
            return; // This should never return

        if (hitFromHand.collider.CompareTag(PLAYER_TAG))
        {
            var character = hitFromHand.collider.gameObject.GetComponent<Identifier>().typePrefix;
            if (character == Identifier.magicianType) // Heal
            {
                CmdPlayerAttacked(hitFromHand.collider.name, -attackStats.heal * Time.deltaTime);
            }
            else // Damage
            {
                CmdPlayerAttacked(hitFromHand.collider.name, attackStats.attackDamage * Time.deltaTime);
            }
        }
        else if (hitFromHand.collider.CompareTag(VOXEL_TAG))
        {
            CmdVoxelDamaged(hitFromHand.collider.gameObject, attackStats.attackEnvDamage * Time.deltaTime);
        }
        else if (hitFromHand.collider.CompareTag("Shield"))
        {
            CmdShieldHit(hitFromHand.collider.gameObject, attackStats.attackShieldDamage * Time.deltaTime);
        }
    }

    #region shield

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
    /// Sets <code>shieldUp</code> to false
    /// </summary>
    public void shieldDown()
    {
        shieldUp = false;
    }

    #endregion

    #region teleken

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

        var tele = currentTelekeneticVoxel.GetComponent<Telekinesis>();
        tele.enabled = true;
        tele.setUp(telekenObjectPos.transform, Telekinesis.VOXEL, GetComponent<Identifier>().id);
    }

    [Command]
    void CmdEndTeleken()
    {
        if (currentTelekeneticVoxel != null)
        {
            currentTelekeneticVoxel.GetComponent<Telekinesis>().throwObject(cam.transform.forward);
        }
    }

    #endregion

    #region syncParticleEffects

    /*
     * Playing partilce effects on all clients
     */

    #region playDiggerEffect

    void playDiggerEffect(bool digging)
    {
        if (isLocalPlayer)
        {
            CmdDiggerEffect(digging);
        }

        if (digging)
        {
            diggerFX.Play();
            return;
        }

        diggerFX.Stop();
    }

    [Command]
    void CmdDiggerEffect(bool digging)
    {
        RpcDiggerEffect(digging);
    }

    [ClientRpc]
    void RpcDiggerEffect(bool digging)
    {
        if (isLocalPlayer)
        {
            return;
        }

        playDiggerEffect(digging);
    }

    #endregion

    #region playDamageEffect

    void playDamageEffect(bool damaging)
    {
        if (isLocalPlayer)
            CmdDamageEffect(damaging);

        if (damaging)
        {
            damageFX.Play();
            return;
        }

        damageFX.Stop();
    }

    [Command]
    void CmdDamageEffect(bool damaging)
    {
        RpcDamageEffect(damaging);
    }

    [ClientRpc]
    void RpcDamageEffect(bool damaging)
    {
        if (isLocalPlayer)
            return;

        playDamageEffect(damaging);
    }

    #endregion

    #endregion
}