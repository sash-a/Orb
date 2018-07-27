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

        if (Input.GetKey(KeyCode.Alpha1))
        {
            selectedWeapon = 0;
        }

        if (Input.GetKey(KeyCode.Alpha2))
        {
            selectedWeapon = 1;
        }

        if (Input.GetKey(KeyCode.Alpha3))
        {
            selectedWeapon = 2;
        }

        if (Input.GetKey(KeyCode.Alpha4))
        {
            selectedWeapon = 3;
        }

        if (Input.GetKey(KeyCode.Alpha5))
        {
            selectedWeapon = 4;
        }

        //scroll up
        if (Input.GetAxis("Mouse ScrollWheel") > 0f) //shouldnt be able to switch when aiming!
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
