﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class MapAssetManager : NetworkBehaviour
{

    public static int mainAssetSparsity = 40;
    public static int grassDensity = 3;//how many blades of grass per voxel
    //colab is a bitch


    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void genAssets()
    {
        if (!MapManager.useMapAssets) return;
        genGrass();
        if (!isServer) return;
        System.Random rand = new System.Random(0);
        genArtifactAltars();
        genMainAssets(rand);



    }


    private void genArtifactAltars()
    {
        int count = 0;
        for (int i = MapManager.mapLayers-1; i >=0; i--)
        {
            if (MapManager.manager.altars.Count >= CaveManager.numAltars)
            {
                break;
            }
            foreach (Voxel vox in MapManager.manager.voxels[i].Values)
            {
                if (MapManager.manager.altars.Count >= CaveManager.numAltars) {
                    break;
                }
                if (vox.mainAsset != null) {
                    continue;
                }

                if (!MapManager.manager.isDeleted(vox.layer, vox.columnID) && MapManager.manager.doesVoxelExist(vox.layer, vox.columnID))
                {
                    if (CaveManager.manager.caveFloors.Contains(vox))//||layer==0
                    {
                        if (vox.layer > 7)
                        {
                            if (newAltarFarEnough(vox)) {
                                vox.addMainAsset(-1, MapAsset.Type.ALTAR);
                            }
                            //Debug.Log("generating altar");

                        }
                    }
                    //if (CaveManager.manager.caveCeilings.Contains(vox))//||layer==0
                    //{
                    //    count++;
                    //    if (UnityEngine.Random.Range(0f, 1f) < 0.6f)
                    //    {
                    //        //Debug.Log("generating critter spawner");
                    //        vox.addMainAsset(1, MapAsset.Type.CRITTERSPANWER);
                    //    }
                    //}
                }

            }
        }
        //Debug.Log("placed " + MapManager.manager.altars.Count + " altars ");
        //Debug.Log("tried to place critter spawners in " + count + " places");
    }

    private bool newAltarFarEnough(Voxel vox)
    {
        int minAllowedDistance = 60;// the closest two altars can be from each other

        foreach (Altar a in MapManager.manager.altars) {
            double d = Vector3.Distance(a.transform.position, vox.worldCentreOfObject);
            if (d < minAllowedDistance) {
                return false;
            }
        }
        return true;
    }

    private void genMainAssets(System.Random rand)
    {
        //random group of trees between 10 and 20
        int numTrees = rand.Next(10, 20);
        // d is related to density
        int d = mainAssetSparsity;
        //loop through every  voxel
        for (int i = 0; i < MapManager.mapLayers; i++)
        {
            foreach (Voxel vox in MapManager.manager.voxels[i].Values)
            {
                if (MapManager.manager.isDeleted(vox.layer, vox.columnID))
                {
                    continue;
                }
                //when the appropriate number of voxels have been skipped
                if (d == 0)
                {
                    //a new tree group size is selected
                    rand = new System.Random((i + 1) * vox.columnID + vox.columnID);
                    numTrees = rand.Next(10, 20);
                    //and d is reset to chosen density value
                    d = mainAssetSparsity;
                    //and the process then repeats itself
                }

                //spawn trees on voxels when conditions met
                if (numTrees > 0 && mainAssetSparsity > 0 && isServer)
                {
                    if (vox.layer == 0)
                    {
                        //Debug.Log("assigning main asset to underground vox");
                        vox.addMainAsset(-1, MapAsset.Type.MAIN);
                    }
                    else
                    {
                        if (CaveManager.manager.caveFloors.Contains(vox))
                        {
                            vox.addMainAsset(-1, MapAsset.Type.MAIN);
                        }
                    }
                }

                //if trees are still spawning, decrement numTree counter
                if (numTrees > 0) numTrees--;

                //once a group of trees has been instantiated, skip a number of voxels related to density
                if (numTrees <= 0) d--;
            }
        }
    }

    private void genGrass()
    {
        for (int i = 0; i < MapManager.mapLayers; i++)
        {
            foreach (Voxel vox in MapManager.manager.voxels[i].Values)
            {
                if (!MapManager.manager.isDeleted(vox.layer, vox.columnID) && MapManager.manager.doesVoxelExist(vox.layer, vox.columnID))
                {
                    if (CaveManager.manager.caveFloors.Contains(vox))//||layer==0
                    {

                        try
                        {
                            vox.genGrass(-1);
                        }
                        catch { }
                    }
                }

            }
        }
    }
}
