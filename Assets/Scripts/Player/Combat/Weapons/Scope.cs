using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scope : MonoBehaviour {

    public Animator animator;
    private bool scopedIn = false;

    public GameObject ScopeOverlay;
    public GameObject weaponCamera;

    public Camera mainCamera;
    //public Camera weaponCam;
    public float zoom = 100f;
    private float normalZoom;

private void Update()
    {
        if(Input.GetButtonDown("Fire2"))
        {
            scopedIn = !scopedIn;
            
            if(scopedIn && !animator.GetBool("isReloading") && !Input.GetKey(KeyCode.LeftShift))
            {
               onScoped();
            }
            else
            {
                onUnscoped();
            }
           
        }

        if (scopedIn)
        {
            if (animator.GetBool("isReloading") || Input.GetKey(KeyCode.LeftShift))
            {
                onUnscoped();
                scopedIn = false;
            }
        }
        
       
    }

    void onUnscoped()
    {
        //remove scope image
        ScopeOverlay.SetActive(false);
        //enables camera that sees player
        weaponCamera.SetActive(true);
        //reset zoom
        mainCamera.fieldOfView = normalZoom;
    }

    void onScoped()
    {
        //display scope picture
        ScopeOverlay.SetActive(true);
        //disable camera that sees player (when scoping)
        weaponCamera.SetActive(false);
        //zooming in (and saving normal view)
        normalZoom = mainCamera.fieldOfView;
        mainCamera.fieldOfView = zoom;
    }

    private void OnDisable()
    {
        onUnscoped();
        scopedIn = false;
    }

    //Note: use of second camera also good for when player is colliding with other objects, preventing gun from clipping through
    //may cause issues down the line, but keeping it for now
}
