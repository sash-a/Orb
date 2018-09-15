using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TeamManager : NetworkBehaviour
{

    public HashSet<PlayerController> players;
    public Team magicians;
    public Team gunners;

    public static string localPlayerName;
    public static TeamManager singleton;
    public static PlayerController localPlayer;

    public static int playerCount;//incremented by lobby system


    Vector3[] spawnPoints;


    // Use this for initialization
    void Start()
    {
        //Debug.Log("starting team manager");
        singleton = this;
        players = new HashSet<PlayerController>();
        magicians = new Team("magicians");
        gunners = new Team("gunners");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void addPlayer(PlayerController player)
    {
        players.Add(player);
        if (player.gameObject.name.Contains("unner"))
        {
            gunners.addPlayer(player);
        }
        else if (player.gameObject.name.Contains("agician"))
        {
            magicians.addPlayer(player);
        }
        else
        {
            Debug.LogError("cannot assign player to team - given player name: " + player.gameObject.name);
        }
        player.sendToSpawnRoom();
    }

    [Command]
    public void CmdSpawnAllPlayers()
    {//should only be spawned server side
        if (!isServer)
        {
            Debug.LogError("trying to spawn players client side");
        }
        Debug.Log("spawning players");
        spawnPoints = getFurthestClearings();

        StartCoroutine(sendRedundantSpawnMessages(spawnPoints[0], spawnPoints[1]));
    }

    IEnumerator sendRedundantSpawnMessages(Vector3 spawnPoint1, Vector3 spawnPoint2) {
        for (int i = 0; i < 5; i++)
        {
            RpcSpawnAllPlayers(spawnPoints[0], spawnPoints[1]);
            yield return new WaitForSecondsRealtime(1f);
        }
    }

    bool spawned = false;
    [ClientRpc]
    void RpcSpawnAllPlayers(Vector3 spawnPoint1, Vector3 spawnPoint2)
    {
        if (spawned) {
            return;
        }
        spawned = true;

        magicians.mapSpawnPoint = spawnPoint1;
        gunners.mapSpawnPoint = spawnPoint2;
     
        RespawnPlayer spawn = new RespawnPlayer(localPlayer,GameEventManager.clockTime + 5);
        GameEventManager.singleton.addEvent(spawn);
    }

    private Vector3[] getFurthestClearings()
    {
        //search through surface voxels to find biggest and furthest apart clearings
        HashSet<Voxel> clearingCandidates = new HashSet<Voxel>();

        int samples = 50;//number of empty voxels that will be checked to be clearings


        System.Random rand = new System.Random(0);

        for (int i = 0; i < samples; i++)
        {
            Voxel seed = MapManager.manager.voxels[0][rand.Next(0, MapManager.manager.voxels[0].Count)];
            int triesRemaining = 20;
            while ((seed.isMelted || seed.mainAsset != null || !seedFarEnough(seed, clearingCandidates)) && triesRemaining>0)
            {
                seed = MapManager.manager.voxels[0][rand.Next(0, MapManager.manager.voxels[0].Count)];
                triesRemaining--;
            }
            if (triesRemaining <= 0)
            {
                Debug.LogError("failed to find a place for a new clearing seed on sample " + i);
            }
            else
            {
                if (seedClearEnough(seed))
                {
                    clearingCandidates.Add(seed);
                }
            }
        }

        //Debug.Log(clearingCandidates.Count + " / " + samples + " of clearings passed ");

        if (clearingCandidates.Count < 2) {
            Debug.LogError("too few clearing candidates");
        }

        Voxel first = null;
        Voxel second = null;
        double maxDistance = -1;
        foreach(Voxel v in clearingCandidates) {
            if (first == null)
            {
                first = v;
            }
            else
            {
                double d = Vector3.Distance(v.worldCentreOfObject, first.worldCentreOfObject);
                if (d > maxDistance) {
                    maxDistance = d;
                    second = v;
                }
            }
            }

        StartCoroutine(first.setTexture(Resources.Load<Material>("Materials/Earth/LowPolyCaveWalls")));
        StartCoroutine(second.setTexture(Resources.Load<Material>("Materials/Earth/LowPolyCaveWalls")));

        return new Vector3[] { first.worldCentreOfObject*0.9f,second.worldCentreOfObject*0.9f};
    }

    private bool seedClearEnough(Voxel seed)
    {
        foreach (Voxel v in MapManager.manager.voxels[0].Values)
        {
            if (v.mainAsset != null || v.isMelted || MapManager.manager.isDeleted(v.layer, v.columnID))
            {
                if (Vector3.Distance(v.worldCentreOfObject, seed.worldCentreOfObject) < 26)
                {
                    //Debug.Log("too close to vox with main asset : " + (v.mainAsset != null));
                    return false;
                }
            }

        }
        return true;
    }

    private bool seedFarEnough(Voxel seed, HashSet<Voxel> clearingCandidates)
    {
        int minimumDistance = 60;

        foreach (Voxel clearing in clearingCandidates)
        {
            double d = Vector3.Distance(clearing.worldCentreOfObject, seed.worldCentreOfObject);
            if (d < minimumDistance)
            {
                return false;
            }
        }
        return true;
    }
}
