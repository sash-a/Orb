using UnityEngine;

[System.Serializable]
public class MagicType : Item
{
    public bool isTelekenetic;
    public bool isDamage;
    public bool isShield;
    public bool isForcePush;
    public bool isDigger;

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