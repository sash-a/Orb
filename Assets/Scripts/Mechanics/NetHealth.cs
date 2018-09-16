using System;
using UnityEngine;
using UnityEngine.Networking;

public class NetHealth : NetworkBehaviour
{
    [SerializeField] [SyncVar] private float health;
    [SerializeField] [SyncVar] private float armour;

    [SerializeField] public float maxArmour;
    [SerializeField] public float maxHealth;

    [SyncVar] private bool _isDead;

    // Do not delete this may be needed later
    // [SerializeField] private Behaviour[] disableOnDeath;

    public bool isDead
    {
        get { return _isDead; }
        set { _isDead = value; }
    }

    void Start()
    {
        _isDead = false;
//        setInitialHealth(maxHealth, maxArmour);
        health = maxHealth;
    }

    /// <summary>
    /// Damages/Heals the gameobject this is connected to on all clients
    /// </summary>
    /// <param name="amount">Amount of damage/healing to do, pass in a negative to heal</param>
    [ClientRpc]
    public void RpcDamage(float amount)
    {
        if (isDead) return;

        // Healing
        if (amount < 0)
        {
            heal(-amount);
            return;
        }

        // Remove damage from armour first
        if (amount >= armour)
        {
            amount -= armour;
            armour = 0;
        }
        else
        {
            armour -= amount;
            return;
        }

        health -= amount;
        if (isServer)
        {
            if (health <= 0)
                die();
            else if (health >= maxHealth)
                health = maxHealth;
        }
    }

    // This completely removes the object from the game
    // TODO specilize for player (disable and set cam)
    private void die()
    {
        //Debug.Log("Calling die!!");
        isDead = true;

        if (gameObject.name.Contains("voxel") || gameObject.name.Contains("Voxel") ||
            gameObject.name.Equals("TriVoxel"))
        {
            var voxel = gameObject.GetComponent<Voxel>();
            if (voxel.layer < MapManager.mapLayers - 1) voxel.CmdDestroyVoxel();
        }
        else
        {
            PlayerController player = GetComponent<PlayerController>();
            if (player != null)
            {
                // Is a player who has died - spawn back in spawn area
                //player.team.RpcInformKilled(player);
                Destroy(gameObject);
            }
            else
            {
                NetworkServer.Destroy(gameObject);
            }
        }
    }

    public float getHealth()
    {
        return health;
    }

    public void setInitialHealth(float maxHealth, float maxArmour = 0, float currentArmour = 0)
    {
        this.maxHealth = maxHealth;
        this.health = maxHealth;

        this.armour = currentArmour;
        this.maxArmour = maxArmour;
    }

    public float getHealthPercent()
    {
        return health / maxHealth;
    }

    public float getArmourPercent()
    {
        return armour / maxArmour;
    }

//    [ClientRpc]
    public void RpcGetArmour(float amount)
    {
        armour = Math.Min(maxArmour, armour + amount);
    }

    public void heal(float amout)
    {
        health = Mathf.Min(health + amout, maxHealth);
    }

    public void setHealth(float amount)
    {
        health = amount;
    }
}