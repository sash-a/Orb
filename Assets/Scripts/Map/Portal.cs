using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Portal : MapAsset
{
    System.Random rand;

    Vector3 portalDims;

    GameObject supplyRoom;
    private GameObject player;

    [SerializeField] private const int supplyTime = 10;

    [SyncVar] int columnID;
    [SyncVar] int layer;

    bool created = false;

    void Start()
    {
        supplyRoom = GameObject.Find("SupplyRoom");
        //orient();
    }



    private void OnCollisionEnter(Collision collision)
    {
        // Moved suply room to be found on start
        player = collision.gameObject;
        player.transform.position = supplyRoom.transform.position;
        StartCoroutine(ReturnPlayer());
    }

    IEnumerator ReturnPlayer()
    {
        yield return new WaitForSeconds(supplyTime);
        try
        {
            player.transform.position = new Vector3(0, 40, 0);
            NetworkServer.Destroy(gameObject);
        }
        catch { /* ignored */ }
    }
}
