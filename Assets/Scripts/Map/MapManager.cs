using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class MapManager : MonoBehaviour
{
    public static int mapLayers = 10;
    public static int mapSize = 200;
    public static int splits = 0;

    public static Dictionary<int, Dictionary<int, Voxel>> voxels; // indexed like voxels[layer][column]
    public static Dictionary<int, Dictionary<int, Vector3>> voxelPositions; // the world coords of each vox center

    public static Dictionary<int, HashSet<int>>
        neighboursMap; //maps column id's onto a set of all column ids that that column is adjacent to

    public static Voxel DeletedVoxel;

    //public Dictionary<int,Column> columns;

    public static GameObject Map;
    //public static MapManager manager;

    // Use this for initialization
    void Start()
    {
        //manager = this;
        Map = GameObject.Find("Map");
        Debug.Log("starting map manager");

        DeletedVoxel = new Voxel();
        voxels = new Dictionary<int, Dictionary<int, Voxel>>();
        voxelPositions = new Dictionary<int, Dictionary<int, Vector3>>();
        neighboursMap = new Dictionary<int, HashSet<int>>();
        for (int i = 0; i < mapLayers; i++)
        {
            voxels[i] = new Dictionary<int, Voxel>();
            voxelPositions[i] = new Dictionary<int, Vector3>();
        }

        // Shane what does this do?
        Map.transform.localScale *= mapSize;
        
    }


    public static void voxelsLoaded()
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
        }
    }

    internal static bool doesVoxelExist(int layer, int columnID)
    {
        if (layer >= 0 && layer < mapLayers //within correct range
                       && voxels[layer].ContainsKey(columnID) //has a record in the voxels list
                       && MapManager.voxels[layer][columnID] != null //should never be null
                       && !MapManager.voxels[layer][columnID].Equals(MapManager.DeletedVoxel)) //has not been deleted
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    internal static void informDeleted(int layer, int columnID) //block at layer columnID
    {
        Vector3 centre = voxels[layer][columnID].centreOfObject;
        voxels[layer][columnID] = MapManager.DeletedVoxel;

        foreach (int n in neighboursMap[columnID])
        {
            if (doesVoxelExist(layer, n))
            {
                voxels[layer][n].smoothBlock(true, centre);
            }
        }

        if (doesVoxelExist(layer + 1, columnID))
        {
            voxels[layer + 1][columnID].smoothBlock(false, centre);
        }

        if (doesVoxelExist(layer - 1, columnID))
        {
            voxels[layer - 1][columnID].smoothBlock(false, centre);
        }
    }

    // returns true only if the voxel has been created and destroyed|| or the voxel is in layer -1
    internal static bool isDeleted(int l, int columnID)
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
}