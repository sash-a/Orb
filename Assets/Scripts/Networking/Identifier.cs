using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class Identifier : MonoBehaviour
{
    public static string magicianType = "Magician";
    public static string gunnerType = "Gunner";
    
    public string id;
    public string typePrefix;

    public PlayerUI UI;
}