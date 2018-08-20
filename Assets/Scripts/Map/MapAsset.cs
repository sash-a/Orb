using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//Used to orientate a gameObject with a rigidBody correctly in the circle

public class MapAsset : NetworkBehaviour
{

    static float heightVariation = 0.5f;
    static float widthVariation = 0.3f;
    static float rotateVariation = 5f;

    Rigidbody rb;
    GameObject asset;
    public Voxel voxel;
    [SyncVar] int layer;
    [SyncVar] int colID;


    bool falling = false;
    bool ready = false;

    public GameObject mainAsset;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Vector3 forward = getFoward();
        rb.MoveRotation(Quaternion.LookRotation(forward, -transform.position.normalized));//stand up straight
        rb.isKinematic = true;
        //Debug.Log("starting map asset at: " + transform.position);
        gameObject.tag = "MapAsset";
        StartCoroutine(waitNSet());
         
    }

    public static MapAsset createAsset(Voxel vox)
    {
        //Debug.Log("creating map asset at: " + vox.worldCentreOfObject);

        GameObject ass = null;
       
            //ass = Instantiate(Resources.Load<GameObject>("Prefabs/Map/MapAssets/Palm_Tree"), vox.worldCentreOfObject, Quaternion.identity);
        ass = spawnMainAsset(vox);

        ass.GetComponent<MapAsset>().voxel = vox;
        ass.GetComponent<MapAsset>().colID = vox.columnID;
        ass.GetComponent<MapAsset>().layer = vox.layer;
        
        NetworkServer.Spawn(ass);
        return ass.GetComponent<MapAsset>();
    }

    private static GameObject spawnMainAsset(Voxel vox)
    {
        string folder = "";
        if (vox.layer == 0)
        {
            folder = "MainSurfaceAssets";
        }
        else {
            folder = "MainCaveFloorAssets";
            Debug.Log("placing cave floor asset");
        }

        GameObject ass = (GameObject)Instantiate(Resources.Load<UnityEngine.Object>("Prefabs/Map/MapAssets/MainAsset"), vox.worldCentreOfObject, Quaternion.identity);
        UnityEngine.Object[] assets = Resources.LoadAll<GameObject>("Prefabs/Map/MapAssets/"+ folder);
        //GameObject model = Instantiate(Resources.Load<GameObject>("Prefabs/Map/MapAssets/MainAssets/Palm_Tree_1"), vox.worldCentreOfObject, Quaternion.identity);
        int idx = UnityEngine.Random.Range(0, assets.Length);
        //Debug.Log("loading asset idx " + idx+ "from asset list of length: " + assets.Length);
        GameObject model = (GameObject)Instantiate(assets[idx], vox.worldCentreOfObject, Quaternion.identity);

        model.transform.parent = ass.transform;
        NetworkServer.Spawn(model);

        return ass;
    }

    public IEnumerator waitNSet()
    {
        yield return new WaitForSecondsRealtime(3f);
        setParent();

    }

    public void setParent()
    {

        MapManager man = MapManager.manager;
        GameObject map = man.Map;
        //transform.parent = map.transform.GetChild(2);
        if (voxel == null)
        {
            voxel = man.voxels[layer][colID];
        }

        setTransform();

        if (voxel.asset != null && voxel.asset != this)
        {
            Debug.Log("removing duplicate tree");
            NetworkServer.Destroy(gameObject);
            return;
        }
        voxel.asset = this;
        //changeParent(voxel.gameObject.transform);
        changeParent(MapManager.manager.Map.transform.GetChild(3));

        if (voxel.Equals(MapManager.DeletedVoxel))
        {
            Debug.Log("removing straggler tree");
            NetworkServer.Destroy(gameObject);
        }

        ready = true;
    }

    bool transformed = false;
    private void setTransform()
    {
        if (transformed) Debug.Log("resetting map transform again");
        Vector3 forward = getFoward();
        Vector3 right = Vector3.Cross(forward, -transform.position).normalized;
        System.Random rand = new System.Random(voxel.layer * voxel.columnID + voxel.columnID);
        //transform.position += forward * (float)(rand.NextDouble() - 0.5f) + right * (float)(rand.NextDouble() - 0.5f);
        float size = (float)(rand.NextDouble() * 0.6f + 0.4);//supposed to be height*wdth

        float width = 2f * size + (float)(rand.NextDouble() * widthVariation + widthVariation * 0.5f);
        float height = size + (float)(rand.NextDouble() * heightVariation + heightVariation * 0.5f) + 0.2f;
        transform.localScale = new Vector3(
            transform.localScale.x * width,
            transform.localScale.y * height,
            transform.localScale.z * width);
        transform.Rotate(new Vector3((float)(rand.NextDouble() * rotateVariation + rotateVariation * 0.5f), (float)(rand.NextDouble() * rotateVariation + rotateVariation * 0.5f), (float)(rand.NextDouble() * rotateVariation + rotateVariation * 0.5f)));
        transformed = true;
    }

    public void changeParent(Transform tran)
    {
        Vector3 absPos = transform.position;
        transform.parent = tran;
        transform.position = absPos;
    }

    public Vector3 getFoward()
    {
        var up = -transform.position.normalized;
        var foward = Vector3.Cross(up, transform.right);

        if (Vector3.Dot(foward, transform.forward) < 0)
        {
            foward *= -1;
        }

        return foward.normalized;
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        if (!falling && ready)
        {
            //Debug.Log("map asset collided with: " + collision.gameObject + " tag: " + collision.gameObject.tag);
            if (collision.gameObject.tag.Equals("TriVoxel"))
            {
                voxel = collision.gameObject.GetComponent<Voxel>();
                voxel.asset = this;
                //Debug.Log("reassigning  map asset to new voxel: " + voxel + " ");
                rb.isKinematic = true;
                //transform.parent = collision.gameObject.transform;
            }
        }
    }


    private void Update()
    {
        if (voxel == null && !falling && ready) {
            if (rb.isKinematic)
            {
                rb.isKinematic = false;
                //Debug.Log("assets voxel has been deleted");
            }
            else {
                CmdAddGravity();
                rb.AddForce(transform.forward,ForceMode.Acceleration);
            }
        }
    }

    [Command]
    public void CmdAddGravity()
    {
        RpcAddGravity();
        gameObject.AddComponent<Gravity>();
        gameObject.GetComponent<NetworkTransform>().enabled = true;
        rb.AddForce(transform.forward, ForceMode.Acceleration);
        falling = true;
    }

    [ClientRpc]
    private void RpcAddGravity()
    {
        if (gameObject.GetComponent<Gravity>() == null) {
            gameObject.AddComponent<Gravity>();
            gameObject.GetComponent<NetworkTransform>().enabled = true;
            falling = true;
            //rb.AddForce(transform.forward, ForceMode.Acceleration);
        }

    }

    internal void CmdMoveTo(Vector3 pos)
    {
        RpcMoveTo(pos);
    }

    private void RpcMoveTo(Vector3 pos)
    {
        transform.position = pos;
    }
}