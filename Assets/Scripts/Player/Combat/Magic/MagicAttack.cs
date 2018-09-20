using System;
using System.Collections.Generic;
using Player.Combat.Magic.Attacks;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(ResourceManager))]
public class MagicAttack : AAttackBehaviour
{
    #region variables

    public Transform rightHand;

    public ShieldManager shieldManager { get; private set; }

    [SerializeField] private GameObject magicGrenadeFX;

    [SerializeField] private float manaRegen;
    /// <summary>
    /// 0 = Digger
    /// 1 = Damage/Heal
    /// 2 = Telekinesis
    /// </summary>
    [SyncVar] public int spellIndex;

    public List<SpellType> spells;
    private SpellType currentSpell;

    public DamageType damage;
    public DamageType digger;
    public TelekinesisType telekin;

    // Used so that commands are not passed every frame
    [SerializeField] private float waitTime;
    private float timePassed;

    //Animation
    public Animator animator;

    #region Viewing variables

    PlayerController player;

    float idealLookSensitivity;
    float telekenesisLookSensSlowDown = 0.4f;

    public Transform cameraPivot;
    Vector3 idealPivotLocalPosition;
    float idealCamFieldOfView = 65f;
    float pivotRetraction = 2.5f;
    float FOVincrease = 1.4f;

    #endregion

    #endregion

    void Start()
    {
        // Initial weapon selection
        spells = new List<SpellType> {digger, damage, telekin};
        spellIndex = 0;

        if (!isLocalPlayer) return;

        resourceManager = GetComponent<ResourceManager>();
        player = GetComponent<PlayerController>();
        shieldManager = GetComponent<ShieldManager>();

        idealLookSensitivity = player.lookSensitivityBase;
        idealPivotLocalPosition = cameraPivot.localPosition;

        for (int i = 0; i < spells.Count; i++)
            spells[i].equippedIndex = i;
    }

    void Update()
    {
        currentSpell = spells[spellIndex];
        orientEffects(); // Needs to be done for non-local clients


        if (!isLocalPlayer) return;

        base.Update(); // Checks when attack related keys are pressed

        // Ends the shield if no energy remaining
        if (!resourceManager.hasEnergy() && shieldManager.isShielding) endSecondaryAttack();

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
        else if (currentSpell.isActive)
        {
            currentSpell.endAttack();
            timePassed = 0;
        }

        energyUser();
        cycleWeapons();

        // Throw grenade
        if (Input.GetKeyDown(KeyCode.G)) CmdSpawnGrenade();

        //Animation
        Animation(currentSpell);

        setUpCam();
    }

    void Animation(SpellType currentSpell)
    {
        animator.SetBool("isAttacking", currentSpell.isActive && currentSpell.name == SpellType.ATTACK_TYPE);
        animator.SetBool("shieldUp", shieldManager.isShielding);
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
    }

    /// <summary>
    /// Called when mouse 1 released
    /// </summary>
    [Client]
    public override void endAttack()
    {
        Debug.Log("ending attack because m1 released");
        timePassed = 0;
        spells[spellIndex].endAttack();
    }

    /// <summary>
    /// Called when mouse2 pressed
    /// Spawns a shield if the player has enough energy
    /// </summary>
    [Client]
    public override void secondaryAttack()
    {
        if (!isLocalPlayer) return;

        if (resourceManager.getEnergy() > shieldManager.initialMana
            && !shieldManager.isShielding && !shieldManager.isShieldCoolingdown)
        {
            resourceManager.useEnergy(shieldManager.initialMana);

            shieldManager.spawnShield(GetComponent<Identifier>().id);

            if (shieldManager.hasArtifact)
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
        shieldManager.destroyShield();

        if (shieldManager.hasArtifact)
        {
            idealPivotLocalPosition /= pivotRetraction;
            idealCamFieldOfView /= FOVincrease;
        }
    }

    private void cycleWeapons()
    {
        if (!isLocalPlayer || currentSpell.isActive) return;

        var scroll = Input.GetAxis("Mouse ScrollWheel");

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
        if (!shieldManager.isShielding && !currentSpell.isActive)
            resourceManager.gainEnery(manaRegen * Time.deltaTime);

        // Shield health gain
        if (!shieldManager.isShielding && !shieldManager.isShieldCoolingdown)
        {
            shieldManager.currentHealth = Math.Min
            (
                shieldManager.maxHealth,
                shieldManager.currentHealth + Time.deltaTime * shieldManager.healRate
            );
        }

        // Mana drain
        // Attacks
        if (currentSpell.isActive) resourceManager.useEnergy(currentSpell.mana * Time.deltaTime);

        // Shield
        if (shieldManager.isShielding) resourceManager.useEnergy(shieldManager.mana * Time.deltaTime);
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

    [Command]
    void CmdSpawnGrenade()
    {
        var magicGrenade = Instantiate(magicGrenadeFX, rightHand.position, cam.transform.rotation);
        magicGrenade.GetComponent<MagicGrenade>().setCaster(gameObject);
        NetworkServer.Spawn(magicGrenade);
    }

    /// <summary>
    /// Orients the direction of effects
    /// </summary>
    private void orientEffects()
    {
        var activeSpell = spells[spellIndex]; // can't use current spell because index is the sync var
        if (activeSpell.isActive)
        {
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
}