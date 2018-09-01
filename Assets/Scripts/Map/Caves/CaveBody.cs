using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveBody : CaveComponent
{

    int caveSize;
    public int tier;
    bool hasWeepHole;//a weephole is a drop style entrance - a hole in the ceiling
    public Vector3 center;
    public int centerColumnID;
    public int centerDepth;

    int length = 10;

    public CaveBody(Digger d) : base(d)
    {
        //Debug.Log("creating cave body - adding to caves list");
        digger.neighbourCount = 0;
        digger.gradient = 0;
        //Debug.Log("before: " + digger.master is CaveEntrance);
        digger.master = this;
        //ebug.Log("after: " + digger.master is CaveEntrance);
        center = Vector3.zero;
        CaveManager.caves.Add(this);
        digger.setScale(8);

    }


    public override void informArrived()
    {

        if (digger.neighbourCount < length / 5)
        {//grow
            if (digger.transform.localScale.magnitude < Digger.maxSize)
            {
                digger.transform.localScale += Digger.stdscale * 4.5f;
            }
        }
        else if (digger.neighbourCount < length / 3) {
            if (digger.transform.localScale.magnitude < Digger.maxSize)
            {
                digger.transform.localScale += Digger.stdscale * 6.5f;
            }
        }
        else if (digger.neighbourCount < length / 2)
        {
            if (digger.transform.localScale.magnitude < Digger.maxSize)
            {
                digger.transform.localScale += Digger.stdscale * 10.5f;
            }
        }
        else
        {//shrink
            if (center == Vector3.zero) {
                center = digger.transform.position;
                centerColumnID = digger.colID;
                centerDepth = digger.layer;
            }
            if (digger.transform.localScale.magnitude > Digger.minSize)
            {
                digger.transform.localScale -= Digger.stdscale * 10.5f;
            }
        }

        if (digger.neighbourCount >= length)//done digging entrance
        {
            digger.neighbourCount = 0;
            //Debug.Log("digger finished digging body ; shatters = " + MapManager.shatters);

            CaveManager.manager.removeDigger(digger);

        }

    }

}
