﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveBody : CaveComponent
{

    int caveSize;
    bool hasWeepHole;//a weephole is a drop style entrance - a hole in the ceiling
    Vector3 center;

    int length = 10;

    public CaveBody(Digger d) : base(d)
    {
        digger.neighbourCount = 0;
        digger.gradient = 0;
        //Debug.Log("before: " + digger.master is CaveEntrance);
        digger.master = this;
        //ebug.Log("after: " + digger.master is CaveEntrance);
        center = Vector3.zero;
        CaveManager.manager.caves.Add(this);
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

            CaveManager.removeDigger(digger);

        }

    }

}