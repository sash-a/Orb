using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Identifier))]
public class Shield : NetworkBehaviour
{
    [SerializeField] private const string REMOTE_LAYER_NAME = "RemotePlayer";
    [SerializeField] private const string LOCAL_LAYER_NAME = "LocalPlayer";

    [SerializeField] private Identifier caster;

    // Stats
    [SerializeField] public int shieldHealth;

    // How will this be done for artifacts? Needs to be static as needs to be checked that has enough magic to use
    [SerializeField] public static int initialEnergyUsage = 20;
    [SerializeField] public int energyDrainRate;

    void Start()
    {
        // TODO is this ever called doesn't seem to be registering
        GameManager.register(GetComponent<NetworkIdentity>().netId.ToString(), GetComponent<Identifier>());

        shieldHealth = 100;
        energyDrainRate = 2;

        if (!isLocalPlayer) assignRemoteLayer();
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
    }


    private void OnDisable()
    {
        Debug.Log("Disabling");
        var magic = GameManager.getObject(caster.id).GetComponent<MagicAttack>();

        magic.shieldDown();

        if (magic.getShieldEnergyDrain() != null)
            magic.getResourceManager().endEnergyDrain(magic.getShieldEnergyDrain());

        GameManager.deregister(transform.name);
    }
}