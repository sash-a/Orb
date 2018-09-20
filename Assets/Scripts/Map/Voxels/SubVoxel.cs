using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SubVoxel : NetworkBehaviour
{
    public Transform voxelPosition;



    public  void Start()
    {
        // Set the position to the world centre of the voxel so that its position can be tracked
        voxelPosition.position = GetComponent<Voxel>().worldCentreOfObject;

        if (!MapManager.manager.mapDoneLocally)
        {
            Debug.LogError("spawning subvoxel before map has been completed");
        }

        if (!isServer) {
            //if this voxel was spawned on a client - it has no properties, it must find the locally created subvoxel and 
            //inherrit its properties before switching out the local version with this network synced version

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

                spawnedVox.delegateTexture();
                GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().mesh;
                GetComponent<MeshCollider>().convex = false;


                double scale = Math.Pow(Voxel.scaleRatio, Math.Abs(spawnedVox.layer)) * MapManager.mapSize;
                transform.localScale = Vector3.one * (float)scale;
                //Debug.Log("absorbing found vox into spawned vox ; making scale: " + scale);

                if (spawnedVox.shatterLevel == 0)
                {
                    MapManager.manager.voxels[spawnedVox.layer][spawnedVox.columnID] = spawnedVox;
                }
                else
                {
                    MapManager.manager.replaceSubVoxel(spawnedVox);
                }

                Destroy(foundVox.gameObject);
            }
            else {
                Debug.LogError("identical found and spawned sub vox on server = " + isServer);
            }
        }

        
    }

    // Update is called once per frame
    void Update()
    {
    }
}