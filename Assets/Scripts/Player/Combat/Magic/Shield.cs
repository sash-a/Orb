using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Identifier))]
public class Shield : NetworkBehaviour
{
    [SerializeField] private Identifier caster;

    void Start()
    {
        if (isLocalPlayer)
        {
            transform.localPosition = new Vector3(1, 5, 0);
        }
        else
        {
            assignRemoteLayer();
            StartCoroutine(movePosition());
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // TODO is this never called doesn't seem to be registering
        GameManager.register(GetComponent<NetworkIdentity>().netId.ToString(), GetComponent<Identifier>());
    }

    void assignRemoteLayer()
    {
        gameObject.layer = LayerMask.NameToLayer(PlayerSetup.REMOTE_LAYER_NAME);
    }

    public void setCaster(Identifier id, float maxHealth, float currentHealth)
    {
        caster = id;

        var player = caster.gameObject.GetComponent<MagicAttack>();
        // if owns artifact make the shield larger
        if (player.getAttackStats().artifactType == PickUpItem.ItemType.HEALER_ARTIFACT)
            transform.localScale = new Vector3(18f, 9, 18f);

        // Setting the health of the shield needs to be done locally (for UI) and server side (for sync var)
        if (!(player.isLocalPlayer || player.isServer))
            return;

        var netHealth = GetComponent<NetHealth>();
        netHealth.setInitialHealth(maxHealth);
        netHealth.setHealth(currentHealth);

        // UI
        if (player.isLocalPlayer && caster.UI != null)
            ((MagicianUI) caster.UI).onShieldUp(netHealth); // server error on cast
    }


    private void OnDisable()
    {
        Debug.Log("In shield on disable...");

        GameManager.deregister(transform.name);
        if (!caster.gameObject.GetComponent<MagicAttack>().isLocalPlayer)
            return;

        var magic = GameManager.getObject(caster.id).GetComponent<MagicAttack>(); // Client error on release

        Debug.Log("Calling shield down: " + GetComponent<NetHealth>().getHealth());
        magic.shieldDown(GetComponent<NetHealth>().getHealth());

        // Remove from UI
        ((MagicianUI) caster.GetComponent<Identifier>().UI).onShieldDown(); // Server error on release
    }

    IEnumerator movePosition()
    {
        yield return new WaitForSeconds(0.5f);
        transform.localPosition = new Vector3(1, 5, 0);
    }
}