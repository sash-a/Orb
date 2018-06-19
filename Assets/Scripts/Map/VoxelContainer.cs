using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelContainer :  Voxel {

    /*
     * A voxel container is a voxel which has been broken into peices 
     * while the entire thing behaves as a single voxel - this is only for storage purposes, in fact it is a container for further subvoxels
     * each of which occupies the same layer,column tuple position - but have different meshes
     */

    ArrayList subVoxels;

    public void createVoxelContainer(Voxel majorVoxel) {
        Mesh majorMesh = majorVoxel.filter.mesh;
        if (majorVoxel.deletedPoints.Count == 0) {//is a full voxel
            Vector3 center = majorVoxel.centreOfObject;
            //need to construct 6 submeshes using the major mesh data
            for (int i = 0; i < 6; i++)
            {
                //sub mesh i is the triangle that goes from vert i -> vert (i+1)%3 -> center - 0,1,2 is bottom 3    4,5,6 is top 3

            }
        }

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
