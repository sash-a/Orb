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
        Debug.Log("Damage Weapon");
        
        isDamage = true;
        isTelekenetic = false;
        isForcePush = false;
    }
    
    public void changeToTeleken()
    {
        Debug.Log("Teleken Weapon");

        isDamage = false;
        isTelekenetic = true;
        isForcePush = false;
    }
    public void changeToPush()
    {
        Debug.Log("Push Weapon");

        isDamage = false;
        isTelekenetic = false;
        isForcePush = true;
    }
}