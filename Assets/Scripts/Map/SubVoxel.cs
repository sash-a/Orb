using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SubVoxel : NetworkBehaviour
{


    private void Start()
    {
        //Debug.Log("a found voxel has a pos: " + gameObject.GetComponent<Voxel>().layer + " , " + gameObject.GetComponent<Voxel>().columnID);

    }
    public override void OnStartClient()
    {
        if (!MapManager.manager.mapDoneLocally) {
            Debug.LogError("spawning subvoxel before map has been completed");
        }

        Voxel spawnedVox = gameObject.GetComponent<Voxel>();
        //Debug.Log("a spawned voxel has a pos: " + spawnedVox.layer + " , " + spawnedVox.columnID);
        Voxel foundVox = MapManager.manager.getSubVoxelAt(spawnedVox.layer, spawnedVox.columnID, spawnedVox.subVoxelID);
        if (foundVox != spawnedVox)
        {

            GetComponent<MeshFilter>().mesh.vertices = foundVox.GetComponent<MeshFilter>().mesh.vertices;
            GetComponent<MeshFilter>().mesh.triangles = foundVox.GetComponent<MeshFilter>().mesh.triangles;
            GetComponent<MeshFilter>().mesh.uv = foundVox.GetComponent<MeshFilter>().mesh.uv;

            spawnedVox.cloneMeshFilter();

            GetComponent<MeshFilter>().mesh.RecalculateNormals();

            spawnedVox.setTexture();
            GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().mesh;
            GetComponent<MeshCollider>().convex = false;


            double scale = Math.Pow(Voxel.scaleRatio, Math.Abs(spawnedVox.layer)) * MapManager.mapSize;
            transform.localScale = Vector3.one * (float)scale ;
            //Debug.Log("absorbing found vox into spawned vox ; making scale: " + scale);

            if (spawnedVox.shatterLevel == 0)
            {
                MapManager.manager.voxels[spawnedVox.layer][spawnedVox.columnID] = spawnedVox;
            }
            else {
                MapManager.manager.replaceSubVoxel(spawnedVox);
            }

            Destroy(foundVox.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
