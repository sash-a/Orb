using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class ShredManager : MonoBehaviour
{
    public static ShredManager singleton;
    MapManager manager;

    Dictionary<int, Dictionary<int, Voxel>> voxels;

    MapChunk chunk;
    GameObject shreddingShell;
    GameObject warningShell;

    bool isServer;

    Vector3 shredOrigin = new Vector3(0, MapManager.mapSize * 2f, 0);
    int shredNo = 0;//counter to how many shreds have occured
    float nextShredRadius = MapManager.mapSize * 1.6f;//increased after use

    float sizeIncrease = MapManager.mapSize * 0.2f;

    int addBatchSize = 1;


    private void Start()
    {
        singleton = this;
    }

    private void Update()
    {
        if (shredNo == 4 && warningShell != null)
        {
            Destroy(warningShell);
        }
    }


    public void ShredMapNext()//has local player authority
    {
        if (warningShell != null)
        {
            Destroy(warningShell);
        }
        else
        {
            Debug.LogError("no warning shell - but trying to shred");
        }

        //Debug.Log("Shredding map");

        if (nextShredRadius > MapManager.mapSize * 2.5)
        {//reached max shredding
            Debug.Log("trying to shred more than max allowable shredding radius");
            return;
        }

        if (isServer)
        {
            createNextMapChunk();
        }

        shredNo++;
        updateShreddingShell();
        nextShredRadius += sizeIncrease;
    }

    private void createNextMapChunk()
    {
        if (chunk != null)
        {
            chunk.destroyChunk();
        }

        StartCoroutine(createMapChunkIncrementally());

    }

    int batchCount = 0;

    IEnumerator createMapChunkIncrementally()
    {

        //Debug.Log("started creating map chunk : " + shredNo);

        GameObject mapChunk = Instantiate(Resources.Load<GameObject>("Prefabs/Map/MapChunk"));
        chunk = mapChunk.GetComponent<MapChunk>();
        chunk.chunkOrigin = shredOrigin;
        //Debug.Log("chunk: " + chunk);
        Vector3 center = Vector3.zero;
        int count = 0;


        List<Voxel> voxelsInChunk = new List<Voxel>();

        for (int i = 0; i < MapManager.mapLayers; i++)
        {
            foreach (Voxel vox in voxels[i].Values)
            {
                if (!manager.isDeleted(i, vox.columnID))
                {
                    if (Vector3.Distance(vox.worldCentreOfObject, shredOrigin) < nextShredRadius)
                    {
                        if (vox == null)
                        {
                            Debug.LogError("found null voxel in map manager - which hasnt been deleted");
                        }
                        else
                        {
                            count++;
                            center += vox.worldCentreOfObject;
                            voxelsInChunk.Add(vox);
                        }
                    }

                }
            }
        }
        mapChunk.transform.position = center / count;


        for (int i = 0; i < voxelsInChunk.Count; i++)
        {

            if (batchCount >= addBatchSize)
            {
                batchCount = 0;
                yield return new WaitForEndOfFrame();
            }
            else
            {
                batchCount++;
            }

            Voxel vox = voxelsInChunk[i];

            chunk.addVoxel(vox);
            batchCount++;
        }

        chunk.finishChunk(nextShredRadius);

        //Debug.Log("finished creating map chunk : " + (shredNo - 1));

    }

    void updateShreddingShell()
    {
        if (shreddingShell == null)
        {
            shreddingShell = (GameObject)Instantiate<UnityEngine.Object>(Resources.Load("Prefabs/Map/ShreddingShell"));
            shreddingShell.transform.position = shredOrigin;
        }
        shreddingShell.transform.localScale = new Vector3(2.1f * nextShredRadius, 2.1f * nextShredRadius, 2.1f * nextShredRadius);
    }


    public static void createWarningShell()
    {
        if (singleton != null)
        {
            if (singleton.warningShell == null)
            {
                singleton.warningShell = (GameObject)Instantiate<UnityEngine.Object>(Resources.Load("Prefabs/Map/ShreddingWarningShell"));
                singleton.warningShell.transform.localScale = new Vector3(2 * singleton.nextShredRadius, 2 * singleton.nextShredRadius, 2 * singleton.nextShredRadius);
                singleton.warningShell.transform.position = singleton.shredOrigin;
            }
        }
    }


    internal static bool isInWarningZone(Vector3 position)
    {
        if (singleton == null)
        {
            //Debug.Log("singleton is null");
            return false;
        }

        if (singleton.warningShell == null)
        {
            //Debug.Log("no warning shell");
            return false;
        }

        double distance = Vector3.Distance(singleton.shredOrigin, position);
        if (distance < singleton.nextShredRadius)
        {
            return true;
        }
        //Debug.Log("there is a warning shell but you are not in it");


        return false;
    }

    internal void setProperties(MapManager mapManager, bool isServer, Dictionary<int, Dictionary<int, Voxel>> voxs)
    {
        voxels = voxs;
        manager = mapManager;
        this.isServer = isServer;
    }
}
