using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveBody : CaveComponent {

    int caveSize;
    bool hasWeepHole;//a weephole is a drop style entrance - a hole in the ceiling
    Vector3 center;

    int length = 8;


    public override void informArrived()
    {
        
        digger.transform.localScale += Digger.stdscale * 1.01f;
        if (digger.transform.localScale.magnitude > Digger.maxSize)
        {
            digger.transform.localScale = digger.transform.localScale.normalized * Digger.maxSize;
        }
        
    }

}
