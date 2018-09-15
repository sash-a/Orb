using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Scope : MonoBehaviour {

    public Animator animator;
    public bool scopedIn = false;

    public GameObject ScopeOverlay;
    public Camera weaponCamera;

    public Camera mainCamera;
    //public Camera weaponCam;

    public bool isLocalPlayer=false;

   public WeaponAttack playerAttack;

    private void Start()
    {
        //weaponCamera.SetActive(false);
        if (mainCamera == null) {
            Debug.LogError("no main camera attached to sniper scsope script");
        }
    }

    private void Update()
    {

        if (!isLocalPlayer) {
            ScopeOverlay.SetActive(false);
            weaponCamera.gameObject.SetActive(false);
            return;
        }

        weaponCamera.fieldOfView = playerAttack.camAngle;
        Camera.main.fieldOfView = playerAttack.camAngle;
        mainCamera.fieldOfView = playerAttack.camAngle;

        if (animator.GetBool("isReloading") || Input.GetKey(KeyCode.LeftShift) || !Input.GetButton("Fire2"))
        {//unscope
            unscoped();
            scopedIn = false;
        }
        else {
            scoped();
            scopedIn = true;
        }

        //Debug.Log("sniper scoped: " + scopedIn);
    }

    void unscoped()
    {
        //remove scope image
        ScopeOverlay.SetActive(false);
        //enables camera that sees player
        weaponCamera.gameObject.SetActive(true);
        //reset zoom
    }

    void scoped()
    {
        //display scope picture
        ScopeOverlay.SetActive(true);
        //disable camera that sees player (when scoping)
        weaponCamera.gameObject.SetActive(false);
        //mainCamera.fieldOfView;

    }

    private void OnDisable()
    {
        unscoped();
        scopedIn = false;
    }

    //Note: use of second camera also good for when player is colliding with other objects, preventing gun from clipping through
    //may cause issues down the line, but keeping it for now
}
