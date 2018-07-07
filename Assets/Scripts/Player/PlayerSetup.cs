using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Identifier))]
public class PlayerSetup : NetworkBehaviour
{
    public Behaviour[] componentsToDisable;

    private Camera mainCam;

    [SerializeField] private const string REMOTE_LAYER_NAME = "RemotePlayer";
    [SerializeField] private const string LOCAL_LAYER_NAME = "LocalPlayer";

    [SerializeField] private GameObject playerUIPrefab;
    private GameObject playerUIInstance;

    // Use this for initialization
    void Start()
    {
        if (!isLocalPlayer)
        {
            foreach (Behaviour comp in componentsToDisable)
                comp.enabled = false;
            
            assignRemotePlayer();
        }
        else
        {
            // TODO move somewhere else
            MapManager.localPlayer = gameObject;
            mainCam = Camera.main;
            if (mainCam != null)
                mainCam.gameObject.SetActive(false);

            // Create player UI
            playerUIInstance = Instantiate(playerUIPrefab);
            playerUIInstance.name = playerUIPrefab.name;
            // Might need to check if null
            playerUIInstance.GetComponent<PlayerUI>().setPlayer(gameObject);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        GameManager.register(GetComponent<NetworkIdentity>().netId.ToString(), GetComponent<Identifier>());
    }

    void assignRemotePlayer()
    {
        gameObject.layer = LayerMask.NameToLayer(REMOTE_LAYER_NAME);
    }

    private void OnDisable()
    {
        Destroy(playerUIInstance);
        
        if (mainCam != null && isLocalPlayer)
            mainCam.gameObject.SetActive(true);

        GameManager.deregister(transform.name);
    }
}