﻿using UnityEngine;
using UnityEngine.Networking;

public abstract class AAttackBehaviour : NetworkBehaviour
{
    protected const string PLAYER_TAG = "Player";
    protected const string VOXEL_TAG = "TriVoxel";

    // Local players camera.
    [SerializeField] protected Camera cam;

    // Layers that the local player can attack.
    [SerializeField] protected LayerMask mask;

    void Start()
    {
        if (cam == null)
        {
            Debug.LogError("Player shoot: no camera referenced");
            enabled = false;
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1")) attack();
        if (Input.GetButtonUp("Fire1")) endAttack();
    }

    [Client]
    public abstract void attack();

    [Client]
    public abstract void endAttack();

    /// <summary>
    /// Notifies server that player has been shot
    /// </summary>
    /// <param name="id">The ID of the player as provided in the game manager class</param>
    /// <param name="damage">The amount of damage to do to the player</param>
    [Command]
    public void CmdPlayerAttacked(string id, float damage)
    {
        var player = GameManager.getPlayer(id);
        var health = player.GetComponent<NetHealth>();

        if (health == null)
        {
            Debug.LogError("Player did not have health component");
            return;
        }

        health.RpcDamage(damage);
    }

    /// <summary>
    /// Notifies the server that a voxel has been hit
    /// </summary>
    /// <param name="go">GameObject that was hit</param>
    /// <param name="damage">Damage to be done to the voxel</param>
    [Command]
    protected void CmdVoxelDamaged(GameObject go, float damage)
    {
        var health = go.GetComponent<NetHealth>();
        if (health == null)
        {
            Debug.LogError("Voxel did not have health component");
            return;
        }

        health.RpcDamage(damage);
    }
}