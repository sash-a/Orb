using UnityEngine;

[System.Serializable]
public class MagicType : Item
{
    public bool isTelekenetic;
    public bool isDamage;
    public bool isShield;
    public bool isForcePush;

    public void changeToDamage()
    {        
        isDamage = true;
        isTelekenetic = false;
        isForcePush = false;
    }
    
    public void changeToTeleken()
    {
        isDamage = false;
        isTelekenetic = true;
        isForcePush = false;
    }
    public void changeToPush()
    {
        isDamage = false;
        isTelekenetic = false;
        isForcePush = true;
    }
}