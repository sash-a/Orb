using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Identifier))]
public class PlayerSetup : NetworkBehaviour
{
    public Behaviour[] componentsToDisable;
    public string dontDrawLayerName = "DontDraw";
    public string dontChangeLayerName = "EnvColliders";
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

            assignRemotePlayer(dontChangeLayerName);
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
                var weaponWheel = ui.weaponWheel.GetComponent<WeaponWheel>();

                // UI
                ui.setUp(gameObject);
                ui.magicianUI.SetActive(false);
                ui.gameObject.SetActive(true);
                type.UI = ui;

                GetComponent<WeaponAttack>().damageIndicator = ui.damageIndicator;
                //Debug.Log("Set dmg indicator " + (GetComponent<WeaponAttack>().damageIndicator == null));

                // Weapon wheel
                weaponWheel.weapons = GetComponent<WeaponAttack>();
                weaponWheel.playerHealth = GetComponent<NetHealth>();
                weaponWheel.rm = GetComponent<ResourceManager>();
            }
            else if (type.typePrefix == Identifier.magicianType)
            {
                var ui = playerUIInstance.GetComponentInChildren<MagicianUI>();
                ui.setUp(gameObject);
                ui.gunnerUI.SetActive(false);
                ui.gameObject.SetActive(true);
                type.UI = ui;
                
                GetComponent<MagicAttack>().damageIndicator = ui.damageIndicator;

            }
            
            // Disable local player graphics
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        GameManager.register(GetComponent<NetworkIdentity>().netId.ToString(), GetComponent<Identifier>());
    }

    void assignRemotePlayer(string ignorelayer)
    {
        setLayer(gameObject, LayerMask.NameToLayer(REMOTE_LAYER_NAME), LayerMask.NameToLayer(ignorelayer));
    }

    void setLayer(GameObject obj, int layer, int ignoreLayer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            if (child.gameObject.layer != ignoreLayer)
            {
                setLayer(child.gameObject, layer, ignoreLayer);
            }
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