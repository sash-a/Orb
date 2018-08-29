using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Identifier))]
public class PlayerSetup : NetworkBehaviour
{
    public Behaviour[] componentsToDisable;
    public string dontDrawLayerName = "DontDraw";
    public GameObject playerGraphics;

    private Camera mainCam;


    
    public const string REMOTE_LAYER_NAME = "RemotePlayer";
    public const string LOCAL_LAYER_NAME = "LocalPlayer";

    [SerializeField] private GameObject playerUIPrefab;
    private GameObject playerUIInstance;

    // Use this for initialization
    void Start()
    {
        if (!isLocalPlayer)
        {
            if (componentsToDisable != null)
            {
                foreach (Behaviour comp in componentsToDisable)
                    comp.enabled = false;
            }

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
            if (playerUIPrefab != null)
            {
                playerUIInstance = Instantiate(playerUIPrefab);
                playerUIInstance.name = playerUIPrefab.name;
            }
            
            // Might need to check if null
            var type = GetComponent<Identifier>();
            // Enabling and setting up the correct UI for the class
            if (type.typePrefix == Identifier.gunnerType)
            {
                var ui = playerUIInstance.GetComponentInChildren<GunnerUI>();
                ui.setUp(gameObject);
                ui.magicianUI.SetActive(false);
                ui.gameObject.SetActive(true);
                type.UI = ui;
            }
            else if (type.typePrefix == Identifier.magicianType)
            {
                var ui = playerUIInstance.GetComponentInChildren<MagicianUI>();
                ui.setUp(gameObject);
                ui.gunnerUI.SetActive(false);
                ui.gameObject.SetActive(true);
                type.UI = ui;
            }
            
            // Disable local player graphics
            setLayer(playerGraphics, LayerMask.NameToLayer(dontDrawLayerName));
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

    void setLayer(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            setLayer(child.gameObject, layer);
        }
    }
    
    private void OnDisable()
    {
        Destroy(playerUIInstance);

        if (mainCam != null && isLocalPlayer)
            mainCam.gameObject.SetActive(true);

        GameManager.deregister(transform.name);
    }
}