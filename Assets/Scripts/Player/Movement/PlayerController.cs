﻿using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerActions))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed;
    public float lookSensitivityBase;
    public float lookSens;

    [SerializeField] private float jumpForce;
    [SerializeField] private float runMultiplier;
    private PlayerActions actions;
    Gravity gravity;

    //Animation
    public Animator animator;
    private bool isMoving = false;
    private float MouseY = 0.0f;
    //NB variables for walking/running Blend animations
    private float xMoveOld = 0;
    private float yMoveOld = 0;
    private float xMove = 0;
    private float yMove = 0;
    private float interpSpeed = 0.25f;

    internal bool isInSphere()
    {
        return gravity.inSphere;
    }

    public Team team;

    void Start()
    {
        actions = GetComponent<PlayerActions>();
        gravity = GetComponent<Gravity>();
        lookSens = lookSensitivityBase;
        gravity.inSphere = false;
        //sendToSpawnRoom();
        StartCoroutine(AddPlayer());

       

        //Debug.Log("look sens: " + lookSens);
    }

    IEnumerator AddPlayer()
    {
        yield return new WaitForSecondsRealtime(1f);
        TeamManager.singleton.addPlayer(this);
    }

    void Update()
    {
        if (PlayerUI.isPaused)
        {
            actions.move(Vector3.zero);
            actions.rotate(Vector3.zero, 0);
            return;
        }

        // Movement
        var xMov = Input.GetAxis("Horizontal") * transform.right;
        var yMov = Input.GetAxis("Vertical") * transform.forward;
        var velocity = (xMov + yMov).normalized * speed;

        if (transform.name.Contains("unner") && !animator.GetBool("isReloading") && !animator.GetBool("isShooting"))
        {
            actions.move(Input.GetKey(KeyCode.LeftShift) ? velocity * runMultiplier : velocity);
        }
        else
        {
            actions.move(Input.GetKey(KeyCode.LeftShift) ? velocity * 1 : velocity);
        }

        if (transform.name.Contains("agician"))
        {

            actions.move(Input.GetKey(KeyCode.LeftShift) ? velocity * runMultiplier : velocity);
        }

        // Rotation
        var yRot = new Vector3(0, Input.GetAxis("Mouse X"), 0) * lookSens * Time.deltaTime;
        float xRot = Input.GetAxis("Mouse Y") * lookSens * Time.deltaTime;

        actions.rotate(yRot, xRot);

        if (Input.GetButtonDown("Jump"))
            actions.jump(jumpForce);

        //Animation
        MovementAnimation(velocity);

        if (MapManager.manager != null && MapManager.manager.warningShell != null)
        {
            if (MapManager.manager.isInWarningZone(transform.position))
            {
                new UIMessage("WARNING! You are in the shredding zone!", 1);
                DynamicLightingController.singleton.informInShredZone();
                //Debug.Log("player is in warning zone!");
            }
        }

    }

    internal void sendToSpawnRoom()
    {
        GetComponent<Gravity>().inSphere = false;
        transform.position = team.getSpawnRoom().transform.position;
        transform.rotation = Quaternion.LookRotation(new Vector3(transform.position.x, 0, transform.position.z), Vector3.up);
    }

    void MovementAnimation(Vector3 velocity)
    {
        //Animation:
        yMove = Input.GetAxis("Vertical");
        xMove = Input.GetAxis("Horizontal");

        if (transform.name.Contains("unner") && !animator.GetBool("isReloading") && !animator.GetBool("isShooting"))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                yMove *= 2;
                xMove *= 2;
            }
        }

        if (transform.name.Contains("agician"))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                yMove *= 2;
                xMove *= 2;
            }
        }

        yMoveOld = yMoveOld + (yMove - yMoveOld) * (interpSpeed / (Mathf.Round(yMove) == 0 ? 1 : (float)Mathf.Abs(Mathf.Round(yMove))));
        xMoveOld = xMoveOld + (xMove - xMoveOld) * (interpSpeed / (Mathf.Round(xMove) == 0 ? 1 : (float)Mathf.Abs(Mathf.Round(xMove))));

        if (Input.GetButtonDown("Jump"))
        {
            animator.SetTrigger("jump");
        }

        if (velocity != Vector3.zero)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        //aiming up and down
        //MouseY = Input.GetAxis("Mouse Y");
        if (Input.GetKeyDown(KeyCode.X) && MouseY == 0.0f)
        {
            MouseY = 1.0f;
        }
        else if(Input.GetKeyDown(KeyCode.X) && MouseY == 1.0f)
        {
            MouseY = 0.0f;
        }

        animator.SetBool("isMoving", isMoving);
        animator.SetFloat("xMove", xMoveOld);
        animator.SetFloat("yMove", yMoveOld);
        animator.SetFloat("MouseY", MouseY);
    }



}