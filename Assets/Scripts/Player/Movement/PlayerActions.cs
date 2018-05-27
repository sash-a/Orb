﻿using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerActions : MonoBehaviour
{
    Vector3 velocity;
    Vector3 rotation;

    [SerializeField] private float camRotLimitX = 180f;
    private float camRotationX;
    private float currentCamRotX = 0f;

    [SerializeField] private Camera cam;

    private Rigidbody rb;

    private bool isJumping;
    bool needsHop;
    Vector3 lastFramePos;

    void Start()
    {
        initVars();
    }

    private void initVars()
    {
        velocity = Vector3.zero;
        rb = GetComponent<Rigidbody>();

        isJumping = false;
        needsHop = false;
        lastFramePos = transform.position;
    }

    void FixedUpdate()
    {
        Vector3 forward = getFoward();
        if (!forward.Equals(Vector3.zero) && !transform.position.Equals(Vector3.zero))
        {
            rb.MoveRotation(Quaternion.LookRotation(forward, -transform.position.normalized));
        }

        doMovement();
        doRotations();
    }

    // TODO this should be move to a utility/player properites class
    public Vector3 getFoward()
    {
        var up = -transform.position.normalized;
        var foward = Vector3.Cross(up, transform.right);

        if (Vector3.Dot(foward, transform.forward) < 0)
        {
            foward *= -1;
        }

        return foward;
    }

    public void move(Vector3 _velocity)
    {
        velocity = _velocity;
    }

    void doMovement()
    {
        if (velocity.magnitude > 0)
        {
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
            //if has been stopped from moving
            if (needsHop &&
                Vector3.Distance(lastFramePos, transform.position) < (velocity * Time.fixedDeltaTime).magnitude * 0.5f
                && Vector3.Dot(velocity.normalized, transform.forward) > 0.7) // was mostly moving forward
            {
                //Debug.Log("\t\tstopped from moving forwards");
                rb.MovePosition(transform.position - transform.forward * 0.1f);
                rb.AddForce(-transform.position.normalized * 2f, ForceMode.VelocityChange);
            }
        }

        lastFramePos = transform.position;
    }

    public void rotate(Vector3 _rotation, float _camRot)
    {
        rotation = _rotation;
        camRotationX = _camRot;
    }

    void doRotations()
    {
        // Rotating player around y
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
        // Rotating cam around the x axis
        currentCamRotX -= camRotationX;
        currentCamRotX = Mathf.Clamp(currentCamRotX, -camRotLimitX, camRotLimitX);
        cam.transform.localEulerAngles = new Vector3(currentCamRotX, 0, 0);
    }

    public void jump()
    {
        if (isJumping)
        {
            isJumping = false;
            rb.AddForce(-transform.position.normalized * 900);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        isJumping = other.gameObject.name.Equals("TriVoxel");
        if (other.gameObject.name.Equals("TriVoxel"))
        {
            Voxel vox = other.gameObject.GetComponent<Voxel>();
            if (MapManager.isDeleted(vox.layer - 2, vox.columnID) || MapManager.isDeleted(vox.layer - 1, vox.columnID))
            // &&  vox.deletedPoints.Count==0)//difficult to tell if infront it has deleted points
            {
                Vector3 diff = transform.position - vox.worldCentreOfObject;
                float upness = Vector3.Dot(transform.position.normalized, diff.normalized);

                if (Mathf.Abs(upness) < 0.2)
                {
                    needsHop = true;
                }
            }
            else
            {
                needsHop = false;
            }
        }
    }
}