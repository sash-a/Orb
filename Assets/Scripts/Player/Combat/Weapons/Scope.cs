using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scope : MonoBehaviour {

    public Animator animator;
    private bool scopedIn = false;

    public GameObject ScopeOverlay;
    public GameObject weaponCamera;

    public Camera mainCamera;
    public float zoom = 15f;
    private float normalZoom;

private void Update()
    {
        if(Input.GetButtonDown("Fire2"))
        {
            scopedIn = !scopedIn;
            animator.SetBool("isScoped", scopedIn);
            
            if(scopedIn)
            {
                StartCoroutine(onScoped());
            }
            else
            {
                onUnscoped();
            }
           
        }
       
    }

    void onUnscoped()
    {
        //remove scope image
        ScopeOverlay.SetActive(false);
        //enables camera that sees weapons
        weaponCamera.SetActive(true);
        //reset zoom
        mainCamera.fieldOfView = normalZoom;
    }

    //coroutine
    IEnumerator onScoped()
    {
        //will wait certain amount of seconds before calling rest of the code (animation transition time)
        yield return new WaitForSeconds(.15f);
        //display scope picture
        ScopeOverlay.SetActive(true);
        //disable camera that sees guns (when scoping)
        weaponCamera.SetActive(false);
        //zooming in (and saving normal view)
        normalZoom = mainCamera.fieldOfView;
        mainCamera.fieldOfView = zoom;
    }

    //Note: use of second camera also good for when player is colliding with other objects, preventing gun from clipping through
    //may cause issues down the line, but keeping it for now
}
