using UnityEngine;

[System.Serializable]
public class MagicType : Item
{
    #region Variables

    // Type of magic
    public bool isTelekenetic;
    public bool isDamage;
    public bool isShield;
    public bool isForcePush;
    public bool isDigger;

    // Magic stats
    public float manaRegen;
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
}