using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkConfig : MonoBehaviour {

    ConnectionConfig config;

	// Use this for initialization
	void Start () {
        config = GetComponent<ConnectionConfig>();
        config.NetworkDropThreshold = 60;//50% of packets can be dropped before drop connection request - reconmended by unity for wireless connections
        config.OverflowDropThreshold = 15;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
