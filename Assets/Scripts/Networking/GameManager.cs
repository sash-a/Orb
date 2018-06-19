using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static Dictionary<string, Identifier> networkObjects = new Dictionary<string, Identifier>();

    public static void register(string id, Identifier identifier)
    {
        string key = identifier.typePrefix + id;
        networkObjects.Add(key, identifier);

        identifier.id = key;
        identifier.transform.name = key;
    }

    /*
     * ID should always be = transform.name
     */
    public static void deregister(string id)
    {
        networkObjects.Remove(id);
    }

    /*
     * Player ID should always be = transform.name
     */
    public static Identifier getObject(string id)
    {
        return networkObjects[id];
    }
}