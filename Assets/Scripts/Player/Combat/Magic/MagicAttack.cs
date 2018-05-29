using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(ResourceManager))]
public class MagicAttack : AAttackBehaviour
{
    [SerializeField] private MagicType type;
    [SerializeField] private GameObject shield;

    private ResourceManager resourceManager;

    private bool shieldUp; // True if the player is currently using a shield
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
//            shieldEnergyDrain = null;
        }

        if (shieldUp && resourceManager.hasEnergy())
        {
            // Drain energy
        }
    }

    [Client]
    public override void attack()
    {
        if (type.isShield && resourceManager.hasEnergy() && !shieldUp)
        {
            CmdSpawnShield();
            shieldEnergyDrain = resourceManager.beginEnergyDrain(1);
        }
        else if (type.isDamage)
        {
        }
        else if (type.isTelekenetic)
        {
        }
    }

    [Client]
    public override void endAttack()
    {
        CmdDestroyShield();
        resourceManager.endEnergyDrain(shieldEnergyDrain);
        shieldEnergyDrain = null;
    }

    [Command]
    public void CmdSpawnShield()
    {
        currentShield = Instantiate(shield, transform.position, Quaternion.identity);
        currentShield.transform.parent = transform;
        NetworkServer.Spawn(currentShield);
        shieldUp = true;
    }

    [Command]
    public void CmdDestroyShield()
    {
        if (currentShield == null) return;

        Destroy(currentShield);
        NetworkServer.Destroy(currentShield);
        shieldUp = false;
    }
}