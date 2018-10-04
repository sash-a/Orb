using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ShatterManager : NetworkBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    [ClientRpc]
    public void RpcDestroyNextSubvoxel(int layer, int columnID, string subID)
    {
        //Debug.Log("client rpc destroy next subvoxel");
        StartCoroutine(FinalDestroyNextSubVoxel(layer, columnID, subID));
    }

    IEnumerator FinalDestroyNextSubVoxel(int layer, int columnID, string subID)
    {
        //Debug.Log("finally destroying sub: '" + subID + "' determines shatter level: " + (subID.Split(',').Length - 1));

        Voxel v = getSubVoxelAt(layer, columnID, subID);
        int maxTries = 5;
        while (maxTries > 0 && v == null) {
            v = getSubVoxelAt(layer, columnID, subID);
            maxTries--;
            yield return new WaitForSecondsRealtime(0.25f);
        }

        if (v != null)
        {

            if (v.isContainer)
            {
                Debug.LogError("tried deleting subvoxel at: " + subID + " found voxel container instead: " + v);
            }
            else
            {

                if (v.shatterLevel < MapManager.manager.shatters)
                {
                    StartCoroutine(v.ConvertToContainer());
                }
                else
                {
                    Destroy(v.gameObject);
                }
            }
        }
        else {
            Debug.LogError("failed to get voxel to destroy it. error in subvoxel storage or subvoxel does not exist; subid=" + subID);
        }
        yield return new WaitForEndOfFrame();
    }

    internal void replaceSubVoxel(Voxel spawnedVox)
    {
        Voxel v = MapManager.manager.voxels[spawnedVox.layer][spawnedVox.columnID]; //v should be a voxel container for this to be a valid call to destroy subvoxel
        //Debug.Log("found top level container: " + v + " of type: " + v.GetType());
        int shatterLevel = spawnedVox.subVoxelID.Split(',').Length - 1;

        VoxelContainer vc = null;
        for (int i = 1; i <= shatterLevel; i++)
        {
            vc = v.gameObject.GetComponent<VoxelContainer>();
            //Debug.Log("opening container " + vc + " - " + vc.subVoxelID);
            v = (Voxel)vc.subVoxels[int.Parse(spawnedVox.subVoxelID.Split(',')[i])];
            //Debug.Log("opening contained subVoxel " + v + " - " + v.subVoxelID);
        }

        vc.subVoxels[int.Parse(spawnedVox.subVoxelID.Split(',')[shatterLevel])] = spawnedVox;
    }


    public Voxel getSubVoxelAt(int layer, int columnID, string subID)
    {
        // v should be a voxel container for this to be a valid call to destroy subvoxel
        Voxel v = MapManager.manager.voxels[layer][columnID];
        if (subID.ToCharArray()[0] == ',') {
            subID = subID.Substring(1);
        }

        int shatterLevel = subID.Split(',').Length - 1;

        for (int i = 0; i <= shatterLevel; i++)
        {
            if (v == null)
            {
                Debug.LogError("found null voxel looking for subvoxel: " + layer + "," + columnID + "," + subID + " ; failed at level: " + i);
                break;
            }
            VoxelContainer vc = v.gameObject.GetComponent<VoxelContainer>();
            if (vc.subVoxels == null) {
                Debug.LogError("vox cont " + vc + " has a null subvoxels array");
                return null;
            }
            int id = -1;
            try
            {
                id = int.Parse(subID.Split(',')[i]);
            }
            catch (Exception e)
            {
                Debug.LogError("trying to parse " + subID + " index " + i + "into an int, which failed\t" + e.Message);
                return null;
            }
            try
            {
                v = (Voxel)vc.subVoxels[id]; // TODO try catch, this isn't working every time
            }
            catch (Exception e)
            {
                Debug.LogError("trying to get subvoxel at " + subID + " from " + vc + " failed at shatterlevel " + i + " id = " + id + " num subvoxels = " + vc.subVoxels.Count + "\t" + e.Message);
                return null;
            }

        }

        return v;
    }


}
