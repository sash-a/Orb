using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//colab is kak

public class MapChunk : MonoBehaviour
{
    HashSet<Voxel> containedVoxels;
    public Vector3 chunkOrigin;
    float chunkRadius;

    int columnsRemaining = 0;
    int skipFrames = 90;

    private void Update()
    {
        transform.position += chunkOrigin.normalized * 30;//moves away until it is deleted when the next shred is scheduled to occur
    }

    public void addVoxel(Voxel v)
    {
        if (v == null)
        {
            Debug.LogError("null voxel being added to map chunk " + v);
            return;
        }

        v.isInChunk = true;
        v.gameObject.transform.parent = gameObject.transform;


        if (containedVoxels == null)
        {
            containedVoxels = new HashSet<Voxel>();
        }
        Destroy(v.GetComponent<Rigidbody>());

        NetworkTransform netTrans = v.GetComponent<NetworkTransform>();
        if (netTrans != null)
        {
            netTrans.enabled = true;
        }
        else
        {
            Debug.LogError("voxel " + v.layer + " ; " + v.columnID + " has no network transform");
        }
        containedVoxels.Add(v);
        //v.GetComponent<Rigidbody>().isKinematic = true;


        if (v.mainAsset != null)
        {
            v.mainAsset.gameObject.transform.parent = transform;
        }
    }

    public void finishChunk( float radius)//has local player authority
    {
        chunkRadius = radius;

        HashSet<Voxel> suspectedEdges = new HashSet<Voxel>();

        foreach (Voxel v in containedVoxels)
        {
            if (Vector3.Distance(v.worldCentreOfObject, chunkOrigin) > radius * 0.95)
            {
                suspectedEdges.Add(v);
            }

        }

        int edgeCount = 0;
        foreach (Voxel v in suspectedEdges)
        {
            int containedNeighboursCount = 0;
            int unspawnedNeighboursCount = 0;
            foreach (int n in MapManager.manager.neighboursMap[v.columnID])
            {
                //count number of this voxels neighbours are also in the chunk
                if (MapManager.manager.voxels[v.layer].ContainsKey(n))
                {
                    Voxel vn = MapManager.manager.voxels[v.layer][n];
                    if (containedVoxels.Contains(vn))
                    {
                        containedNeighboursCount++;
                    }
                }
                else
                {
                    unspawnedNeighboursCount++;
                }
            }

            if (containedNeighboursCount < 3 - v.getDeletedAdjacentCount() - unspawnedNeighboursCount)
            {
                //not all this voxels neighbours are in the chunk - so it must be an edge voxel
                columnsRemaining++;
                StartCoroutine(createPillarIncrementally(v));
                //createPillar(v);
                edgeCount++;
            }
        }
    }


    int batchCount = 0;
    IEnumerator createPillarIncrementally(Voxel v)//has local player authority
    {
        int batchSize = 1;
        int framesLeft = UnityEngine.Random.Range(0, skipFrames);//offsets the different pillars


        for (int i = 1; i < MapManager.mapLayers; i++)
        {
            while (framesLeft > 0)
            {
                framesLeft--;
                yield return new WaitForFixedUpdate();
            }
            framesLeft = skipFrames;


            v.createNewVoxel(i - v.layer);
            if (!MapManager.manager.isDeleted(i, v.columnID))
            {
                Voxel vox = MapManager.manager.voxels[i][v.columnID];
                if (vox != null && !vox.isMelted)
                {
                    //
                    addVoxel(vox);
                    vox.gameObject.transform.parent = gameObject.transform;
                    //Debug.Log("adding new column voxel to map chunk - parent name: " + vox.gameObject.transform.parent.gameObject.name);
                    vox.showNeighbours(false);
                    MapManager.manager.CmdInformDeleted(vox.layer, vox.columnID);//has local player authority
                    checkNeighbours(vox);

                    if (batchCount >= batchSize)
                    {
                        batchCount = 0;
                        yield return new WaitForFixedUpdate();
                        //yield return new WaitForEndOfFrame();
                    }
                    else
                    {
                        batchCount++;
                    }
                }
                else
                {
                    //todo - deal with voxel containers
                }

            }
        }
        columnsRemaining--;
    }


    public void destroyChunk()
    {

        //Debug.Log("destroying chunk");
        foreach (Voxel vox in containedVoxels)
        {
            if (vox.mainAsset != null)
            {
                NetworkServer.Destroy(vox.mainAsset.gameObject);
            }


            if (vox != null)
            {
                NetworkServer.Destroy(vox.gameObject);
            }
        }

        Destroy(gameObject);
    }

    private void checkNeighbours(Voxel vox)
    {
        foreach (int n in MapManager.manager.neighboursMap[vox.columnID])
        {
            if (!MapManager.manager.isDeleted(vox.layer, n))
            {
                if (vox != null && MapManager.manager.doesVoxelExist(vox.layer, n))
                {
                    Voxel v = MapManager.manager.voxels[vox.layer][n];
                    if (Vector3.Distance(v.worldCentreOfObject, chunkOrigin) < chunkRadius * 0.98f)
                    {
                        if (!containedVoxels.Contains(v))
                        {
                            //v is a newly created voxel which should be carried away with the map chunk
                            addVoxel(v);
                            v.gameObject.transform.parent = gameObject.transform;
                        }
                    }
                }
            }
        }
    }
}