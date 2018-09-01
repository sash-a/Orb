using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveTunnel : CaveComponent {

    /**
     *a tunnel connects two cave components and should be flat 
     */

    CaveComponent source;
    CaveComponent destination;

    public Vector3 direction;
    static int tunnelSize = 5;
    public int tunnelDepth = 6;
    int destDepth;//the depth the cave entrance ends at


    internal void tunnelFrom(CaveBody body)
    {
        source = body;
        int n = rand.Next(0, 2);
        int count = 0;
        Vector3 dir = Vector3.zero;
        foreach (int nei in MapManager.manager.neighboursMap[body.centerColumnID])
        {
            if (count == n)
            {
                dir = MapManager.manager.getPositionOf(0, nei) - MapManager.manager.getPositionOf(0, body.centerColumnID);
                break;
            }
            count++;
        }

        tunnelFrom( body,dir);
    }

    public void tunnelFrom(  CaveBody body, Vector3 dir)
    {
        CaveManager.tunnels.Add(this);

        GameObject digObj = CaveManager.getNewDigger();
        digObj.transform.localScale = new Vector3(1, 1, 1);
        digger = digObj.GetComponent<Digger>();
        digger.init(this);
        digger.colID = body.centerColumnID;
        digger.tier = body.tier+1;
        digger.layer = body.centerDepth+2;
        destDepth = body.centerDepth + tunnelDepth;
        digger.neighbourCount = 0;
        digger.transform.position = Vector3.zero;
        digger.setScale(tunnelSize);
        digger.gradient = 2;
        direction = dir;


        digger.right = Vector3.Cross(direction, -MapManager.manager.getPositionOf(0, body.centerColumnID)).normalized;
        digger.travelDir = direction.normalized;
        //Debug.Log("creating new digger with dir = " + digger.travelDir + " right = " + digger.right);
        digger.nextDest = 3 * MapManager.manager.getPositionOf(0, body.centerColumnID) - 2 * MapManager.manager.getPositionOf(1, body.centerColumnID);

        digger.gameObject.SetActive(true);

        digger.travelToNext();
    }

    public override void informArrived()
    {
        digger.gameObject.transform.localScale *= 0.999f;

        if (digger.layer >= destDepth)//done digging entrance
        {
            digger.neighbourCount = 0;
            //digger.travelDir = (digger.travelDir.normalized + digger.right * (float)(rand.NextDouble() * 0.7f - 0.35f)).normalized;
            //digger.right = Vector3.Cross(digger.travelDir, -digger.gameObject.transform.position);
            //digger.gameObject.active = false;
            //CaveManager.removeDigger(digger);

            createCaveBody();
           
        }
    }

}
