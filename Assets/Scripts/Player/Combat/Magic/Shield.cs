using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Identifier))]
public class Shield : NetworkBehaviour
{
    [SerializeField] private const string REMOTE_LAYER_NAME = "RemotePlayer";
    [SerializeField] private const string LOCAL_LAYER_NAME = "LocalPlayer";

    [SerializeField] private Identifier caster;

    // Stats
    public int shieldHealth;

    // How will this be done for artifacts? Needs to be static as needs to be checked that has enough magic to use
    [SerializeField] public static int initialEnergyUsage = 20;
    [SerializeField] public static int energyDrainRate = 2;
    [SerializeField] public static int energyGainRate = 1;

    void Start()
    {
        shieldHealth = 100;

        if (!isLocalPlayer) assignRemoteLayer();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // TODO is this never called doesn't seem to be registering
        GameManager.register(GetComponent<NetworkIdentity>().netId.ToString(), GetComponent<Identifier>());
        //Debug.Log("Registered shield");
    }

    [ClientRpc]
    private void RpcRegister()
    {
    }

    void assignRemoteLayer()
    {
        gameObject.layer = LayerMask.NameToLayer(REMOTE_LAYER_NAME);
    }

    public void setCaster(Identifier id)
    {
        caster = id;
        // UI
        ((MagicianUI) caster.UI).onShieldUp(GetComponent<NetHealth>());
    }


    private void OnDisable()
    {
        //Debug.Log("Disabling: " + caster.id);
        var magic = GameManager.getObject(caster.id).GetComponent<MagicAttack>();

        magic.shieldDown();
        GameManager.deregister(transform.name);

        // Remove from UI
        ((MagicianUI) caster.GetComponent<Identifier>().UI).onShieldDown();
    }
}