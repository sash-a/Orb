using UnityEngine;
using UnityEngine.Networking;

public class HostGame : MonoBehaviour
{
    [SerializeField] private uint roomSize = 7; // 1 beast, 2 magicians, 4 gunners
    [SerializeField] private string roomName;

    private NetworkManager netMan;
    public Canvas canvas;

    void Start()
    {
        netMan = NetworkManager.singleton;
        if (netMan.matchMaker == null)
        {
            netMan.StartMatchMaker();
        }
    }

    public void setRoomName(string name)
    {
        roomName = name;
    }

    public void createRoom()
    {
        if (string.IsNullOrEmpty(roomName)) return;

        Debug.Log("Creating room: " + roomName + " num players: " + roomSize);
        netMan.matchMaker.CreateMatch(roomName, roomSize, true, "", "", "", 0, 0, netMan.OnMatchCreate);

        canvas.enabled = false;
    }
}