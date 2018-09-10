using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Team{

    public string teamName;
    public HashSet<PlayerController> players;
    public GameObject SpawnRoom;
    public Vector3 mapSpawnPoint;

    // Use this for initialization
    public Team(string name) {
        teamName = name;
        players = new HashSet<PlayerController>();
        getSpawnRoom();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    internal void addPlayer(PlayerController player)
    {
        player.team = this;
        players.Add(player);
        player.sendToSpawnRoom();
    }

    [ClientRpc]
    internal void RpcInformKilled(PlayerController player)
    {
        players.Remove(player);
        //Destroy()
        //if (player == TeamManager.localPlayer)
        //{
        //    player.sendToSpawnRoom();
        //}
        if (players.Count <= 0) {
            Debug.Log(teamName + " have been defeated ");
        }
    }

    public GameObject getSpawnRoom() {
        if (teamName.Contains("unner"))
        {
            SpawnRoom = GameObject.Find("GunnerSpawnRoom");
        }
        else if (teamName.Contains("agician"))
        {
            SpawnRoom = GameObject.Find("MagicianSpawnRoom");
        }
        else
        {
            Debug.LogError("cannot assign team spawn room - given player name: " + teamName);
        }
        if (SpawnRoom == null)
        {
            Debug.LogError("could not find team " + teamName + " a spawn room ");
        }
        return SpawnRoom;
    }


}
