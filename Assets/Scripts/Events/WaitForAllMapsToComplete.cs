using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForAllMapsToComplete : GameEvent
{
    int completedMaps = 0;
    int noPlayers = -1;

    public WaitForAllMapsToComplete() {
        //Debug.Log("no players based on net connections = " + Network.connections.Length);
        //Debug.Log("no players based on team manager = " + TeamManager.singleton.players.Count);
        noPlayers = TeamManager.singleton.players.Count;
    }

    public override void start()
    {
        isTimeBased = false;
        serverOnly = true;
        name = "waitForMapCompletion";
        validateParameters();
        if (noPlayers == -1) {
            Debug.LogError("invalid number of players given : " + noPlayers);
        }
        //Debug.Log("started a wait for maps event");
    }

    public override void execute()
    {
        //spawns players on the map
        TeamManager.singleton.CmdSpawnPlayers();
    }

    public override bool hook()
    {
        if (completedMaps == noPlayers)
        {
            Debug.Log("all maps completed!");
            return true;
        }
        else {
            if (completedMaps > noPlayers) {
                Debug.LogError("error with map completed reporting - claims " + completedMaps + " completed maps");
            }
            return false;
        }
    }

    public override void passMessage(string message)
    {
        if (message.Contains("mapCompleted")) {
            completedMaps++;
        }
    }
}
