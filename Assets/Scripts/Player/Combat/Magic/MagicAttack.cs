using System;
using System.Collections;
using System.Collections.Generic;
using Player.Combat.Magic.Attacks;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(ResourceManager))]
public class MagicAttack : AAttackBehaviour
{
    #region variables

    [SerializeField] private MagicType attackStats;

    [SerializeField] private Transform rightHand;

    // Shield
    private Shield currentShield; // The current instance of shield
    [SerializeField] private GameObject shield;
    private bool shieldUp; // True if the player is currently using a shield
    private bool isShieldCoolingdown;

    public GameObject magicGrenadeFX;

    public DamageType damage;
    public DamageType digger;
    public TelekinesisType telekin;


    /// <summary>
    /// 0 = Digger
    /// 1 = Damage/Heal
    /// 2 = Telekinesis
    /// </summary>
    [SyncVar] public int spellIndex;

    private List<SpellType> spells;
    private SpellType currentSpell;

    [SerializeField] private float pickupDistance;

    // Used so that commands are not passed every frame
    [SerializeField] private float waitTime;
    private float timePassed;

    //Animation:
    public Animator animator;

    // Viewing variables
    PlayerController player;

    float idealLookSensitivity;
    float telekenesisLookSensSlowDown = 0.4f;

    public Transform cameraPivot;
    Vector3 idealPivotLocalPosition;
    float idealCamFieldOfView = 65f;
    float pivotRetraction = 2.5f;
    float FOVincrease = 1.4f;

    #endregion


    void Start()
    {
        // Initial weapon selection
        spells = new List<SpellType> {digger, damage, telekin};
        spellIndex = 0;
        shieldUp = false;

        if (!isLocalPlayer) return;

        resourceManager = GetComponent<ResourceManager>();
        player = GetComponent<PlayerController>();

        idealLookSensitivity = player.lookSensitivityBase;
        idealPivotLocalPosition = cameraPivot.localPosition;

        playEffect(false);
    }

    void Update()
    {
        currentSpell = spells[spellIndex];

        orientEffects(); // Needs to be done for non-local clients


        if (!isLocalPlayer) return;

        base.Update(); // Checks when attack related keys are pressed

        // Ends the shield if no energy remaining
        if (!resourceManager.hasEnergy() && shieldUp) endSecondaryAttack();

        // End attacks if no energy left
        if (currentSpell.isActive && resourceManager.hasEnergy())
        {
            if (timePassed < waitTime)
            {
                timePassed += Time.deltaTime;
                return;
            }

            currentSpell.attack();
            timePassed = 0;
        }
        else
        {
            currentSpell.endAttack();
            timePassed = 0;
        }

        energyUser();
        cycleWeapons();
        // Trying to pick up something
        if (Input.GetButtonDown("Use")) pickup();

        // Throw grenade
        if (Input.GetKeyDown(KeyCode.G)) CmdSpawnGrenade();

        //Animation:
        Animation(currentSpell);

        setUpCam();
    }

    void Animation(SpellType currentSpell)
    {
        animator.SetBool("isAttacking", currentSpell.isActive && currentSpell.name == SpellType.ATTACK_TYPE);
        animator.SetBool("shieldUp", shieldUp);
        animator.SetBool("isDigging", currentSpell.isActive && currentSpell.name == SpellType.DIGGER_TYPE);
        animator.SetBool("isTelekening", currentSpell.isActive && currentSpell.name == SpellType.TELEKINESIS_TYPE);
    }

    [Client]
    public override void attack()
    {
        if (!MapManager.manager.mapDoneLocally)
        {
            Debug.LogError("Attacking before map finished");
            return;
        }

        currentSpell.startAttack();
        playEffect(currentSpell.isActive);
        CmdSetSpellActive(true);
    }

    /// <summary>
    /// Called when mouse 1 released
    /// </summary>
    [Client]
    public override void endAttack()
    {
        timePassed = 0;
        spells[spellIndex].endAttack();
        playEffect(false);
        CmdSetSpellActive(false);
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

    private void cycleWeapons()
    {
        if (!isLocalPlayer || currentSpell.isActive) return;

        var scroll = Input.GetAxis("Mouse ScrollWheel");

        // I understand the direction makes no sense, but it works better for the UI
        if (scroll < 0f)
        {
            spellIndex = ++spellIndex % 3;
        }
        else if (scroll > 0f)
        {
            if (spellIndex == 0)
                spellIndex = 2;
            else
                spellIndex = --spellIndex % 3;
        }

        if (Input.GetKey(KeyCode.Alpha1)) spellIndex = 0;

        if (Input.GetKey(KeyCode.Alpha2)) spellIndex = 1;

        if (Input.GetKey(KeyCode.Alpha3)) spellIndex = 2;

        if (isLocalPlayer)
            CmdSetSpellIndexServer(spellIndex);
    }

    /// <summary>
    /// Drains and gains energy depending on active spells
    /// </summary>
    private void energyUser()
    {
        // Mana gain
        if (!shieldUp && !currentSpell.isActive) resourceManager.gainEnery(attackStats.manaRegen * Time.deltaTime);

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
        // Attacks
        if (currentSpell.isActive) resourceManager.useEnergy(currentSpell.mana * Time.deltaTime);

        // Shield
        if (shieldUp) resourceManager.useEnergy(attackStats.shieldMana * Time.deltaTime);
    }

    private void setUpCam()
    {
        if (currentSpell.name == SpellType.TELEKINESIS_TYPE)
            idealLookSensitivity = player.lookSensitivityBase * telekenesisLookSensSlowDown;
        else
            idealLookSensitivity = idealLookSensitivity = player.lookSensitivityBase;

        player.lookSens += (idealLookSensitivity - player.lookSens) * 0.25f;
        cameraPivot.localPosition += (idealPivotLocalPosition - cameraPivot.localPosition) * 0.15f;
        Camera.main.fieldOfView += (idealCamFieldOfView - Camera.main.fieldOfView) * 0.15f;
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

    private void orientEffects()
    {
        var activeSpell = spells[spellIndex]; // can't use current spell because index is the sync var
        if (activeSpell.isActive)
        {
            Debug.Log("Orienting on local player " + isLocalPlayer);
            RaycastHit hit;
            if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 1000, mask)) return;

            activeSpell.fx.transform.LookAt(hit.point);
        }
    }

    /// <summary>
    /// Sets the current spell on the server because sync vars do not sync to the server
    /// </summary>
    /// <param name="index"></param>
    [Command]
    void CmdSetSpellIndexServer(int index)
    {
        spellIndex = index;
    }

    // Sets a spell active on the server and on all clients
    #region activeate spells

    [Command]
    void CmdSetSpellActive(bool active)
    {
        setSpellActive(active);
        RpcSetSpellActive(active);
    }

    [ClientRpc]
    void RpcSetSpellActive(bool active)
    {
        setSpellActive(active);
    }

    private void setSpellActive(bool active)
    {
        spells[spellIndex].isActive = active;
    }

    #endregion
    

    /*
     * Playing partilce effects on all clients
     */
    void playEffect(bool isPlaying)
    {
        if (isLocalPlayer)
        {
            CmdPlayEffect(isPlaying);
        }

        if (isPlaying)
        {
            spells[spellIndex].fx.Play();
            return;
        }

        spells[spellIndex].fx.Stop();
    }

    [Command]
    void CmdPlayEffect(bool isPlaying)
    {
        RpcPlayEffect(isPlaying);
    }

    [ClientRpc]
    void RpcPlayEffect(bool isPlaying)
    {
        if (isLocalPlayer) return;

        playEffect(isPlaying);
    }


    public MagicType getAttackStats()
    {
        return attackStats;
    }
}