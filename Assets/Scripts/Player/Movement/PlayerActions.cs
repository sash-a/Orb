using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


[RequireComponent(typeof(Rigidbody))]
public class PlayerActions : NetworkBehaviour
{
    public static PlayerActions localActions;
    
    Vector3 velocity;
    Vector3 rotation;

    [SerializeField] private float pickupDistance;
    private AAttackBehaviour attackScript;
    private Identifier id;

    [SerializeField] private float camRotLimitX = 180f;
    private float camRotationX;
    private float currentCamRotX = 0f;

    [SerializeField] private Camera cam;

    private Rigidbody rb;
    Gravity grav;
    NetHealth health;

    Vector3 pivotPoint; //the local position of the cam pivot vs the player on start time - before any controls

    private bool isGroundPlanted;
    private bool isJumping = false;
    private bool hasDoubleJumped = false;

    PlayerController player;


    void Start()
    {
        initVars();
        if (!isLocalPlayer)
        {
            cam.GetComponent<AudioListener>().enabled = false;
            return;
        }
        else
        {
            PlayerController player = GetComponent<PlayerController>();
            if (player != null)
            {
                TeamManager.localPlayer = player;
                DynamicLightingController.localPlayer = player;
            }
            else
            {
                Debug.Log("failed to find player controller component from player action script");
            }
        }

        if (transform.name.Contains("agician"))
        {
            float y = pivotPoint.y;
            //pivotPoint *= 1.2f;//makes magicians camera further away as theyre bigger
        }
    }

    private void initVars()
    {
        pivotPoint = cam.transform.parent.localPosition;
        velocity = Vector3.zero;
        rb = GetComponent<Rigidbody>();
        health = GetComponent<NetHealth>();
        grav = GetComponent<Gravity>();
        player = GetComponent<PlayerController>();
        id = GetComponent<Identifier>();

        if (id.typePrefix == Identifier.magicianType)
            attackScript = GetComponent<MagicAttack>();
        else
            attackScript = GetComponent<WeaponAttack>();

        isGroundPlanted = false;
    }

    internal void deliverPlayerName()
    {
        if (isLocalPlayer)
        {
            Debug.Log("init player name");
            CmdSetPlayerName(TeamManager.localPlayerName);
        }
        else
        {
            Debug.LogError("trying to deliver name from non local player");
        }
    }

    void FixedUpdate()
    {
        Vector3 forward = getFoward();
        if (!forward.Equals(Vector3.zero) && !transform.position.Equals(Vector3.zero))
        {
            transform.rotation = Quaternion.LookRotation(forward, -grav.getDownDir());
        }

        doMovement();
        doRotations();

        if (transform.position.magnitude > MapManager.mapSize * 3.5f && MapManager.manager != null &&
            MapManager.manager.mapDoneLocally) //if you fall out come back in
        {
            //transform.position = new Vector3(0, -10, 0);
            player.spawnOnMap();
            rb.velocity = Vector3.zero;
        }

        if (grav == null)
        {
            grav = GetComponent<Gravity>();
        }

        if (health == null)
        {
            health = GetComponent<NetHealth>();
        }

        if (grav != null && !grav.inSphere && health != null && health.getHealth() > 0 && MapManager.manager != null &&
            MapManager.manager.mapDoneLocally && TeamManager.singleton != null && !TeamManager.localPlayer.spawned)
        {
            //should be in sphere but isnt
            if (GameEventManager.clockTime > 250)
            {
                //enough time has passed that the origonal spawning must have failed
                Debug.LogError("having to respawn players manually after 250 seconds from game start");
                BuildLog.writeLog("having to respawn players manually after 250 seconds from game start");

                grav.inSphere = true;
                TeamManager.singleton.CmdSpawnAllPlayers();
            }
        }
    }

    public Vector3 getFoward()
    {
        var up = -grav.getDownDir();
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
        currentCamRotX = Mathf.Clamp(currentCamRotX, -camRotLimitX + 15, camRotLimitX - 30);
        cam.transform.localEulerAngles = new Vector3(currentCamRotX, 0, 0);

        //Debug.Log("current: " + currentCamRotX + " limit: +-" + camRotLimitX);

        Transform pivot = cam.transform.parent;
        //pivot.localPosition = pivotPoint -grav.getDownDir() * currentCamRotX * 0.1f;
        //pivot.localPosition = pivotPoint - transform.position.normalized * currentCamRotX * 0.1f;
        pivot.localPosition = pivotPoint - Vector3.down * currentCamRotX * 0.1f;
    }

    public void jump(float jumpForce)
    {
        if (isGroundPlanted)
        {
            isGroundPlanted = false;
            rb.AddForce(-grav.getDownDir() * jumpForce);
            isJumping = true;
        }
        else
        {
            if (isJumping && !hasDoubleJumped)
            {
                rb.AddForce(-grav.getDownDir() * jumpForce);
                hasDoubleJumped = true;
            }
        }
    }

    public void pickup()
    {
        var isMagician = id.typePrefix == Identifier.magicianType;
        Debug.Log("in pick is magician: " + isMagician);

        RaycastHit hit;
        if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, pickupDistance,
            attackScript.getLayerMask()))
            return;
        
        Debug.Log("hit " + hit.transform.name);
        
        PickUpItem item = hit.transform.gameObject.GetComponentInChildren<PickUpItem>(); // Pickup item lives on parent

        if (item == null) return;

        if (item.itemClass == PickUpItem.Class.MAGICIAN && isMagician)
        {
            MagicAttack magic = (MagicAttack) attackScript;

            if (item.itemType != PickUpItem.ItemType.LESSER_ARTIFACT)
            {
                Debug.Log("lesser");
                magic.shieldManager.downgrade();
                magic.spells.ForEach(spell => spell.downgrade());
            }

            magic.spells.ForEach(spell => spell.upgrade(item.itemType));
            magic.shieldManager.upgrade(item.itemType);

            Debug.Log("picked up");
            item.pickedUp();
        }
        else if (item.itemClass == PickUpItem.Class.GUNNER && !isMagician)
        {
            // TODO gunner pickup   
        }
    }

    void OnCollisionEnter(Collision other)
    {
        isGroundPlanted = other.gameObject.CompareTag("TriVoxel");
        if (isGroundPlanted)
        {
            isJumping = false;
            hasDoubleJumped = false;
        }
    }

    [Command]
    public void CmdSetPlayerName(string name)
    {
        Debug.Log("cmd player name");

        RpcSetPlayerName(name);
        player.setPlayerName(name, isLocalPlayer);
    }

    [ClientRpc]
    void RpcSetPlayerName(string name)
    {
        Debug.Log("rpc player name: " + name);
        if (player == null)
        {
            player = GetComponent<PlayerController>();
            if (player == null)
            {
                Debug.LogError("cannot find component player controller from player action script");
            }
        }

        if (name != null)
        {
            player.setPlayerName(name, isLocalPlayer);
        }
        else
        {
            player.setPlayerName(gameObject.name, isLocalPlayer);
        }
    }
}