using UnityEngine;

//collab is a cunt
public class WeaponSwitch : MonoBehaviour {
    
    public WeaponAttack attack;

	void Start ()
    {
        SelectWeapon();
	}

	void Update ()
    {
        SelectWeapon();
    }

    void SelectWeapon()
    {
        //index
        int i = 0;
        //only the current weapon we are using will be enabled at a time
        foreach (Transform weapon in transform)
        {
            if (i == attack.selectedWeapon)
            {
                weapon.gameObject.SetActive(true);
            }
            else
            {
                weapon.gameObject.SetActive(false);
            }

            i++;
        }
    }
}
