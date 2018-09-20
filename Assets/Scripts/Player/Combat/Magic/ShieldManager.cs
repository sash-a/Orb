using System.Collections;
using UnityEngine;
using UnityEngine.Networking;


public class ShieldManager : NetworkBehaviour
{
    public GameObject shieldPrefab;
    public Shield currentShield;

    public bool isShielding { get; private set; }

    public bool isShieldCoolingdown { get; private set; }

    public float maxHealth;
    public float currentHealth;
    public float healRate;
    public float cooldownTime;
    public float initialMana;
    public float mana;

    public bool hasArtifact;

    public void spawnShield(string casterID)
    {
        CmdSpawnShield(casterID);
        isShielding = true;
    }

    public void destroyShield()
    {
        CmdDestroyShield();
        isShielding = false;
    }

    public void upgrade(PickUpItem.ItemType artifactType)
    {
        if (artifactType != PickUpItem.ItemType.HEALER_ARTIFACT || artifactType != PickUpItem.ItemType.LESSER_ARTIFACT)
            return;

        hasArtifact = artifactType == PickUpItem.ItemType.HEALER_ARTIFACT;
        float modifier = 1.3f;

        if (artifactType == PickUpItem.ItemType.LESSER_ARTIFACT)
            modifier = 1.1f;


        maxHealth *= modifier;
        currentHealth = maxHealth;
        initialMana /= modifier;
        healRate *= modifier;
        mana /= modifier;
        cooldownTime /= modifier;
    }

    public void downgrade()
    {
        if (!hasArtifact)
            return;

        hasArtifact = false;
        float modifier = 1.3f;

        maxHealth /= modifier;
        initialMana *= modifier;
        healRate /= modifier;
        mana *= modifier;
        cooldownTime *= modifier;
    }


    /// <summary>
    /// Called on the server to spawn a shield for the local player
    /// </summary>
    [Command]
    private void CmdSpawnShield(string casterID)
    {
        var shieldInst =
            Instantiate(shieldPrefab, transform.position + transform.up * 5 + transform.right * 1, transform.rotation);
        NetworkServer.Spawn(shieldInst);
        // Servers current shield is not neccaserily the servers instance of shield (is likely local clients instance)
        setUpShield(shieldInst);

        var shieldID = shieldInst.GetComponent<Identifier>().id;

        setUpShieldUI(casterID, shieldID);
        RpcSetUpShieldUI(casterID, shieldID, shieldInst);
    }

    private void setUpShield(GameObject shieldInst)
    {
        currentShield = shieldInst.GetComponent<Shield>();
        var netHealth = currentShield.GetComponent<NetHealth>();
        netHealth.setInitialHealth(maxHealth);
        netHealth.setHealth(currentHealth);

        // Setting the caster to this magician and setting up UI
        currentShield.setCaster
        (
            GetComponent<Identifier>(),
            maxHealth,
            currentHealth
        );

        // Allowing it to move with the player
        currentShield.transform.parent = transform;
    }

    /// <summary>
    /// Makes the shield a child of the local player on all clients
    /// </summary>
    [ClientRpc]
    private void RpcSetUpShieldUI(string casterID, string shieldID, GameObject shieldInst)
    {
        // This is not a UI element but is required on all clients for UI to work
        currentShield = shieldInst.GetComponent<Shield>();
        setUpShieldUI(casterID, shieldID);
    }

    private void setUpShieldUI(string casterID, string shieldID)
    {
        GameManager.getObject(shieldID).GetComponent<Shield>().setCaster
        (
            GameManager.getObject(casterID),
            maxHealth,
            currentHealth
        );
        GameManager.getObject(shieldID).transform.parent = GameManager.getObject(casterID).transform;
    }

    /// <summary>
    /// Destroys the currently active shield for the local player on all clients
    /// </summary>
    [Command]
    private void CmdDestroyShield()
    {
        if (currentShield == null) return;

        Destroy(currentShield.gameObject);
        NetworkServer.Destroy(currentShield.gameObject);
    }

    /// <summary>
    /// Sets <code>shieldUp</code> to false
    /// </summary>
    public void shieldDown(float shieldHealth)
    {
        isShielding = false;
        currentHealth = Mathf.Max(0, shieldHealth);

        if (currentHealth <= 0)
            StartCoroutine(shieldCooldown());
    }

    private IEnumerator shieldCooldown()
    {
        isShieldCoolingdown = true;
        yield return new WaitForSeconds(cooldownTime);
        isShieldCoolingdown = false;
    }
}