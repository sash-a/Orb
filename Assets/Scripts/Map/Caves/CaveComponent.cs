using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CaveComponent 
{


    public Digger digger;
    public System.Random rand;

    public CaveComponent()
    {
        rand = new System.Random();
    }

    public CaveComponent(Digger d) : this()
    {
        digger = d;
    }

    public abstract void informArrived();//the digger tells the controlling cave component it has arrived at its next destination

}
