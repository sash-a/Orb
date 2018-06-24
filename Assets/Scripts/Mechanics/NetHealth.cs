using UnityEngine;
using UnityEngine.Networking;

public class NetHealth : NetworkBehaviour
{
    [SerializeField] [SyncVar] private float health;
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
        setInitialHealth(maxHealth);
    }

    [ClientRpc]
    public void RpcDamage(float amount)
    {
        if (isDead) return;

        health -= amount;
        if (isServer)
        {
            if (health <= 0) die();
        }
    }

    // This completely removes the object from the game
    // TODO specilize for player (disable and set cam)
    private void die()
    {
        isDead = true;

        if (gameObject.name.Contains("voxel")|| gameObject.name.Contains("Voxel") || gameObject.name.Equals("TriVoxel"))
        {
            var voxel = gameObject.GetComponent<Voxel>();
            if (voxel.layer < MapManager.mapLayers - 1) voxel.destroyVoxel();
        }
        else
        {
            NetworkServer.Destroy(gameObject);
        }
    }

    public float getHealth()
    {
        return health;
    }

    public void setInitialHealth(float maxHealth)
    {
        this.maxHealth = maxHealth;
        this.health = maxHealth;
    }

    public float getHealthPercent()
    {
        return health / maxHealth;
    }
}