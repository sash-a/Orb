﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveEntrance : CaveComponent
{

    CaveComponent destination;
    int destDepth;//the depth the cave entrance ends at
    int columnID;//the column id of the surface voxel this entrance begins at

    static int entrancesize = 4;

    public CaveEntrance() : base(){
        destDepth = 6;
    }

    public CaveEntrance(int depth) : this(){
        destDepth = depth;
    }

    public void createEntranceAt(int colID)
    {
        columnID = colID;

        GameObject digObj = CaveManager.getNewDigger();
        digObj.transform.localScale = new Vector3(1, 1, 1);
        digger = digObj.GetComponent<Digger>();
        digger.init(this);
        CaveManager.diggers.Add(digger);

        int n = rand.Next(0, 2);



        digger.layer = 0;
        digger.neighbourCount = 0;
        digger.transform.position = Vector3.zero;
        digger.setScale(entrancesize);
        digger.gradient = 2;

        int count = 0;
        Vector3 dir = Vector3.zero;
        int neighbour = -1; ;
        foreach (int nei in MapManager.manager.neighboursMap[colID])
        {
            if (count == n)
            {
                dir = MapManager.manager.getPositionOf(0, nei) - MapManager.manager.getPositionOf(0, colID);
                neighbour = nei;
                break;
            }
            count++;
        }

        digger.right = Vector3.Cross(dir, -MapManager.manager.getPositionOf(0, neighbour));
        digger.travelDir = dir.normalized;
        digger.nextDest = MapManager.manager.getPositionOf(0, colID);

        digger.gameObject.SetActive(true);

        digger.travelToNext();
    }

    public override void informArrived()
    {
        digger.gameObject.transform.localScale *= 0.999f;

        if (digger.layer >= destDepth)//done digging entrance
        {
            digger.neighbourCount = 0;
            digger.travelDir = digger.travelDir.normalized + digger.right * (float)(rand.NextDouble() * 1.5f - 0.75f);
            digger.right = Vector3.Cross(digger.travelDir, -digger.gameObject.transform.position);

            //digger.gameObject.active = false;
            CaveManager.removeDigger(digger);
            Debug.Log("digger finished digging entrance");

        }
    }
}
