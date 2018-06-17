using UnityEngine;

public class WeaponSwitch : MonoBehaviour {
    
    //starting weapon
    private int selectedWeapon = 0;

	// Use this for initialization
	void Start ()
    {
        SelectWeapon();
	}

	// Update is called once per frame
	void Update ()
    {
        int previousSelectedWeapon = selectedWeapon;

        //scroll up
		if(Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (selectedWeapon >= transform.childCount -1)
            {
                selectedWeapon = 0;
            }
            else
            {
                selectedWeapon++;
            }
            
        }
        //scroll down
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (selectedWeapon <= 0)
            {
                selectedWeapon = transform.childCount - 1;
            }
            else
            {
                selectedWeapon--;
            }

        }

        if(previousSelectedWeapon != selectedWeapon)
        {
            SelectWeapon();
        }
    }

    void SelectWeapon()
    {
        //index
        int i = 0;
        //only the current weapon we are using will be enabled at a time
        foreach (Transform weapon in transform)
        {
            if (i == selectedWeapon)
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
