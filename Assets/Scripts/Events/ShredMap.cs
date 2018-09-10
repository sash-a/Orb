using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ShredMap : GameEvent
{
    public static int countDown = 25;


    public ShredMap(float startTime)
    {
        this.startTime = startTime;
        isTimeBased = true;
        countDownPeriod = countDown;
        name = "map shredding";
        message = "next map shredding in: ";
        serverOnly = true;
    }

    public override void execute()
    {
        //yield return new WaitForEndOfFrame();
        //Debug.Log("automatically shredding map");
        MapManager.manager.CmdShredMap();
    }

    public override bool hook()
    {
        throw new System.NotImplementedException();
    }

    public override void passMessage(string message)
    {
        throw new System.NotImplementedException();
    }

    public override void start()
    {
        validateParameters();
    }

    public override int getCountDownValue()
    {
        if (GameEventManager.clockTime <= startTime && GameEventManager.clockTime >= startTime - countDownPeriod && isTimeBased)
        {
            if (MapManager.manager.warningShell == null)
            {
                MapManager.manager.CmdCreateWarningShell();
            }
            return Mathf.RoundToInt(startTime - GameEventManager.clockTime);

        }
        else
        {
            return -1;
        }
    }

   
}
