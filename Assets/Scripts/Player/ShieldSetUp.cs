using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Identifier))]
public class ShieldSetUp : NetworkBehaviour
{
    [SerializeField] private const string REMOTE_LAYER_NAME = "RemotePlayer";

    [SerializeField] private const string LOCAL_LAYER_NAME = "LocalPlayer";

    [SerializeField] private Identifier caster;

    void Start()
    {
        GameManager.register(GetComponent<NetworkIdentity>().netId.ToString(), GetComponent<Identifier>());

        if (!isLocalPlayer) assignRemoteLayer();
    }

    void assignRemoteLayer()
    {
        gameObject.layer = LayerMask.NameToLayer(REMOTE_LAYER_NAME);
    }

    public void setCaster(Identifier id)
    {
        caster = id;
    }


    private void OnDisable()
    {
        Debug.Log("Disabling");
        var magic = GameManager.getObject(caster.id).GetComponent<MagicAttack>();
        magic.shieldDown();
        magic.getResourceManager().endEnergyDrain(magic.getShieldEnergyDrain());

        GameManager.deregister(transform.name);
    }
}