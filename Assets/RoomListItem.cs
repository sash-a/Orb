using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

public class RoomListItem : MonoBehaviour
{
//    public delegate void JoinRoomDelegate(MatchInfoSnapshot matchInfo);
//
//    private JoinRoomDelegate joinRoomCallback;
    
    private MatchInfoSnapshot matchInfo;
    [SerializeField] private Text roomInfo;
    [SerializeField] private Canvas menu;

    private NetworkManager netMan = NetworkManager.singleton;

    private void Start()
    {
        menu = GameObject.Find("HomeMenu").GetComponent<Canvas>();
    }

    public void setUp(MatchInfoSnapshot _matchInfo)
    {
        matchInfo = _matchInfo;
        roomInfo.text = "Lobby: " + matchInfo.name + " has " +
                      matchInfo.currentSize + "/" + matchInfo.maxSize + " players";
    }

    public void joinMatch()
    {
        netMan.matchMaker.JoinMatch(matchInfo.networkId, "", "", "", 0, 0, netMan.OnMatchJoined);
        menu.enabled = false;
    }

   
}