using UnityEngine;


public class Overlord : MonoBehaviour
{
    public MapManager mapManager;
    public NetworkMapGen networkMapGen;

    void Start()
    {
        Debug.Log("Starting overlord");
        mapManager.start();
        networkMapGen.start();
//        NetworkServer.Spawn(NetworkManager.singleton.playerPrefab);
    }
}