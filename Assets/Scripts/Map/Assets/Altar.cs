using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Altar : MonoBehaviour {

    public enum Type { ARTIFACT, WEAPON};
    public Type type;

	// Use this for initialization
	void Start () {
        //spawnCollectable();
    }

    public void spawnCollectable()
    {
        string folder = "Prefabs/Map/MapAssets/";
        if (type.Equals(Type.ARTIFACT))
        {
            folder += "Artifacts/TestArtifact";
        }
        else
        {
            folder += "DummyGuns";
        }
        GameObject collect =(GameObject) Instantiate(Resources.Load<UnityEngine.Object>(folder), transform.position, transform.rotation);
        collect.transform.position += transform.up*transform.parent.localScale.y;
        collect.transform.parent = transform;
        MapManager.manager.collectables.Add(collect.GetComponent<PickUpItem>());
    }

    // Update is called once per frame
    void Update () {
		
	}
}
