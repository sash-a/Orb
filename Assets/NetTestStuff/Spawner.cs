using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Spawner : NetworkBehaviour
{
    public GameObject go;
    public GameObject voxel;

    private bool once = true;

    private GameObject inst;


    // Use this for initialization

    void Start()
    {
        // Adds this to the list of game objects that the server knows about so that it can spawn it
        ClientScene.RegisterPrefab(go);
        Debug.Log("Instantiating...?");
        Instantiate(voxel, new Vector3(1, 1, 1), Quaternion.identity);
        StartCoroutine(spwn());
    }

    void CmdSpawn()
    {
        if (isServer)
        {
            // Creates go on server
            inst = Instantiate(go, new Vector3(1, 1, 1), Quaternion.identity) as GameObject;

            // Spawns on all clients
            NetworkServer.Spawn(inst);
            Debug.Log("Instantiated");
        }
    }

    IEnumerator spwn()
    {
        yield return new WaitForSeconds(5);
        CmdSpawn();
    }

    // Update is called once per frame
    void Update()
    {
    }
}