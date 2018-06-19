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
            scopedIn = true;
            StartCoroutine(onScoped());
           
        }

        if (Input.GetButtonUp("Fire2"))
        {
            scopedIn = false;
            onUnscoped();
        }

        animator.SetBool("isScoped", scopedIn);
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
        //will wait certain amount of seconds before calling rest of the code
        yield return new WaitForSeconds(.15f);
        //display scope picture
        ScopeOverlay.SetActive(true);
        //disable camera that sees guns (when scoping)
        weaponCamera.SetActive(false);
        //zoom
        normalZoom = mainCamera.fieldOfView;
        mainCamera.fieldOfView = zoom;
    }

    //Note: use of second camera also good for when player is colliding with other objects, preventing gun from clipping through
}
