using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CaveManager : NetworkBehaviour
{
    public static HashSet<Digger> diggers;
    static int caveNo = 1;
    static int shatters;

    public static UnityEngine.Object diggerPrefab;

    static System.Random rand;

    // Use this for initialization
    void Start()
    {
        if (!isServer) { return; }
        rand = new System.Random();
        diggers = new HashSet<Digger>();
        diggerPrefab = Resources.Load("Prefabs/Digger");
        //Debug.Log("found digger : " + diggerPrefab);
    }

    internal static GameObject getNewDigger()
    {
        return (GameObject)Instantiate(diggerPrefab, new Vector3(0, 0, 0), Quaternion.LookRotation(new Vector3(0, 0, 1)));
    }

    public static void digCaves()
    {
        shatters = MapManager.shatters;
        MapManager.shatters = 0;
        for (int i = 0; i < caveNo; i++)
        {
            CaveEntrance entrance = new CaveEntrance();
            entrance.createEntranceAt(rand.Next(0, MapManager.manager.neighboursMap.Count - 1));
        }
    }

    static void SmoothVoxels()
    {
        //Debug.Log("smoothing voxels");
        MapManager.useSmoothing = true;
        for (int i = 0; i < MapManager.mapLayers; i++)
        {
            ArrayList keys = new ArrayList();
            foreach (int n in MapManager.manager.voxels[i].Keys)
            {
                keys.Add(n);
            }
            for (int j = 0; j < keys.Count; j++)
            {
                MapManager.manager.voxels[i][(int)keys[j]].smoothBlockInPlace();
            }

        }

        for (int i = 0; i < MapManager.mapLayers; i++)
        {
            ArrayList keys = new ArrayList();
            foreach (int n in MapManager.manager.voxels[i].Keys)
            {
                keys.Add(n);
            }
            for (int j = 0; j < keys.Count; j++)
            {
                MapManager.manager.voxels[i][(int)keys[j]].smoothBlockInPlace();
            }

        }
    }

    public static void removeDigger(Digger d)
    {
        diggers.Remove(d);
        Destroy(d.gameObject);
        if (diggers.Count <= 0)
        {
            SmoothVoxels();
            MapManager.shatters = shatters;
            if (MapManager.useHills)
            {
                MapManager.manager.deviateHeights();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
