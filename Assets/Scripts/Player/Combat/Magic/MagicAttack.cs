using UnityEngine;

[RequireComponent(typeof(ResourceManager))]
public class MagicAttack : AAttackBehaviour
{
    [SerializeField] private MagicType type;
    [SerializeField] private GameObject shield;

    private ResourceManager resourceManager;

    private bool shieldUp; // True if the player is currently using a shield
    private GameObject currentShield; // The current instance of shield

    void Start()
    {
        resourceManager = GetComponent<ResourceManager>();
        shieldUp = false;
    }

    void Update()
    {
        if (!resourceManager.hasEnergy() && currentShield != null)
        {
            Destroy(currentShield);
            currentShield = null;
            shieldUp = false;
        }
    }

    public override void attack()
    {
        if (type.isShield && resourceManager.hasEnergy() && !shieldUp)
        {
            currentShield = Instantiate(shield); // Position, child to player etc
            shieldUp = true;
            // Drain energy
        }
        else if (type.isDamage)
        {
        }
        else if (type.isTelekenetic)
        {
        }
    }

    public override void endAttack()
    {
        if (currentShield == null) return;

        Destroy(currentShield);
        currentShield = null;
        shieldUp = false;
    }
}