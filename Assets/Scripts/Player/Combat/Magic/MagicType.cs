using UnityEngine;

[System.Serializable]
public class MagicType
{
    #region Variables

    public PickUpItem.ItemType artifactType;
    
    // Type of magic
    public bool isTelekenetic;
    public bool isDamage;
    public bool isShield;
    public bool isForcePush;
    public bool isDigger;

    // Magic stats
    public float manaRegen;

    public float headshotMultiplier;
    
    // Digger
    public float diggerDamage;
    public float diggerEnvDamage;
    public float diggerRange;
    public float diggerMana;

    // Attack
    public float attackDamage;
    public float attackEnvDamage;
    public float attackShieldDamage;
    public float heal;
    public float attackRange;
    public float attackMana;

    // Shield
    public float shieldMana;
    public float initialShieldMana;
    public float shieldHealth;

    // Teleken
    public float telekenMana;
    public float telekenRange;

    #endregion


    public void changeToDamage()
    {
        isDamage = true;
        isTelekenetic = false;
        isForcePush = false;
        isDigger = false;
    }

    public void changeToTeleken()
    {
        isDamage = false;
        isTelekenetic = true;
        isForcePush = false;
        isDigger = false;
    }

    public void changeToPush()
    {
        isDamage = false;
        isTelekenetic = false;
        isForcePush = true;
        isDigger = false;
    }

    public void changeToDigger()
    {
        isDamage = false;
        isTelekenetic = false;
        isForcePush = false;
        isDigger = true;
    }

    public void upgrade(PickUpItem.ItemType artifactType)
    {
        // Remove previous artifact effects
        if (artifactType != PickUpItem.ItemType.NONE)
            downgrade(this.artifactType);
        
        this.artifactType = artifactType;
        manaRegen *= 1.5f;

        new UIMessage("you have picked up the " + artifactType.ToString(),4);

        if (artifactType == PickUpItem.ItemType.DAMAGE_ARTIFACT)
        {
            attackDamage *= 1.8f;
            attackEnvDamage *= 1.2f;
            attackRange *= 1.8f; // TODO extend effect!
            attackMana *= 0.8f;

            new UIMessage("damage and atack range doubled!", 3);


        }
        else if (artifactType == PickUpItem.ItemType.HEALER_ARTIFACT)
        {
            heal *= 1.8f;
            shieldMana *= 0.8f;
            initialShieldMana *= 1.2f;
            shieldHealth *= 1.8f;

            new UIMessage("heal speed and shield helth doubled!", 3);

            // TODO shield type/size!
        }
        else if (artifactType == PickUpItem.ItemType.TELEPATH_ARTIFACT)
        {
            telekenMana *= 0.8f;
            telekenRange *= 1.8f;
            new UIMessage("telekenesis range doubled!", 3);

            // TODO can hit humans and can teleken bigger blocks!
        }
    }

    public void downgrade(PickUpItem.ItemType artifactType)
    {
        manaRegen /= 1.5f;

        if (artifactType == PickUpItem.ItemType.DAMAGE_ARTIFACT)
        {
            attackDamage /= 1.8f;
            attackEnvDamage /= 1.2f;
            attackRange /= 1.8f; // TODO extend effect!
            attackMana /= 1.2f;

        }
        else if (artifactType == PickUpItem.ItemType.HEALER_ARTIFACT)
        {
            heal /= 1.8f;
            shieldMana /= 1.2f;
            initialShieldMana /= 1.2f;
            shieldHealth /= 1.8f;
            // TODO shield type/size!
        }
        else if (artifactType == PickUpItem.ItemType.TELEPATH_ARTIFACT)
        {
            telekenMana /= 1.2f;
            telekenRange /= 1.8f;
            
            // TODO can hit humans and can teleken bigger blocks!
        }
    }
}