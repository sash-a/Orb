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
        }
        catch {
            Debug.LogError("Asset model belonging to dud voxel");
            Destroy(gameObject);
        }
    }
}
