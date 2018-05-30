using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Identifier))]
public class ShieldSetUp : NetworkBehaviour
{
    [SerializeField] private const string REMOTE_LAYER_NAME = "RemotePlayer";

    [SerializeField] private const string LOCAL_LAYER_NAME = "LocalPlayer";

    void Start()
    {
        GameManager.register(GetComponent<NetworkIdentity>().netId.ToString(), GetComponent<Identifier>());

        if (!isLocalPlayer) assignRemoteLayer();
    }

    void assignRemoteLayer()
    {
        gameObject.layer = LayerMask.NameToLayer(REMOTE_LAYER_NAME);
    }

    // TODO set shieldUp = false
    private void OnDisable()
    {
        GameManager.deregister(transform.name);
    }
}