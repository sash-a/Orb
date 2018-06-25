using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MapSync : NetworkBehaviour {

    MapManager localMapManager;
    [SyncVar] public string passInfo;

	// Use this for initialization
	void Start () {
        passInfo += isServer + " ; ";
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
