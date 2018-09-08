using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MapChunk : MonoBehaviour
{
    HashSet<Voxel> containedVoxels;
    Vector3 chunkOrigin;
    float chunkRadius;

    private void Update()
    {
        if (transform.position.magnitude > MapManager.mapSize * 5)
        {
            destroyChunk();
        }
    }

    private void destroyChunk()
    {
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

    private void separateChunk()
    {
        foreach (Voxel vox in containedVoxels)
        {
            //Destroy(vox.GetComponent<MeshCollider>());

            if (vox.mainAsset != null) {
                vox.mainAsset.gameObject.transform.parent = transform;
            }
            MapManager.manager.CmdInformDeleted(vox.layer, vox.columnID);
        }
    }

    public void addVoxel(Voxel v)
    {
        if (v == null) {
            Debug.LogError("null voxel being added to map chunk " + v);
            return;
        }

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
       

        if (v.mainAsset != null) {
            //v.asset.changeParent(gameObject.transform);
        }
    }

    public void finishChunk(Vector3 origin, float radius)
    {
        chunkOrigin = origin;
        chunkRadius = radius;

        HashSet<Voxel> suspectedEdges = new HashSet<Voxel>();

        foreach (Voxel v in containedVoxels)
        {
            if (Vector3.Distance(v.worldCentreOfObject, chunkOrigin) > radius * 0.95)
            {
                suspectedEdges.Add(v);
            }

            v.gameObject.transform.parent = gameObject.transform;
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
                StartCoroutine(createPillarIncrementally(v));
                //createPillar(v);
                edgeCount++;
            }
        }

        separateChunk();
       // Debug.Log("suspected  " + suspectedEdges.Count + "/" + containedVoxels.Count + " voxels of being on edge | actually " + edgeCount + " edges  |  radius: " + radius);
    }

    private void createPillar(Voxel v)
    {
        for (int i = 1; i < MapManager.mapLayers; i++)
        {
            v.createNewVoxel(i - v.layer);
            if (!MapManager.manager.isDeleted(i, v.columnID))
            {
                Voxel vox = MapManager.manager.voxels[i][v.columnID];
                if (vox != null)
                {
                    //
                    addVoxel(vox);
                    vox.gameObject.transform.parent = gameObject.transform;
                    //Debug.Log("adding new column voxel to map chunk - parent name: " + vox.gameObject.transform.parent.gameObject.name);
                    vox.showNeighbours(false);
                    MapManager.manager.CmdInformDeleted(vox.layer, vox.columnID);
                    checkNeighbours(vox);
                }
            }
        }
    }
    int batchCount = 0;

    IEnumerator createPillarIncrementally(Voxel v) {
        int batchSize = 1;
        int skipFrames = 25;
        int framesLeft = UnityEngine.Random.Range(0,skipFrames);//offsets the different pillars


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
                if (vox != null)
                {
                    //
                    addVoxel(vox);
                    vox.gameObject.transform.parent = gameObject.transform;
                    //Debug.Log("adding new column voxel to map chunk - parent name: " + vox.gameObject.transform.parent.gameObject.name);
                    vox.showNeighbours(false);
                    MapManager.manager.CmdInformDeleted(vox.layer, vox.columnID);
                    checkNeighbours(vox);

                    if (batchCount >= batchSize)
                    {
                        batchCount = 0;
                        yield return new WaitForFixedUpdate();
                        //yield return new WaitForEndOfFrame();
                    }
                    else {
                        batchCount++;
                    }
                }
            }
        }
    }

    private void checkNeighbours(Voxel vox)
    {
        foreach (int n in MapManager.manager.neighboursMap[vox.columnID])
        {
            if (!MapManager.manager.isDeleted(vox.layer, n))
            {
                if (vox != null)
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