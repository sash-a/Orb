using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPlayer : GameEvent {

    PlayerController player;

    public RespawnPlayer(PlayerController play, float startTime) {
        countDownPeriod = 5;
        this.startTime = startTime;
        isTimeBased = true;
        name = "Spawn Player";
        message = "respawning player in: ";
        if (play == null)
        {
            Debug.LogError("trying to spawn invalid player");
        }
        else {
            player = play;
        }
    }

    public override void execute()
    {
        //yield return new WaitForEndOfFrame();
        //Debug.Log("respawning local player");
        if (TeamManager.singleton.magicians.players.Contains(player))
        {
            player.transform.position = TeamManager.singleton.magicians.mapSpawnPoint;
        }
        else if (TeamManager.singleton.gunners.players.Contains(player))
        {
            player.transform.position = TeamManager.singleton.gunners.mapSpawnPoint;
        }
        else
        {
            Debug.LogError("cannot find team for player : " + player);
        }
        player.GetComponent<Gravity>().inSphere = true;//moving player back in orb

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

    // Use this for initialization
    void Start () {
		
	}

}
