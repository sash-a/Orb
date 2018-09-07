using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShredMap : GameEvent {

    public GameObject shreddingShellPrefab;

    public ShredMap(float startTime) {
        this.startTime = startTime;
        isTimeBased = true;
        countDownPeriod = 10;
        name = "map shredding";
        serverOnly = true;
    }

    public override void execute()
    {
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

}
