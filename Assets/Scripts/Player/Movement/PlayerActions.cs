using System;
using UnityEngine;
using UnityEngine.Networking;


[RequireComponent(typeof(Rigidbody))]
public class PlayerActions : NetworkBehaviour
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

    private void nearPickupItem(PickUpItem item)
    {
        if (item.itemClass == PickUpItem.Class.MAGICIAN)
        {
            if (item.itemType == PickUpItem.ItemType.DAMAGE_ARTIFACT)
            {
                //do something
                //item.pickedUp();//destroys item game object
            }
        }
        else
        {
        }
    }


    void Start()
    {
        initVars();
        if (!isLocalPlayer)
        {
            cam.GetComponent<AudioListener>().enabled = false;
        }
    }

    private void initVars()
    {
        velocity = Vector3.zero;
        rb = GetComponent<Rigidbody>();

        isJumping = false;
        needsHop = false;
    }

    void FixedUpdate()
    {
        Vector3 forward = getFoward();
        if (!forward.Equals(Vector3.zero) && !transform.position.Equals(Vector3.zero))
        {
            //rb.MoveRotation(Quaternion.LookRotation(forward, -transform.position.normalized));
            transform.rotation = Quaternion.LookRotation(forward, -transform.position.normalized);
        }

        doMovement();
        doRotations();
        checkCollectables();

        if (transform.position.magnitude > MapManager.mapSize * 5)
        {
            transform.position = new Vector3(0, -10, 0);
            rb.velocity = Vector3.zero;
        }
    }

    private void checkCollectables()
    {
        if (MapManager.manager == null) return;
        if (MapManager.manager.mapDoneLocally && MapManager.manager.collectables == null) return;
        int count = 0;
        foreach (PickUpItem item in MapManager.manager.collectables)
        {
            count++;
            if (Vector3.Distance(transform.position, item.gameObject.transform.position) < 20)
            {
                nearPickupItem(item);
            }
        }
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

    public void doMovement()
    {
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }

    public void rotate(Vector3 _rotation, float _camRot)
    {
        rotation = _rotation;
        camRotationX = _camRot;
    }

    public void doRotations()
    {
        // Rotating player around y
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
        // Rotating cam around the x axis
        currentCamRotX -= camRotationX;
        currentCamRotX = Mathf.Clamp(currentCamRotX, -camRotLimitX, camRotLimitX);
        cam.transform.localEulerAngles = new Vector3(currentCamRotX, 0, 0);
    }

    public void jump(float jumpForce)
    {
        if (isJumping)
        {
            isJumping = false;
            rb.AddForce(-transform.position.normalized * jumpForce);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        isJumping = other.gameObject.name.Contains("Voxel");
    }
}