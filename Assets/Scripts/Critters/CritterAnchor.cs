using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterAnchor : MonoBehaviour {

    public CritterGravity gravity;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("critter anchor hit something - " + other.gameObject.name);

        if (other.gameObject.name.Contains("oxel")){
            if (other.gameObject.GetComponent<Voxel>() != gravity.attachedVoxel     ) {
                //touching a new voxel - cant switch to new voxel if just switched to wall
                gravity.replaceVoxel(other.gameObject.GetComponent<Voxel>());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Contains("oxel"))
        {
            if (other.gameObject.GetComponent<Voxel>() == gravity.attachedVoxel)
            {
                //touching a new voxel
                gravity.attachedVoxel = null;
            }
        }
    }
}
