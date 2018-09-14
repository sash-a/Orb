using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Identifier))]
public class Shield : NetworkBehaviour
{
    [SerializeField] private Identifier caster;

    void Start()
    {
        transform.localPosition += new Vector3(0, 4, 0);

        if (!caster.gameObject.GetComponent<MagicAttack>().isLocalPlayer) assignRemoteLayer();
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

    public void setCaster(Identifier id, float maxHealth)
    {
        caster = id;

        var player = GameManager.getObject(caster.id);
        //Debug.Log(player.GetComponent<MagicAttack>().getAttackStats().artifactType);
        // if owns artifact
        if (player.GetComponent<MagicAttack>().getAttackStats().artifactType == PickUpItem.ItemType.HEALER_ARTIFACT)
        {
            Debug.Log("Scaling");
            transform.localScale = new Vector3(15f, 15, 15f);
        }

        // UI
        if (isServer) return;

        var netHealth = GetComponent<NetHealth>();
        netHealth.setInitialHealth(maxHealth);
        if (caster.UI != null)
        {
            ((MagicianUI) caster.UI).onShieldUp(netHealth); // server error on cast
            Debug.Log(GetComponent<NetHealth>().getHealth() + "/" + GetComponent<NetHealth>().maxHealth);
        }
        else
        {
            Debug.LogError("null UI component");
        }

        // Move camera
//        var camPivot = GameManager.getObject(caster.id).GetComponentInChildren<Camera>().transform.parent.transform;
//        Debug.Log("moving cam " + camPivot.position);
//        camPivot.position += camPivot.up * 2;
//        Debug.Log("moved cam " + camPivot.position);
    }


    private void OnDisable()
    {
        if (!isLocalPlayer)
        {
            GameManager.deregister(transform.name);
            return;
        }

        Debug.Log("Looking for magician: " + caster.id);
        var magic = GameManager.getObject(caster.id).GetComponent<MagicAttack>(); // Client error on release

        magic.shieldDown();

        // Remove from UI
        ((MagicianUI) caster.GetComponent<Identifier>().UI).onShieldDown(); // Server error on release
    }
}