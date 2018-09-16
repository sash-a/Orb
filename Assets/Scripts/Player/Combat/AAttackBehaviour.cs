using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;

public abstract class AAttackBehaviour : NetworkBehaviour
{
    public const string PLAYER_TAG = "Player";
    public const string VOXEL_TAG = "TriVoxel";

    // Local players camera.
    [SerializeField] protected Camera cam;

    // Layers that the local player can attack.
    [SerializeField] protected LayerMask mask;

    // Manages ammo and energy for players
    protected ResourceManager resourceManager;

    public DamageIndicator damageIndicator;

    void Start()
    {
        if (cam == null)
        {
            Debug.LogError("Player shoot: no camera referenced");
            enabled = false;
        }
    }

    protected void Update()
    {
        if (PlayerUI.isPaused) return;

        if (Input.GetButtonDown("Fire1")) attack();
        if (Input.GetButtonUp("Fire1")) endAttack();

        if (Input.GetButtonDown("Fire2")) secondaryAttack();
        if (Input.GetButtonUp("Fire2")) endSecondaryAttack();
    }

    [Client]
    public abstract void attack();

    [Client]
    public abstract void endAttack();

    [Client]
    public abstract void secondaryAttack();

    [Client]
    public abstract void endSecondaryAttack();

    /// <summary>
    /// Notifies server that player has been shot
    /// </summary>
    /// <param name="id">The ID of the player being shot as provided in the game manager class</param>
    /// <param name="damage">The amount of damage to do to the player</param>
    [Command]
    public void CmdPlayerAttacked(string id, float damage)
    {
        var player = GameManager.getObject(id);
        var health = player.GetComponent<NetHealth>();

        if (health == null)
        {
            Debug.LogError("Player did not have health component");
            return;
        }

        var playerIdentifier = player.GetComponent<Identifier>();
        TargetDamageIndicator
        (
            player.GetComponent<NetworkIdentity>().connectionToClient,
            GetComponent<Identifier>().id,
            playerIdentifier.id,
            playerIdentifier.typePrefix
        );
        Debug.Log("Calling dmg indicator");

        health.RpcDamage(damage);
    }

    /// <summary>
    /// Notifies the server that a voxel has been hit
    /// </summary>
    /// <param name="go">GameObject that was hit</param>
    /// <param name="damage">Damage to be done to the voxel</param>
    [Command]
    public virtual void CmdVoxelDamaged(GameObject go, float damage)
    {
        if (go == null)
        {
            Debug.LogError("trying to damage null voxel");
            return;
        }

        var health = go.GetComponent<NetHealth>();
        if (health == null)
        {
            Debug.LogError("Voxel did not have health component");
            return;
        }
        Debug.Log("damaging voxel health");
        health.RpcDamage(damage);
    }


    [Command]
    public void CmdShieldHit(GameObject go, float damage)
    {
        Debug.Log("CmdShieldHit");
        // This should be the shields health
        var shield = go.GetComponent<NetHealth>();
        if (shield == null)
        {
            Debug.LogError("No shield");
            return;
        }

        shield.RpcDamage(damage);
    }

    /// <summary>
    /// Returns the resource manager for this player
    /// </summary>
    /// <returns>The <code>ResourceManager</code> of the local player</returns>
    public ResourceManager getResourceManager()
    {
        return resourceManager;
    }

    [TargetRpc]
    void TargetDamageIndicator(NetworkConnection client, string shooterID, string victim, string victimClass)
    {
        Debug.Log("In targetRpc");
        if (!isLocalPlayer)
        {
            Debug.Log("Not local player");
        }

        var shooter = GameManager.getObject(shooterID);
        var shot = GameManager.getObject(victim);

        if (victimClass == Identifier.gunnerType)
        {
            shot.GetComponent<WeaponAttack>().damageIndicator.hit(shooter.transform);
            Debug.Log("hit gunner");
        }
        else
        {
            Debug.Log("Hit magician");
            shot.GetComponent<MagicAttack>().damageIndicator.hit(shooter.transform);
        }
    }
}