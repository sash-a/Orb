using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

//767 voxs split=0

public class NetworkMapGen : NetworkBehaviour
{
    Dictionary<int, Voxel> voxelDict = new Dictionary<int, Voxel>();

    public GameObject parent;
    public int density;

    public static NetworkMapGen mapGen;

//    void Start()
//    {
//        mapGen = this;
//    }

    public void start()
    {
        BuildLog.flushLogFile();

        mapGen = this;

        spawnVoxelsOnServer(MapManager.splits);
        genTrees(density);
    }


    private void spawnVoxelsOnServer(int splits)
    {
        if (!isServer) return;

        Object[] voxels = Resources.LoadAll("Voxels/Prefabs/Split" + splits, typeof(GameObject));

        //Debug.Log("Spawning voxels on server");
        if (voxels.Length == 0) Debug.LogError("Failed to load voxels");


        int count = 0;
        foreach (var voxel in voxels)
        {
            var voxelGameObj = (GameObject) voxel;
            voxelGameObj.GetComponent<Voxel>().columnID = count;
            voxelDict.Add(count, voxelGameObj.GetComponent<Voxel>());
            count++;
        }

        foreach (var colID in voxelDict.Keys)
        {
            GameObject inst = Instantiate(voxelDict[colID].gameObject);
            inst.GetComponent<Voxel>().setColumnID(colID);
            inst.name = "Voxel" + colID;

            NetworkServer.Spawn(inst);
        }

        // Calling server side only
//        MapManager.manager.voxelsLoaded();
    }

    /// <summary>
    /// Checks if all voxels have spawned client side
    /// </summary>
    public override void OnStartClient()
    {
        base.OnStartClient();
        mapGen = this;

        StartCoroutine(CountSpawnedVoxels());
    }

    /// <summary>
    /// Repeatedly counts the number of voxels currently spawned and once all voxels are spawned calls voxelsLoaded
    /// </summary>
    /// <returns></returns>
    IEnumerator CountSpawnedVoxels()
    {
        bool loaded = false;
        int maxTries = 10;

        while (!loaded && maxTries > 0)
        {
            yield return new WaitForSeconds(1);
            loaded = MapManager.manager.spawnedVoxels.Count == 768 * Math.Pow(2, MapManager.splits);
            maxTries--;
        }

        if (loaded)
        {
            //Debug.Log("Client voxels spawned correctly");
            MapManager.manager.voxelsLoaded(); // Calling client side only
            MapManager.SmoothVoxels();
        }
        else
        {
            Debug.LogError("waited 5 seconds and not all voxels have been spawned - only found " +
                           MapManager.manager.spawnedVoxels.Count + " unique column id's; should be: " +
                           768 * Math.Pow(2, MapManager.splits));
        }
    }


    private void genTrees(int density)
    {
        if (!isServer) return;
        //random group of trees between 10 and 20
        int numTrees = UnityEngine.Random.Range(10, 20);
        // d is related to density
        int d = density;
        //loop through every surface voxel
        foreach (Voxel vox in MapManager.manager.voxels[0].Values)
        {
            //when the appropriate number of voxels have been skipped
            if (d == 0)
            {
                //a new tree group size is selected
                numTrees = UnityEngine.Random.Range(10, 20);
                //and d is reset to chosen density value
                d = density;
                //and the process then repeats itself
            }

            //spawn trees on voxels when conditions met
            if (numTrees > 0 && density > 0 && isServer)
            {
                vox.addAsset();
            }

            //if trees are still spawning, decrement numTree counter
            if (numTrees > 0) numTrees--;

            //once a group of trees has been instantiated, skip a number of voxels related to density
            if (numTrees <= 0) d--;
        }
    }
}