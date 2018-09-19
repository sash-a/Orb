using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(ResourceManager))]
public class MagicAttack : AAttackBehaviour
{
    #region variables

    [SerializeField] private MagicType attackStats;

    [SerializeField] private Transform rightHand;

    [SyncVar] private bool isAttacking;

    // Digger tool
    [SyncVar] private bool isDigging;

    // Damage
    [SyncVar] private bool isDamaging;

    // Shield
    private Shield currentShield; // The current instance of shield
    [SerializeField] private GameObject shield;
    private bool shieldUp; // True if the player is currently using a shield
    private bool isShieldCoolingdown;

    // Telekenesis
    private bool isTelekening;
    [SerializeField] private GameObject telekenObjectPos;
    private GameObject currentTelekeneticVoxel;

    // Effects
    [SerializeField] private ParticleSystem damageFX;
    [SerializeField] private ParticleSystem damageHandFX;

    [SerializeField] private ParticleSystem diggerFX;
    [SerializeField] private ParticleSystem diggerHandFX;
    private EnergyBlockEffectSpawner energyBlockEffectSpawner;
    private DestructionEffectSpawner destructionEffectSpawner;

    public GameObject magicGrenadeFX;

    public GameObject damageTextIndicatorEffect;

    // Used so that commands are not passed every frame
    [SerializeField] private float waitTime;
    private float timePassed;

    /// <summary>
    /// 0 = Digger
    /// 1 = Damage/Heal
    /// 2 = Telekinesis
    /// </summary>
    public int currentWeapon;

    [SerializeField] private float pickupDistance;

    //Animation:
    public Animator animator;
    PlayerController player;
    float idealLookSensitivity;
    float telekenesisLookSensSlowDown = 0.4f;

    public Transform cameraPivot;
    Vector3 idealPivotLocalPosition;
    float idealCamFieldOfView = 65f;
    float pivotRetraction = 2.5f;
    float FOVincrease = 1.4f;

    #endregion

    public DamageType dmg;

    void Start()
    {
        if (!isLocalPlayer) return;

        resourceManager = GetComponent<ResourceManager>();
        energyBlockEffectSpawner = GetComponent<EnergyBlockEffectSpawner>();
        destructionEffectSpawner = GetComponent<DestructionEffectSpawner>();
        player = GetComponent<PlayerController>();
        idealLookSensitivity = player.lookSensitivityBase;

        currentWeapon = 0;
        shieldUp = false;
        isAttacking = false;
        idealPivotLocalPosition = cameraPivot.localPosition;
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

        //animations might need to go here
        //use isAttacking, isTeleken, etc for animation parameteres

        if (!isLocalPlayer) return;
        // Checks when attack related keys are pressed
        base.Update();

        cycleWeapons();
        energyUser();

        // Ends the shield if no energy remaining
        if (!resourceManager.hasEnergy() && shieldUp) endSecondaryAttack();

        // End attacks if no energy left
        if (isAttacking && !resourceManager.hasEnergy()) endAttack();

        // Digging
        if (isDigging && resourceManager.hasEnergy()) dig(hit);

        // Damaging
        if (isDamaging && resourceManager.hasEnergy()) damage(hit);

        // Trying to pick up something
        if (Input.GetButtonDown("Use")) pickup();

        // Throw grenade
        if (Input.GetKeyDown(KeyCode.G)) CmdSpawnGrenade();


        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("Damaging shield");
            CmdShieldHit(currentShield.gameObject, 10);
        }

        //Animation:
        Animation();

        if (isTelekening)
        {
            idealLookSensitivity = player.lookSensitivityBase * telekenesisLookSensSlowDown;
        }
        else
        {
            idealLookSensitivity = idealLookSensitivity = player.lookSensitivityBase;
        }

        player.lookSens += (idealLookSensitivity - player.lookSens) * 0.25f;
        cameraPivot.localPosition += (idealPivotLocalPosition - cameraPivot.localPosition) * 0.15f;
        Camera.main.fieldOfView += (idealCamFieldOfView - Camera.main.fieldOfView) * 0.15f;
        //Debug.Log("is tele: " + isTelekening + "  look sense: " + player.lookSens);
        if (Input.GetKeyUp(KeyCode.H))
        {
            attackStats.upgrade(PickUpItem.ItemType.HEALER_ARTIFACT);
        }
    }

    void Animation()
    {
        animator.SetBool("isAttacking", isDamaging);
        animator.SetBool("shieldUp", shieldUp);
        animator.SetBool("isDigging", isDigging);
        animator.SetBool("isTelekening", isTelekening);
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
//            playDamageEffect(true);
            dmg.startAttack();
        }
        else if (attackStats.isTelekenetic)
        {
            // Shoot ray from the camera to center of screen
            RaycastHit hitFromCam;
            if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out hitFromCam,
                attackStats.telekenRange, mask))
                return;

            //Debug.Log(hitFromCam.collider.transform.root.name + " " + hitFromCam.collider.transform.root.tag);

            if (!hitFromCam.collider.CompareTag(VOXEL_TAG) &&
                !hitFromCam.collider.transform.root.CompareTag(PLAYER_TAG)) return;

            //Debug.Log("Through first if");

            if (hitFromCam.collider.CompareTag(VOXEL_TAG))
            {
                Voxel voxel = hitFromCam.collider.gameObject.GetComponent<Voxel>();
                if (voxel == null)
                {
                    //Debug.LogError("Voxel doesn't have voxel scipt");
                    return;
                }

                isTelekening = true;

                if (voxel.shatterLevel >= 1)
                    CmdVoxelTeleken(voxel.columnID, voxel.layer, voxel.subVoxelID);
                else
                    CmdVoxelTeleken(voxel.columnID, voxel.layer, "NOTSUB");
            }
            else // Is player
            {
//                CmdEnableTeleken(hitFromHand.transform.root.gameObject);
            }
        }
        else if (attackStats.isDigger)
        {
            playDiggerEffect(true);
            isDigging = true;
        }
    }

    [Command]
    private void CmdEnableTeleken(GameObject rootGameObject)
    {
        RpcEnableTeleken(rootGameObject);
    }

    [ClientRpc]
    private void RpcEnableTeleken(GameObject rootGameObject)
    {
        var telekenScript = rootGameObject.GetComponent<HumanTeleken>();
        telekenScript.enabled = true;
        telekenScript.setUp(telekenObjectPos.transform);
    }

    /// <summary>
    /// Called when mouse 1 released
    /// </summary>
    [Client]
    public override void endAttack()
    {
        timePassed = 0;
        if (attackStats.isTelekenetic)
        {
            isTelekening = false;
            CmdEndTeleken();
            //restore look sens
        }
        else if (attackStats.isDamage)
        {
            isDamaging = false;
//            playDamageEffect(false);
            dmg.endAttack();
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

        if (resourceManager.getEnergy() > attackStats.initialShieldMana && attackStats.isShield && !shieldUp &&
            !isShieldCoolingdown)
        {
            resourceManager.useEnergy(attackStats.initialShieldMana);

            CmdSpawnShield(GetComponent<Identifier>().id);
            shieldUp = true;

            if (getAttackStats().artifactType == PickUpItem.ItemType.HEALER_ARTIFACT)
            {
                idealPivotLocalPosition *= pivotRetraction;
                idealCamFieldOfView *= FOVincrease;
            }
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
            CmdDestroyShield();
            shieldUp = false;

            if (getAttackStats().artifactType == PickUpItem.ItemType.HEALER_ARTIFACT)
            {
                idealPivotLocalPosition /= pivotRetraction;
                idealCamFieldOfView /= FOVincrease;
            }
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
            changeWeapon();
        }

        if (Input.GetKey(KeyCode.Alpha2))
        {
            currentWeapon = 1;
            changeWeapon();
        }

        if (Input.GetKey(KeyCode.Alpha3))
        {
            currentWeapon = 2;
            changeWeapon();
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

        // Shield health gain
        if (!shieldUp && !isShieldCoolingdown)
        {
            attackStats.currentShieldHealth = Math.Min
            (
                attackStats.maxShieldHealth,
                attackStats.currentShieldHealth + Time.deltaTime * attackStats.shieldHealRate
            );
        }


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
        timePassed += Time.deltaTime;
        if (timePassed < waitTime)
            return;
        timePassed = 0;

        // Checking range, need high distance in raycast to orient effect
        if (Mathf.Abs(Vector3.Distance(hitFromCam.point, transform.position)) > attackStats.diggerRange) return;

        // Shoot ray from hand to hit position
        RaycastHit hitFromHand;
        if (!Physics.Linecast(rightHand.position, hitFromCam.point + 2 * cam.transform.forward, out hitFromHand, mask))
            return; // this should never return

        var rootTransform = hitFromHand.collider.transform.root;

        Debug.Log("Hit: " + hitFromHand.collider.tag);

        if (hitFromHand.collider.CompareTag(VOXEL_TAG))
        {
            var voxel = hitFromHand.collider.gameObject.GetComponent<Voxel>();

            // If voxel is about to die
            if (!voxel.hasEnergy && voxel.GetComponent<NetHealth>().getHealth() <= attackStats.diggerEnvDamage)
                destructionEffectSpawner.play(hitFromHand.point, voxel);


            CmdVoxelDamaged(hitFromHand.collider.gameObject, attackStats.diggerEnvDamage);

            // Spawn energy blocks
            if (voxel.hasEnergy)
            {
                energyBlockEffectSpawner.setVoxel(voxel.gameObject);
                energyBlockEffectSpawner.spawnBlock();
            }
        }
        else if (rootTransform.CompareTag(PLAYER_TAG) && !hitFromHand.collider.CompareTag("Shield"))
        {
            createDamageText(rootTransform, attackStats.diggerDamage);
            CmdPlayerAttacked(rootTransform.gameObject.GetComponent<Identifier>().id, attackStats.diggerDamage);
        }
        else if (hitFromHand.collider.CompareTag("Shield"))
        {
            createDamageText(hitFromHand.transform, attackStats.diggerDamage, false, false, true);
            CmdShieldHit(hitFromHand.transform.gameObject, attackStats.diggerDamage);
        }
    }

    /// <summary>
    /// Controls which voxel gets damaged each frame and the orientation of the effect
    /// </summary>
    [Client]
    void damage(RaycastHit hitFromCam)
    {
        timePassed += Time.deltaTime;
        if (timePassed < waitTime)
            return;
        timePassed = 0;
        
        dmg.attack();

//
//        // Checking range, need high distance in raycast to orient effect
//        if (Mathf.Abs(Vector3.Distance(hitFromCam.point, transform.position)) > attackStats.attackRange) return;
//
//        // Shoot ray from hand to hit position
//        RaycastHit hitFromHand;
//        if (!Physics.Linecast(rightHand.position, hitFromCam.point + 2 * cam.transform.forward, out hitFromHand, mask))
//            return; // This should never return
//
//        Debug.Log("Hit: " + hitFromHand.collider.tag);
//
//        var rootTransform = hitFromHand.collider.transform.root;
//
//        if (rootTransform.CompareTag(PLAYER_TAG) && !hitFromHand.collider.CompareTag("Shield"))
//        {
//            var character = rootTransform.gameObject.GetComponent<Identifier>().typePrefix;
//            if (character == Identifier.magicianType) // Heal
//            {
//                createDamageText(rootTransform, attackStats.heal, true);
//                CmdPlayerAttacked(rootTransform.name, -attackStats.heal);
//            }
//            else // Damage
//            {
//                Debug.Log("Hit head: " + (hitFromHand.collider.name == "Head"));
//                float damage = hitFromHand.collider.name == "Head"
//                    ? attackStats.attackDamage * attackStats.headshotMultiplier
//                    : attackStats.attackDamage;
//
//                createDamageText(rootTransform, damage, false, hitFromHand.collider.name == "Head");
//                CmdPlayerAttacked(rootTransform.name, damage);
//            }
//        }
//        else if (hitFromHand.collider.CompareTag(VOXEL_TAG))
//        {
//            CmdVoxelDamaged(hitFromHand.collider.gameObject, attackStats.attackEnvDamage);
//        }
//        else if (hitFromHand.collider.CompareTag("Shield"))
//        {
//            createDamageText(hitFromHand.transform, attackStats.heal, true, false, true);
//            CmdShieldHit(hitFromHand.collider.gameObject, attackStats.heal);
//        }
    }

    #region shield

    /// <summary>
    /// Called on the server to spawn a shield for the local player
    /// </summary>
    [Command]
    public void CmdSpawnShield(string casterID)
    {
        var shieldInst =
            Instantiate(shield, transform.position + transform.up * 5 + transform.right * 1, transform.rotation);
        NetworkServer.Spawn(shieldInst);
        // Servers current shield is not neccaserily the servers instance of shield (is likely local clients instance)
        setUpShield(shieldInst);

        var shieldID = shieldInst.GetComponent<Identifier>().id;

        setUpShieldUI(casterID, shieldID);
        RpcSetUpShieldUI(casterID, shieldID, shieldInst);
    }

    public void setUpShield(GameObject shieldInst)
    {
        currentShield = shieldInst.GetComponent<Shield>();
        var netHealth = currentShield.GetComponent<NetHealth>();
        netHealth.setInitialHealth(attackStats.maxShieldHealth);
        netHealth.setHealth(attackStats.currentShieldHealth);

        // Setting the caster to this magician and setting up UI
        currentShield.setCaster
        (
            GetComponent<Identifier>(),
            attackStats.maxShieldHealth,
            attackStats.currentShieldHealth
        );

        // Allowing it to move with the player
        currentShield.transform.parent = transform;
    }

    /// <summary>
    /// Makes the shield a child of the local player on all clients
    /// </summary>
    [ClientRpc]
    private void RpcSetUpShieldUI(string casterID, string shieldID, GameObject shieldInst)
    {
        // This is not a UI element but is required on all clients for UI to work
        currentShield = shieldInst.GetComponent<Shield>();
        setUpShieldUI(casterID, shieldID);
    }

    private void setUpShieldUI(string casterID, string shieldID)
    {
        GameManager.getObject(shieldID).GetComponent<Shield>().setCaster
        (
            GameManager.getObject(casterID),
            attackStats.maxShieldHealth,
            attackStats.currentShieldHealth
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
    public void shieldDown(float shieldHealth)
    {
        shieldUp = false;
        attackStats.currentShieldHealth = Mathf.Max(0, shieldHealth);

        if (attackStats.currentShieldHealth <= 0)
            StartCoroutine(shieldCooldown());
    }

    private IEnumerator shieldCooldown()
    {
        isShieldCoolingdown = true;
        yield return new WaitForSeconds(attackStats.shieldCooldownTime);
        isShieldCoolingdown = false;
    }

    #endregion

    #region teleken

    /// <summary>
    /// Allows the player to control a voxel
    /// </summary>
    [Command]
    private void CmdVoxelTeleken(int col, int layer, string subID)
    {
        if (subID == "NOTSUB")
        {
            currentTelekeneticVoxel = MapManager.manager.voxels[layer][col].gameObject;
        }
        else
        {
            currentTelekeneticVoxel = MapManager.manager.getSubVoxelAt(layer, col, subID).gameObject;
        }

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
        if (subID == "NOTSUB")
        {
            currentTelekeneticVoxel = MapManager.manager.voxels[layer][col].gameObject;
        }
        else
        {
            currentTelekeneticVoxel = MapManager.manager.getSubVoxelAt(layer, col, subID).gameObject;
        }

        var voxel = currentTelekeneticVoxel.GetComponent<Voxel>();
        // Creating the voxels below
        //Debug.Log("Show neighbours " + (subID == "NOTSUB"));
        voxel.showNeighbours(subID == "NOTSUB");

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

        var tele = currentTelekeneticVoxel.GetComponent<Telekinesis>();
        tele.enabled = true;
        tele.setUp(telekenObjectPos.transform, Telekinesis.VOXEL, GetComponent<Identifier>().id, isServer);
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

    #region MagicGrenade

    [Command]
    void CmdSpawnGrenade()
    {
        var magicGrenade = Instantiate(magicGrenadeFX, rightHand.position, cam.transform.rotation);
        magicGrenade.GetComponent<MagicGrenade>().setCaster(gameObject);
        NetworkServer.Spawn(magicGrenade);
    }

    #endregion

    public void pickup()
    {
        RaycastHit hit;
        if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, pickupDistance, mask))
            return;

        PickUpItem item = hit.transform.gameObject.GetComponentInChildren<PickUpItem>(); // Pickup item lives on parent

        if (item == null) return;

        if (item.itemClass == PickUpItem.Class.MAGICIAN)
        {
            attackStats.upgrade(item.itemType);
            item.pickedUp();
        }
    }

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

    private void createDamageText(Transform hit, float damage, bool isHealing = false, bool isHeadshot = false,
        bool isShield = false)
    {
        float posUp = isShield ? 10 : 15;
        Instantiate
        (
            damageTextIndicatorEffect,
            hit.position + hit.up * posUp,
            hit.rotation
        ).GetComponent<TextDamageIndicator>().setUp((int) damage, isHealing, isHeadshot);
    }

    #endregion

    public MagicType getAttackStats()
    {
        return attackStats;
    }
}