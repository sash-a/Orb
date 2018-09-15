using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkMessagePasser : NetworkBehaviour
{
    private bool informedMapDoneLocally = false;


    public static NetworkMessagePasser singleton;

    struct Message
    {
        public string mess;
        public bool showOnServer;
        public int duration;

        public Message(string m, bool s, int d)
        {
            mess = m;
            showOnServer = s;
            duration = d;
        }
    }

    List<Message> syncedUIMessages;


    private void Start()
    {
        syncedUIMessages = new List<Message>();
        if (isLocalPlayer)
        {
            singleton = this;
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        if (MapManager.manager != null && MapManager.manager.mapDoneLocally && !informedMapDoneLocally)
        {
            informedMapDoneLocally = true;
            //Debug.Log("Map done locally (called on player)");
            CmdInformMapDoneLocally();
            //this.enabled = false;
        }

        for (int i = 0; i < syncedUIMessages.Count; i++)
        {
            CmdPassUIMessageToClients(syncedUIMessages[i].mess, syncedUIMessages[i].showOnServer, syncedUIMessages[i].duration);
            syncedUIMessages.RemoveAt(i);
            i--;
        }
    }

    [Command]
    void CmdInformMapDoneLocally()
    {
        //Debug.Log("Player telling server map done");
        GameEventManager.singleton.passMessage("waitForMapCompletion", "mapCompleted");
        //Destroy(this);
    }

    [Command]
    void CmdPassUIMessageToClients(string message, bool show, int dur)
    {
        RpcRecieveUIMessage(message, show, dur);
    }

    [ClientRpc]
    void RpcRecieveUIMessage(string message, bool show, int dur)
    {
        if (show || !isServer)
        {
            new UIMessage(message, dur);
        }
    }

    public void addSyncUIMessage(string message, bool show, int dur)
    {
        syncedUIMessages.Add(new Message(message, show, dur));
    }
}