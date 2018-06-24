using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;
using UnityEditor;

public class MapManager : NetworkBehaviour
{
    public static bool useSmoothing = false;
    public static bool use1factorSmoothing = true;
    public static bool use2factorSmoothing = true;
    public static bool use3factorSmoothing = true;

    public static int shatters = 2;//make zero to turn off shattering

    public static int mapLayers = 15;
    public static int mapSize = 200;
    public static int splits = 0;



    public Dictionary<int, Dictionary<int, Voxel>> voxels; // indexed like voxels[layer][column]
    public Dictionary<int, Vector3> voxelPositions; // the object centers of the layer=0 voxels

    public Dictionary<int, HashSet<int>> neighboursMap; //maps column id's onto a set of all column ids that that column is adjacent to

    public static Voxel DeletedVoxel;

    //public Dictionary<int,Column> columns;

    public GameObject Map;
    public static MapManager manager;

    //public static Digger digger;
    public HashSet<Portal> portals;

    public override void OnStartClient()
    {
        if (!isServer)
        {
            start();
        }
    }
    // Use this for initialization
    public void start()
    {

        manager = this;
        Map = GameObject.Find("Map");
        Debug.Log("starting map manager");
        //RegisterPrefabs.registerVoxelPrefabs(MapManager.splits);


        DeletedVoxel = new Voxel();
        voxels = new Dictionary<int, Dictionary<int, Voxel>>();
        voxelPositions = new Dictionary<int, Vector3>();
        neighboursMap = new Dictionary<int, HashSet<int>>();
        portals = new HashSet<Portal>();
        for (int i = 0; i < mapLayers; i++)
        {
            voxels[i] = new Dictionary<int, Voxel>();
        }

        // Shane what does this do?
        Map.transform.localScale *= mapSize;
        //deleteIDs = new ArrayList();
    }

    internal void replaceSubVoxel(Voxel spawnedVox)
    {
        Voxel v = voxels[spawnedVox.layer][spawnedVox.columnID];//v should be a voxel container for this to be a valid call to destroy subvoxel
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

    public void voxelsLoaded()
    {
        //Debug.Log("voxels loaded called");
        Debug.Log("voxels loaded - found " + voxels[0].Count + " initialised  voxels");
        foreach (Voxel vox in voxels[0].Values)
        {
            neighboursMap[vox.columnID] = new HashSet<int>();
            vox.gatherAdjacentNeighbours();
        }

        foreach (Voxel vox in voxels[0].Values)
        {
            vox.gameObject.transform.localScale *= MapManager.mapSize;
            vox.checkNeighbourCount();
            voxelPositions.Add(vox.columnID, vox.centreOfObject);
            vox.gameObject.name = "TriVoxel";
        }
        //CaveManager.digCaves();
    }

    internal bool doesVoxelExist(int layer, int columnID)
    {
        if (layer >= 0 && layer < mapLayers //within correct range
                       && voxels[layer].ContainsKey(columnID) //has a record in the voxels list
                       && voxels[layer][columnID] != null //should never be null
                       && !voxels[layer][columnID].Equals(MapManager.DeletedVoxel)) //has not been deleted
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    internal void informDeleted(int layer, int columnID) //block at layer columnID
    {

        Vector3 centre = voxels[layer][columnID].centreOfObject;
        voxels[layer][columnID] = MapManager.DeletedVoxel;

        foreach (int n in neighboursMap[columnID])
        {
            if (doesVoxelExist(layer, n))
            {
                voxels[layer][n].smoothBlockInPlace();
            }
        }

        if (doesVoxelExist(layer + 1, columnID))
        {
            voxels[layer + 1][columnID].smoothBlockInPlace();

        }

        if (doesVoxelExist(layer - 1, columnID))
        {
            voxels[layer - 1][columnID].smoothBlockInPlace();
        }
        //manager.StartCoroutine(informNeighbours(layer, columnID));
    }


    IEnumerator informNeighbours(int layer, int columnID)
    {
        yield return new WaitForSeconds(0.15f);
        Vector3 centre = voxels[layer][columnID].centreOfObject;
        voxels[layer][columnID] = MapManager.DeletedVoxel;

        foreach (int n in neighboursMap[columnID])
        {
            if (doesVoxelExist(layer, n))
            {
                voxels[layer][n].smoothBlockInPlace();
            }
        }

        if (doesVoxelExist(layer + 1, columnID))
        {
            voxels[layer + 1][columnID].smoothBlockInPlace();

        }

        if (doesVoxelExist(layer - 1, columnID))
        {
            voxels[layer - 1][columnID].smoothBlockInPlace();
        }
    }


    [ClientRpc]
    public void RpcDestroyNextSubvoxel(int layer, int columnID, String subID)
    {
        //Debug.Log("client rpc destroy next subvoxel");
        FinalDestroyNextSubVoxel(layer, columnID, subID);
    }

    int destroyID = 0;//local
    void FinalDestroyNextSubVoxel(int layer, int columnID, String subID)
    {

        //Debug.Log("finally destroying sub: '" + subID + "' determines shatter level: " + (subID.Split(',').Length - 1));

        Voxel v = getSubVoxelAt(layer, columnID, subID);

        if (v.shatterLevel < MapManager.shatters)
        {//can shatter more
            //Debug.Log("destroyed subvoxel being made a new container");
            v.gameObject.AddComponent<VoxelContainer>();
            VoxelContainer vc = v.gameObject.GetComponent<VoxelContainer>();
            vc.start(v);
            StartCoroutine(v.Melt());
        }
        else
        {
            Destroy(v.gameObject);
        }
    }

    public Voxel getSubVoxelAt(int layer, int columnID, String subID) {
        Voxel v = voxels[layer][columnID];//v should be a voxel container for this to be a valid call to destroy subvoxel
                                          //Debug.Log("found top level container: " + v + " of type: " + v.GetType());
        int shatterLevel = subID.Split(',').Length - 1;

        for (int i = 1; i <= shatterLevel; i++)
        {
            VoxelContainer vc = v.gameObject.GetComponent<VoxelContainer>();
            //Debug.Log("opening container " + vc + " - " + vc.subVoxelID);
            v = (Voxel)vc.subVoxels[int.Parse(subID.Split(',')[i])];
            //Debug.Log("opening contained subVoxel " + v + " - " + v.subVoxelID);
        }

        return v;
    }



    // returns true only if the voxel has been created and destroyed|| or the voxel is in layer -1
    internal bool isDeleted(int l, int columnID)
    {
        if (l <= -1) return true;

        if (voxels.ContainsKey(l))
        {
            if (voxels[l].ContainsKey(columnID))
            {
                return voxels[l][columnID].Equals(DeletedVoxel);
            }
            else
            {
                return false; //hasnt even been created yet
            }
        }
        else
        {
            //layer doesnt exist
            return true;
        }
    }

    //returns the position of a voxel whether it exists or not
    public Vector3 getPositionOf(int layer, int columnID)
    {
        float scale = (float)Math.Pow(Voxel.scaleRatio, layer);
        //Debug.Log("rec cen: " + voxelPositions[columnID] + " act cen " + voxels[0][columnID].centreOfObject);
        return voxelPositions[columnID] * scale * mapSize;
    }
}