using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CaveManager : NetworkBehaviour
{
    public static HashSet<Digger> diggers;
    static int caveNo = 4;
    static int shatters;

    public static UnityEngine.Object diggerPrefab;

    public HashSet<CaveBody> caves;
    public HashSet<CaveEntrance> entrances;

    static System.Random rand;

    public static CaveManager manager;

    // Use this for initialization
    void Start()
    {
        //Debug.Log("trying to start cave manager");
        if (!isServer) { return; }

        //Debug.Log("starting cave manager");
        caves = new HashSet<CaveBody>();
        entrances = new HashSet<CaveEntrance>();
        diggers = new HashSet<Digger>();
        diggerPrefab = Resources.Load("Prefabs/Map/Digger");
        manager = this;
        //Debug.Log("found digger : " + diggerPrefab);
    }

    internal static GameObject getNewDigger()
    {
        return (GameObject)Instantiate(diggerPrefab, new Vector3(0, 0, 0), Quaternion.LookRotation(new Vector3(0, 0, 1)));
    }

    public static void digCaves()
    {
        rand = new System.Random();
        Debug.Log("cave manager digging caves");
        shatters = MapManager.manager.shatters;
        MapManager.manager.shatters = 0;
        for (int i = 0; i < caveNo; i++)
        {
            CaveEntrance entrance = new CaveEntrance();
            int colID = rand.Next(0, MapManager.manager.voxels[0].Count - 1);
            bool farEnough = false;
            while (!farEnough)
            {
                farEnough = true;
                foreach (CaveEntrance ent in manager.entrances)
                {
                    double dist = Vector3.Distance(MapManager.manager.getPositionOf(0, colID), MapManager.manager.getPositionOf(0, ent.columnID));
                    if (dist < 110)
                    {
                        farEnough = false;
                        //Debug.Log("found another entrance too close: " + dist);
                    }
                }
                if (!farEnough)
                {
                    colID = rand.Next(0, MapManager.manager.voxels[0].Count - 1);
                }
            }
            entrance.createEntranceAt(colID);
        }
    }

    

    public static void removeDigger(Digger d)
    {
        diggers.Remove(d);
        Destroy(d.gameObject);
        if (diggers.Count <= 0)
        {
            //MapManager.SmoothVoxels();
            if (MapManager.useHills)
            {
                MapManager.manager.deviateHeights();
            }
            else
            {
                Debug.Log("finisheing map - dug caves - not making hills");
                MapManager.manager.finishMap();
            }
            manager.StartCoroutine(manager.RestoreShatters());
        }
    }

    IEnumerator RestoreShatters()
    {
        yield return new WaitForSecondsRealtime(1);
        MapManager.manager.shatters = shatters;

    }

    // Update is called once per frame
    void Update()
    {

    }
}
