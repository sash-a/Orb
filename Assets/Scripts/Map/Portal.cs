using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Portal : MapAsset
{
    System.Random rand;

    Vector3 portalDims;

    GameObject supplyRoom;
    public GameObject player;

    [SerializeField] private const int supplyTime = 10;

    [SyncVar] int columnID;
    [SyncVar] int layer;

    bool created = false;

    void Start()
    {
        supplyRoom = GameObject.Find("SupplyRoom");
        transform.parent = MapManager.manager.Map.transform.GetChild(2);
        //orient();
    }



    public virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Contains("unner"))
        {
            // Moved suply room to be found on start
            player = collision.gameObject;
            player.transform.position = supplyRoom.transform.position;
            player.GetComponent<Gravity>().inSphere = false;
            //StartCoroutine(ReturnPlayer());
            RespawnPlayer spawn = new RespawnPlayer(TeamManager.localPlayer, GameEventManager.clockTime + supplyTime);
            GameEventManager.singleton.addEvent(spawn);
        }
    }

}
