using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpItem : MonoBehaviour {

    public static int numGuns = 1;
    public static int numArtifacts = 3;

    public enum Class {
        GUNNER, MAGICIAN
    };
    public Class itemClass;

    public enum ItemType
    {
        EXPLOSIVE_CROSSBOW, DAMAGE_ARTIFACT, HEALER_ARTIFACT, TELEPATH_ARTIFACT
    };
    public ItemType itemType;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void pickedUp()
    {
        //play some effect or something
        MapManager.manager.collectables.Remove(this);
        Destroy(gameObject);
    }
}
