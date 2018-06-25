using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Portal : NetworkBehaviour
{

    System.Random rand;

    Vector3 portalDims;

    GameObject supplyRoom;
    GameObject player;

    int supplyTime = 10;

    [SyncVar] int columnID;
    [SyncVar] int layer;

    bool created = false;

    // Use this for initialization
    void Start()
    {
        if (!created)
        {
            createFromVoxel(MapManager.manager.voxels[layer][columnID]);
        }
        transform.parent = MapManager.manager.Map.transform.GetChild(3);
    }


    public void createFromVoxel(Voxel seed)
    {
        created = true;
        portalDims = new Vector3(0.03f, 0.05f, 0.002f);
        columnID = seed.columnID;
        layer = seed.layer;

        rand = new System.Random(seed.columnID);

        int ind = Mathf.RoundToInt((float)(rand.NextDouble() * 2));
        int ind2 = (ind + 1) % 3;

        Vector3[] verts = seed.gameObject.GetComponent<MeshFilter>().mesh.vertices;

        Vector3 top = verts[ind];
        Vector3 top2 = verts[ind2];

        Vector3 mid = (top + top2) / 2.0f;
        Vector3 accross = (top2 - top).normalized * portalDims.x / 2.0f;

        top = mid - accross;
        top2 = mid + accross;

        Vector3 bottom = verts[ind + 3];
        Vector3 bottom2 = verts[ind2 + 3];

        mid = (bottom + bottom2) / 2.0f;

        bottom = mid - accross;
        bottom2 = mid + accross;

        Vector3 cross = Vector3.Cross((top - top2), (top - bottom)).normalized * portalDims.z;

        bottom = top + (bottom - top).normalized * portalDims.y;
        bottom2 = top2 + (bottom2 - top2).normalized * portalDims.y;

        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        filter.mesh.vertices = new Vector3[] { top - cross, top2 - cross , top2 + cross , top + cross ,
             bottom - cross, bottom2 - cross , bottom2 + cross , bottom + cross};//topside verts: 0,1,2,3 bottomside verts: 4,5,6,7 

        filter.mesh.triangles = new int[] { 0, 5, 1, 0, 4, 5, 2, 6, 7, 2, 7, 3 };


        GetComponent<MeshCollider>().sharedMesh = filter.mesh;
        transform.localScale *= (float)(MapManager.mapSize * seed.scale);
        transform.position = new Vector3(0, 0, 0);

        MapManager.manager.portals.Add(this);

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("hit portal");
        supplyRoom = GameObject.Find("SupplyRoom");
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
        }
        catch { }
    }
}
