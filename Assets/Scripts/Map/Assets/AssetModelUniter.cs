using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AssetModelUniter : NetworkBehaviour
{

    [SyncVar] public int layer;
    [SyncVar] public int colID;

    // Use this for initialization
    void Start()
    {
        if (!isServer)//asset uniter is to reunite the transform of the main asset wrapper and its model on the client side
        {
            StartCoroutine(waitNSet());
        }
    }

    public IEnumerator waitNSet()
    {
        yield return new WaitForSecondsRealtime(1.5f);

        setPartent();
    }

    private void setPartent()
    {
        try
        {
            if (MapManager.manager.voxels[layer][colID].mainAsset == null) Debug.LogError("trying to reunite asset on client side - but voxel has no asset");
            transform.parent = MapManager.manager.voxels[layer][colID].mainAsset.transform;
            transform.localPosition = Vector3.zero; 
            MapManager.manager.voxels[layer][colID].mainAsset.united = true;
            //Debug.Log("reunited main asset id: " + layer + " ; " + colID);
            StartCoroutine(MapManager.manager.voxels[layer][colID].mainAsset.waitNSet());
            //MapManager.manager.voxels[layer][colID].mainAsset.setParent();
            
            Vector3 forward = getFoward();
            if (!forward.Equals(Vector3.zero) && !transform.position.Equals(Vector3.zero))
            {
                //rb.MoveRotation(Quaternion.LookRotation(forward, -transform.position.normalized));
                transform.rotation = Quaternion.LookRotation(forward, -transform.position.normalized);
            }
        }
        catch {
            Debug.LogError("Asset model belonging to dud voxel");
            Destroy(gameObject);
        }
    }
    
    public Vector3 getFoward()
    {
        var up = -transform.position.normalized;
        var foward = Vector3.Cross(up, transform.right);

        if (Vector3.Dot(foward, transform.forward) < 0)
        {
            foward *= -1;
        }

        return foward;
    }
}
