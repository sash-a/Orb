using Prototype.NetworkLobby;
using UnityEngine;
using UnityEngine.Networking;

public class InGameTypeHook : LobbyHook
{
    public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer,
        GameObject gamePlayer)
    {
        var lobby = lobbyPlayer.GetComponent<LobbyPlayer>();
        var type = gamePlayer.GetComponent<PlayerType>();

        if (lobby.playerColor == LobbyPlayer.Colors[0]) type.type = PlayerType.GUNNER_TYPE;
        else if (lobby.playerColor == LobbyPlayer.Colors[1]) type.type = PlayerType.MAGICIAN_TYPE;
    }
}