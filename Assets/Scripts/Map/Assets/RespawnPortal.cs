using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPortal : Portal {

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
