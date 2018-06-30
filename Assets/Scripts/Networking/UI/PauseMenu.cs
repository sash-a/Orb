using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class PauseMenu : MonoBehaviour
{
    public static bool isPaused = false;
    private NetworkManager netMan;

    [SerializeField] private Canvas HomeMenu;

    void Start()
    {
        netMan = NetworkManager.singleton;
        HomeMenu = GameObject.Find("HomeMenu").GetComponent<Canvas>();
    }

    public void leaveRoom()
    {
        MatchInfo matchInfo = netMan.matchInfo;
        netMan.matchMaker.DropConnection(matchInfo.networkId, matchInfo.nodeId, 0, netMan.OnDropConnection);
        netMan.StopHost(); // If host quits room will die

        isPaused = false;
        HomeMenu.enabled = true;
    }
}