using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Altar : MonoBehaviour {

    public enum Type { ARTIFACT, WEAPON};
    public Type type;

	// Use this for initialization
	void Start () {
        spawnCollectable();
    }

    private void spawnCollectable()
    {
        string folder = "Prefabs/Map/MapAssets/";
        if (type.Equals(Type.ARTIFACT))
        {
            folder += "Artifacts";
        }
        else
        {
            folder += "DummyGuns";
        }
        GameObject collect = Instantiate(Resources.Load<GameObject>(folder), transform.position, transform.rotation);
        collect.transform.position += collect.transform.up;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
