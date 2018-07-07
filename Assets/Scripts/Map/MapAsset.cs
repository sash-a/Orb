using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//Used to orientate a gameObject with a rigidBody correctly in the circle

public class MapAsset : NetworkBehaviour
{

    static float heightVariation = 0.7f;
    static float widthVariation = 1f;
    static float rotateVariation = 5f;

    Rigidbody rb;
    GameObject asset;
    public Voxel voxel;
    [SyncVar] int layer;
    [SyncVar] int colID;


    bool falling = false;



    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Vector3 forward = getFoward();
        rb.MoveRotation(Quaternion.LookRotation(forward, -transform.position.normalized));
        //Debug.Log("starting map asset at: " + transform.position);
        gameObject.tag = "MapAsset";
        StartCoroutine(waitNSet());
    }

    public static MapAsset createAsset(Voxel vox)
    {
        //Debug.Log("creating map asset at: " + vox.worldCentreOfObject);

        GameObject ass = null;
        if (vox.layer == 0)
        {
            ass = Instantiate(Resources.Load<GameObject>("Prefabs/Map/MapAssets/Palm_Tree"), vox.worldCentreOfObject, Quaternion.identity);
        }
        else
        {
            return null;
        }
        ass.GetComponent<MapAsset>().voxel = vox;
        ass.GetComponent<MapAsset>().colID = vox.columnID;
        ass.GetComponent<MapAsset>().layer = vox.layer;
        NetworkServer.Spawn(ass);
        return ass.GetComponent<MapAsset>();
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

        Vector3 forward = getFoward();
        Vector3 right = Vector3.Cross(forward, -transform.position).normalized;

        if (voxel == null)
        {
            voxel = man.voxels[layer][colID];
        }

        if (voxel.rand == null)
        {
            voxel.rand = new System.Random(layer * voxel.columnID + voxel.columnID);
        }
        transform.position += forward * (float)(voxel.rand.NextDouble() - 0.5f) + right * (float)(voxel.rand.NextDouble() - 0.5f);
        float width = (float)(voxel.rand.NextDouble() * widthVariation + widthVariation * 0.5f);
        transform.localScale = new Vector3(transform.localScale.x * width, transform.localScale.y * (float)(voxel.rand.NextDouble() * heightVariation + widthVariation * 0.5f), transform.localScale.z * width);
        transform.Rotate(new Vector3((float)(voxel.rand.NextDouble() * rotateVariation + rotateVariation * 0.5f), (float)(voxel.rand.NextDouble() * rotateVariation + rotateVariation * 0.5f), (float)(voxel.rand.NextDouble() * rotateVariation + rotateVariation * 0.5f)));

        if (voxel.asset != null && voxel.asset != this)
        {
            Debug.Log("removing duplicate tree");
            NetworkServer.Destroy(gameObject);
            return;
        }
        voxel.asset = this;
        changeParent(voxel.gameObject.transform);


        if (voxel.Equals(MapManager.DeletedVoxel))
        {
            Debug.Log("removing straggler tree");
            NetworkServer.Destroy(gameObject);
        }
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
        if (!falling)
        {
            //Debug.Log("map asset collided with: " + collision.gameObject + " tag: " + collision.gameObject.tag);
            if (collision.gameObject.tag.Equals("TriVoxel"))
            {
                voxel = collision.gameObject.GetComponent<Voxel>();
                voxel.asset = this;
                //Debug.Log("reassigning  map asset to new voxel: " + voxel + " ");
                rb.isKinematic = true;
                transform.parent = collision.gameObject.transform;
            }
        }
    }


    private void Update()
    {
        if (voxel == null && !falling) {
            if (rb.isKinematic)
            {
                rb.isKinematic = false;
                //Debug.Log("assets voxel has been deleted");
            }
            else {
                //Debug.Log("assets has no voxel");
                changeParent(MapManager.manager.Map.transform);
                falling = true;
                //rb.isKinematic = true;
                gameObject.AddComponent<Gravity>();
                rb.AddForce(transform.forward,ForceMode.Acceleration);
            }
        }

        
    }

}