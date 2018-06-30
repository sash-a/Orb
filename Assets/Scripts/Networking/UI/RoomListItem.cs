using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

public class RoomListItem : MonoBehaviour
{   
    private MatchInfoSnapshot matchInfo;
    [SerializeField] private Text roomInfo;

    private NetworkManager netMan;

    private void Start()
    {
        netMan = NetworkManager.singleton;
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
    }
}