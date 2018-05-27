using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const string PLAYER_ID_PREFIX = "Player:";

    private static Dictionary<string, Player> players = new Dictionary<string, Player>();

    public static void registerPlayer(string id, Player player)
    {
        string playerID = PLAYER_ID_PREFIX + id;
        players.Add(playerID, player);

        player.transform.name = playerID;
    }

    /*
     * Player ID should always be = transform.name
     */
    public static void deregisterPlayer(string playerID)
    {
        players.Remove(playerID);
    }

    /*
     * Player ID should always be = transform.name
     */
    public static Player getPlayer(string playerID)
    {
        return players[playerID];
    }
}