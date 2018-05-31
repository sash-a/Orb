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

    public GameObject treePrefab;

    //higher density = less trees (yes i know its weird)
    public int density;

    void Start()
    {
    }

    private void spawnVoxelsOnServer(int splits)
    {
        Object[] voxels = Resources.LoadAll("Voxels/Prefabs/Split" + splits, typeof(GameObject));

        if (voxels.Length == 0)
        {
            Debug.LogWarning("failed to load voxels");
        }

        foreach (var voxel in voxels)
        {
            int colID = Int32.Parse(voxel.name.Substring(5));
            var voxelGameObj = (GameObject) voxel;
            voxelGameObj.GetComponent<Voxel>().columnID = colID;

            voxelDict.Add(colID, voxelGameObj.GetComponent<Voxel>());
        }

        foreach (var colID in voxelDict.Keys)
        {
            GameObject inst = Instantiate(voxelDict[colID].gameObject);
            inst.GetComponent<Voxel>().setColumnID(colID);
//            Debug.Log("World center according to server: " + inst.GetComponent<Voxel>().worldCentreOfObject);
            NetworkServer.Spawn(inst);
        }

//        MapManager.voxelsLoaded();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("Spawning");
        spawnVoxelsOnServer(MapManager.splits);
        Debug.Log("Done Spawning");
        StartCoroutine(InitTrees());
    }


    public override void OnStartClient()
    {
        base.OnStartClient();
        StartCoroutine(InitVoxels());
    }

    IEnumerator InitTrees()
    {
        yield return new WaitForSeconds(1);
        GenerateTrees(density);
        //Debug.Log("Generated trees");
    }

    IEnumerator InitVoxels()
    {
        yield return new WaitForSeconds(0.5f);


        GameObject[] objs = FindObjectsOfType<GameObject>();
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i].name.Contains("voxel") || objs[i].name.Equals("TriVoxel"))
            {
                objs[i].transform.parent = parent.transform.GetChild(1);
            }
        }

        //Debug.Log("on start client called childcount = " + parent.transform.GetChild(1).childCount);
        for (int i = 0; i < parent.transform.GetChild(1).childCount; i++)
        {
            GameObject voxObj = parent.transform.GetChild(1).GetChild(i).gameObject;
            if (!(voxObj.name.Contains("voxel") || voxObj.name.Equals("TriVoxel")))
            {
                Debug.LogError("incorect child load - loaded" + voxObj.name);
            }
            else
            {
                Voxel v = voxObj.GetComponent<Voxel>();
                v.setColumnID(i);
            }
        }

        MapManager.voxelsLoaded();
    }

    public void GenerateTrees(int density)
    {
        //random group of trees between 10 and 20
        int numTrees = UnityEngine.Random.Range(10, 20);
        // d is related to density
        int d = density;
        //loop through every surface voxel
        //Debug.LogWarning(MapManager.voxels[0].Count);
        foreach (Voxel vox in MapManager.voxels[0].Values)
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
            if (numTrees > 0 && density > 0)
            {
                if (isServer)
                {
                    //Debug.Log("Center: " + vox.worldCentreOfObject);
                    GameObject tree = Instantiate(treePrefab, vox.worldCentreOfObject, Quaternion.identity);
                    NetworkServer.Spawn(tree);
                }
            }

            //if trees are still spawning, decrement numTree counter
            if (numTrees > 0)
            {
                numTrees--;
            }

            //once a group of trees has been instantiated, skip a number of voxels related to density
            if (numTrees == 0)
            {
                d--;
                continue;
            }
        }
    }
}