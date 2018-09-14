using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WaitForAllMapsToComplete : GameEvent
{
    int completedMaps = 0;
    int noPlayers = -1;

    NetworkManager nm;

    public WaitForAllMapsToComplete()
    {
        //Debug.Log("no players based on net connections = " + Network.connections.Length);
        //Debug.Log("no players based on team manager = " + TeamManager.singleton.players.Count);
        //nm = GameObject.FindObjectOfType<NetworkManager>();
        //Debug.Log("no players based on network manager = " + nm.numPlayers);
        Debug.Log("no players based  on lobby count :" + TeamManager.playerCount);
        //noPlayers = Mathf.Max(Mathf.Max(TeamManager.singleton.players.Count, nm.numPlayers) , TeamManager.playerCount);
        noPlayers = TeamManager.playerCount;
        if (noPlayers == 0) {
            Debug.LogError("no players found in wait for all maps event");
        }
    }

    public override void start()
    {
        isTimeBased = false;
        serverOnly = true;
        name = "waitForMapCompletion";
        validateParameters();
        if (noPlayers == -1)
        {
            Debug.LogError("invalid number of players given : " + noPlayers);
        }
        //Debug.Log("started a wait for maps event");
    }

    public override void execute()//all maps have finished generating
    {
        //yield return new WaitForEndOfFrame();
        //spawns players on the map
        //Debug.Log("executing wait for maps");
        TeamManager.singleton.CmdSpawnPlayers();
        GameEventManager.singleton.CmdAddShredEvents();

    }

    int triesLeft = 1000;

    public override bool hook()
    {
        if (Time.time > 200) {
            Debug.Log("giving up waiting for all maps to complete - ending waiting and beginning spawn/ shredding process");
            return true;
        }

        if (noPlayers > 0)
        {
            if (completedMaps == noPlayers)
            {
                Debug.Log("all maps completed!");
                return true;
            }
            else
            {
                if (completedMaps > noPlayers)
                {
                    Debug.LogError("error with map completed reporting - claims " + completedMaps + " completed maps");
                }
                return false;
            }
        }
        else {
            Debug.LogError("checking wait on map hook - no players = 0");
            noPlayers = TeamManager.playerCount;
            triesLeft--;
            if (triesLeft < 0) {
                Debug.LogError("waited to find players - did not find any");
                return true;
            }
            return false;
        }
    }

    public override void passMessage(string message)
    {
        if (message.Contains("mapCompleted"))
        {
            completedMaps++;
            Debug.Log("map finished - inc completed maps to " + completedMaps);
        }
    }
}
