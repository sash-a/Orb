using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class PauseMenu : MonoBehaviour
{
//    public static bool isPaused;
    private NetworkManager netMan;

    void Start()
    {
        netMan = NetworkManager.singleton;
    }

    public void leaveRoom()
    {
        MatchInfo matchInfo = netMan.matchInfo;
        netMan.matchMaker.DropConnection(matchInfo.networkId, matchInfo.nodeId, 0, netMan.OnDropConnection);
        netMan.StopHost(); // If host quits room will die

//        isPaused = false;
    }
}