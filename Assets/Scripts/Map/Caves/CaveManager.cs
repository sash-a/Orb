using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CaveManager : NetworkBehaviour
{


    public static HashSet<Digger> diggers;
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
        float estimatedEntranceDistance = 0.5f * MapManager.mapSize / (float)Math.Pow(2, MapManager.splits);

        rand = new System.Random();
        //Debug.Log("cave manager digging caves");
        shatters = MapManager.manager.shatters;
        MapManager.manager.shatters = 0;
        for (int i = 0; i < MapManager.noCaves; i++)
        {
            CaveEntrance entrance = new CaveEntrance();
            int colID = rand.Next(0, MapManager.manager.voxels[0].Count - 1);
            bool farEnough = false;

            int remainingTries = 100;

            while (!farEnough && remainingTries > 0)
            {
                farEnough = true;
                foreach (CaveEntrance ent in manager.entrances)
                {//checks if proposed position of new cave entrance is too close to any existing cave entrance or an estimated position of a cave body
                    if (Vector3.Distance(MapManager.manager.getPositionOf(0, colID), MapManager.manager.getPositionOf(0, ent.columnID)) < 160 ||
                        Vector3.Distance(MapManager.manager.getPositionOf(0, colID), MapManager.manager.getPositionOf(0, ent.columnID) + ent.direction.normalized * estimatedEntranceDistance * 0.7f) < 130
                        )
                    {
                        farEnough = false;
                        //Debug.Log("found another entrance too close: " + dist);
                    }
                }
                if (farEnough)//the new cave entrance is far enough away from all other cave entrances
                {
                    Vector3 dir = Vector3.zero;

                    int count = 0;
                    foreach (CaveEntrance ent in manager.entrances)
                    {//points the dir of the new cave entrance away from nearby entrances and cave bodies
                        Vector3 estimatedCavePosition = MapManager.manager.getPositionOf(0, ent.columnID) + ent.direction.normalized * estimatedEntranceDistance * 0.7f;
                        float dist = Vector3.Distance(MapManager.manager.getPositionOf(0, colID), estimatedCavePosition);
                        dir += 0.5f * (MapManager.manager.getPositionOf(0, colID) - estimatedCavePosition).normalized / (float)Math.Pow(Vector3.Distance(MapManager.manager.getPositionOf(0, colID), estimatedCavePosition), 3);//the closer the cave body the more repelling effect it has
                        dir += (MapManager.manager.getPositionOf(0, colID) - MapManager.manager.getPositionOf(0, ent.columnID)).normalized / (float)Math.Pow(Vector3.Distance(MapManager.manager.getPositionOf(0, colID), MapManager.manager.getPositionOf(0, ent.columnID)), 3);//the closer the cave body the more repelling effect it has

                        count++;
                    }
                    dir = planariseDir(colID, dir);
                    //dir is now a direction which is pointing away from all other

                    
                    foreach (CaveEntrance ent in manager.entrances)
                    {
                        double dist = Vector3.Distance(MapManager.manager.getPositionOf(0, colID), MapManager.manager.getPositionOf(0, ent.columnID));
                        if (Vector3.Distance(MapManager.manager.getPositionOf(0, ent.columnID), MapManager.manager.getPositionOf(0, colID) + dir.normalized * estimatedEntranceDistance) < 110)
                        {//the projected destination of this cave entrance is too close to another cave entrance
                            farEnough = false;
                        }
                    }
                    
                    //dir = planariseDir(colID, dir);
                    if (farEnough)
                    {
                        if (count > 0)
                        {
                            //Debug.Log("creating cave entrance with direction dervied from surrounding cave bodies");
                            entrance.createEntranceAt(colID, dir);
                        }
                        else
                        {//this is the first cave entrance
                            entrance.createEntranceAt(colID);
                        }
                    }
                    else
                    {
                        remainingTries--;
                    }
                }
                else
                {
                    colID = rand.Next(0, MapManager.manager.voxels[0].Count - 1);
                    remainingTries--;
                }

            }
            if (remainingTries == 0)
            {
                Debug.LogError("cave manager failed to place a cave entrance");
            }
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

    public static Vector3 planariseDir(int colID, Vector3 dir)
    {
        Vector3 cross = Vector3.Cross(dir, MapManager.manager.getPositionOf(0, colID)).normalized;
        Vector3 newDir = Vector3.Cross(cross, MapManager.manager.getPositionOf(0, colID)).normalized;
        if (Vector3.Dot(newDir, dir) < 0)
        {//new dir facing wrong way
            newDir = newDir * -1;
        }
        //Debug.Log("planarised dir from " + dir.normalized + " to " + newDir);
        return newDir;
    }
}
