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
        PickUpItem item = collect.GetComponent<PickUpItem>();
        MapManager.manager.collectables.Add(item);

        if (type.Equals(Type.ARTIFACT))
        {
            item.itemClass = PickUpItem.Class.MAGICIAN;
            item.itemType = (PickUpItem.ItemType)(UnityEngine.Random.Range(PickUpItem.numGuns, PickUpItem.numGuns + PickUpItem.numArtifacts));
        }
        else
        {
            item.itemClass = PickUpItem.Class.GUNNER;
            item.itemType = (PickUpItem.ItemType)(UnityEngine.Random.Range(0, PickUpItem.numGuns));
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
