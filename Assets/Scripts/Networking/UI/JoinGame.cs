using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Networking.Match;

public class JoinGame : MonoBehaviour
{
    private NetworkManager netMan;
    private List<GameObject> roomList = new List<GameObject>();

    [SerializeField] private Text status;
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private Transform roomParent;


    void Start()
    {
        netMan = NetworkManager.singleton;
        if (netMan.matchMaker == null) netMan.StartMatchMaker();

        refreshRoomList();
    }

    private void OnEnable()
    {
        netMan = NetworkManager.singleton;
        netMan.StopMatchMaker();
        netMan.StartMatchMaker();
        
        if (netMan.matchMaker == null) netMan.StartMatchMaker();

        refreshRoomList();
    }

    public void refreshRoomList()
    {
        clearRoomList();
        
        // This needs to happen after client has exited game and tries to rejoin
        // This is buggy af and will hopefully not end up being a permanent fix
        netMan.StopMatchMaker();
        netMan.StartMatchMaker();
        
        netMan.matchMaker.ListMatches(0, 20, "", true, 0, 0, updateRoomList);
        status.text = "Loading...";
    }

    private void updateRoomList(bool success, string _, List<MatchInfoSnapshot> matches)
    {
        if (!success || matches == null)
        {
            status.text = "Error couldn't retrieve room list";
            return;
        }

        foreach (var matchInfo in matches)
        {
            GameObject room = Instantiate(roomPrefab);
            room.transform.SetParent(roomParent);

            roomList.Add(room);
            room.GetComponent<RoomListItem>().setUp(matchInfo);
        }

        if (roomList.Count == 0)
        {
            status.text = "No rooms right now";
            return;
        }

        status.text = "";
    }

    private void clearRoomList()
    {
        foreach (var room in roomList)
        {
            Destroy(room);
        }

        roomList.Clear();
    }
}