using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveEntrance : CaveComponent
{

    CaveComponent destination;
    public Vector3 direction;
    int destDepth;//the depth the cave entrance ends at
    public int columnID;//the column id of the surface voxel this entrance begins at

    static int entrancesize = 7;

    public CaveEntrance() : base(){
        destDepth = 6;
    }

    public CaveEntrance(int depth) : this(){
        destDepth = depth;
    }

    public void createEntranceAt(int colID)
    {
        //Debug.Log("creating cave entrance at: " + colID + " with random dir" );

        int n = rand.Next(0, 2);
        int count = 0;
        Vector3 dir = Vector3.zero;
        foreach (int nei in MapManager.manager.neighboursMap[colID])
        {
            if (count == n)
            {
                dir = MapManager.manager.getPositionOf(0, nei) - MapManager.manager.getPositionOf(0, colID);
                break;
            }
            count++;
        }

        createEntranceAt(colID, dir);
       
    }

    int travelCounter = 2;


    public override void informArrived()
    {
        digger.gameObject.transform.localScale *= 0.999f;

        //Debug.Log("digger arrived at: " + digger.colID);


        if (digger.layer >= destDepth)//done digging entrance
        {
            digger.neighbourCount = 0;
            //digger.travelDir = (digger.travelDir.normalized + digger.right * (float)(rand.NextDouble() * 0.7f - 0.35f)).normalized;
            //digger.right = Vector3.Cross(digger.travelDir, -digger.gameObject.transform.position);
            //digger.gameObject.active = false;
            //CaveManager.removeDigger(digger);

            if (travelCounter <= 0)
            {
                createCaveBody();
             }
            else {
                travelCounter--;
                digger.gradient = 0;
            }
        }
    }

   

    internal void createEntranceAt(int colID, Vector3 dir)
    {
        columnID = colID;

        CaveManager.entrances.Add(this);

        GameObject digObj = CaveManager.getNewDigger();
        digObj.transform.localScale = new Vector3(1, 1, 1);
        digger = digObj.GetComponent<Digger>();
        digger.init(this);
        digger.colID = colID;
        digger.tier = 0;
        digger.layer = 0;
        digger.neighbourCount = 0;
        digger.transform.position = Vector3.zero;
        digger.setScale(entrancesize);
        digger.gradient = 2;
        direction = dir;


        digger.right = Vector3.Cross(direction, -MapManager.manager.getPositionOf(0, columnID)).normalized;
        digger.travelDir = direction.normalized;
        //Debug.Log("creating new digger with dir = " + digger.travelDir + " right = " + digger.right);
        digger.nextDest = 3* MapManager.manager.getPositionOf(0, colID) - 2 * MapManager.manager.getPositionOf(1, colID);

        digger.gameObject.SetActive(true);

        digger.travelToNext();
    }

 
}
