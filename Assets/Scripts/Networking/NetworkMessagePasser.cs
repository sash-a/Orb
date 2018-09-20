using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// attached to player objects - only runs on local player
/// act as cmd tool to map utilities mostly
/// messages and instructions are passed to this class, it then uses its local player authority to send commands to the server appropriately
/// </summary>

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


    struct Instruction
    {
        public string message;
        public int[] args;

        public Instruction(string mess)
        {
            message = mess;
            args = new int[0];
        }

        public Instruction(string mess , params int[] numbers)
        {
            message = mess;
            args = numbers;
        }
    }

    List<Message> syncedUIMessages;
    List<Instruction> instructions;


    private void Start()
    {
        syncedUIMessages = new List<Message>();
        instructions = new List<Instruction>();
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

        AttendToUIMessage();
        AttendToInstructions();

    }

    private void AttendToInstructions()
    {
        for (int i = 0; i < instructions.Count; i++)
        {
            Instruction inst = instructions[i];
            executeInstruction(inst);
            instructions.RemoveAt(i);
            i--;
        }
    }

    private bool executeInstruction(Instruction inst)
    {
        string message = inst.message;
        bool executed = false;

        if (message == "shred_map")
        {
            //Debug.Log("sending cmd shred map");
            CmdShredMap();
            executed = true;
        }

        if (message == "deliver_player_name")
        {
            //Debug.Log("sending cmd shred map");
            PlayerActions.localActions.CmdSetPlayerName(TeamManager.localPlayerName);
            executed = true;
        }

        if (message == "delete_voxel_at")
        {
            MapManager.manager.CmdInformDeleted(inst.args[0], inst.args[1]);
            executed = true;
        }


        return executed;
    }

    [Command]
    private void CmdShredMap()
    {
        //Debug.Log("CmdShredMap");
        RpcShredMap();
    }

    [ClientRpc]
    private void RpcShredMap()
    {
        //Debug.Log("RpcShredMap");
        ShredManager.singleton.ShredMapNext();
    }

    private void AttendToUIMessage()
    {
        for (int i = 0; i < syncedUIMessages.Count; i++)
        {
            CmdPassUIMessageToClients(syncedUIMessages[i].mess, syncedUIMessages[i].showOnServer, syncedUIMessages[i].duration);
            syncedUIMessages.RemoveAt(i);
            i--;
        }
    }

    public void addSyncUIMessage(string message, bool show, int dur)
    {
        syncedUIMessages.Add(new Message(message, show, dur));
    }

    public void addSyncInstruction(string mess)
    {
        instructions.Add(new Instruction(mess));
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

    internal void informDeleted(int layer, int columnID)
    {
        MapManager.manager.CmdInformDeleted(layer, columnID);//instant reponse on local caller side
        instructions.Add(new Instruction("delete_voxel_at", layer, columnID));
    }
}