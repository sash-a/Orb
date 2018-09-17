using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPortal : Portal {

	
	
	// Update is called once per frame
	void Update () {
        //Debug.Log(" spawnerPortal!");

    }

    public override void OnCollisionEnter(Collision collision)
    {
        player = collision.gameObject;
        if (player.GetComponent<PlayerController>() != null)
        {
            RespawnPlayer spawn = new RespawnPlayer(TeamManager.localPlayer, GameEventManager.clockTime);
            GameEventManager.singleton.addEvent(spawn);
        }
    }
}
