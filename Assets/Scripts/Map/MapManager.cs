﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;
using UnityEditor;
using System.IO;
using Prototype.NetworkLobby;

public class MapManager : NetworkBehaviour
{
    [SyncVar] public bool doneDigging;

    public static bool useSmoothingInGame = false;
    public static bool useSmoothingInGen = true;

    public static bool use1factorSmoothing = true;
    public static bool use2factorSmoothing = true;
    public static bool use3factorSmoothing = false;

    [SyncVar] public int shatters = 2; //make zero to turn off shattering
    public static bool useHills = true;
    public static int noSurfaceCaves = 5;
    public static bool useMapAssets = true;
    public static bool usePortals = false;

    bool loaded = false;

    public static int mapLayers = 15;
    public static int mapSize = 300;
    public static int splits = 1;

    public bool mapDoneLocally;


    public HashSet<int> spawnedVoxels;
    public Dictionary<int, Dictionary<int, Voxel>> voxels; // indexed like voxels[layer][column]



    public Dictionary<int, Vector3> voxelPositions; // the object centers of the layer=0 voxels

    // maps column id's onto a set of all column ids that that column is adjacent to
    public Dictionary<int, HashSet<int>> neighboursMap;
    public HashSet<Portal> portals;

    public static Voxel DeletedVoxel;

    public GameObject Map;
    public static MapManager manager;
    public static GameObject localPlayer;

    public HashSet<PickUpItem> collectables;

    public GameObject shreddingShell;


    /// <summary>
    /// Sets up mapmanager datastructures
    /// </summary>
    public void start()
    {
        doneDigging = noSurfaceCaves == 0;
        mapDoneLocally = false;
        manager = this;

        if (Map == null) return;

        voxels = new Dictionary<int, Dictionary<int, Voxel>>();
        for (int i = 0; i < mapLayers; i++)
        {
            voxels[i] = new Dictionary<int, Voxel>();
        }

        collectables = new HashSet<PickUpItem>();
        DeletedVoxel = new Voxel(); // TODO why?
        spawnedVoxels = new HashSet<int>();
        voxelPositions = new Dictionary<int, Vector3>();
        neighboursMap = new Dictionary<int, HashSet<int>>();
        loadNeighboursMap();
        portals = new HashSet<Portal>();


    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.T))
        {
            CmdShredMap();
        }
    }

    internal void replaceSubVoxel(Voxel spawnedVox)
    {
        Voxel v = voxels[spawnedVox.layer][spawnedVox.columnID]; //v should be a voxel container for this to be a valid call to destroy subvoxel
        //Debug.Log("found top level container: " + v + " of type: " + v.GetType());
        int shatterLevel = spawnedVox.subVoxelID.Split(',').Length - 1;

        VoxelContainer vc = null;
        for (int i = 1; i <= shatterLevel; i++)
        {
            vc = v.gameObject.GetComponent<VoxelContainer>();
            //Debug.Log("opening container " + vc + " - " + vc.subVoxelID);
            v = (Voxel)vc.subVoxels[int.Parse(spawnedVox.subVoxelID.Split(',')[i])];
            //Debug.Log("opening contained subVoxel " + v + " - " + v.subVoxelID);
        }

        vc.subVoxels[int.Parse(spawnedVox.subVoxelID.Split(',')[shatterLevel])] = spawnedVox;
    }

    internal void voxelSpawned(int columnID)
    {
        spawnedVoxels.Add(columnID);
        if (!loaded && spawnedVoxels.Count == 768 * Math.Pow(2, splits))
        {
            //Debug.Log("all voxels spawned and stored in mapManager");
            loaded = true;
            if (NetworkMapGen.mapGen == null)
            {
                Debug.LogError("Null network map gen reference");
                NetworkMapGen.mapGen = GameObject.Find("NetMapGen").GetComponent<NetworkMapGen>();
            }

            // In the event that a voxel from each column has been spawned - but there are extra voxels undrground
            // which have not yet been spawned - the system waits an additional realtime second for the remaining
            // voxels to arrive
            //StartCoroutine(NetworkMapGen.mapGen.InitVoxels());
        }
    }




    public IEnumerator allSurfaceVoxelsLoadedServerSide()
    {
        if (!isServer)
        {
            Debug.LogError("trying to do server map opperations on client side");
        }
        yield return new WaitForSeconds(1.4f);


        foreach (Voxel vox in voxels[0].Values)
        {
            //vox.gameObject.transform.localScale *= MapManager.mapSize;
            vox.checkNeighbourCount();
            //Debug.Log("adding position for voxel 0, " + vox.columnID);
            try
            {
                voxelPositions.Add(vox.columnID, vox.centreOfObject);
            }
            catch (ArgumentException a)
            {
                //Debug.LogError(a);
                Debug.LogError("already added this voxel to voxel positions? colID = " + vox.columnID + " old center: " + voxelPositions[vox.columnID] + " new center: " + vox.centreOfObject);
            }

            // This was uneccasarry and was causing errors with new lobby system (names are still trivoxel
            // vox.gameObject.name = "TriVoxel";
        }


        if (noSurfaceCaves > 0)
        {
            //Debug.Log("digging caves");
            CaveManager.digTierZeroCaves();
        }
        else
        {
            if (useHills)
            {
                //Debug.Log("creating hills");
                deviateHeights();
            }
            else
            {
                //Debug.Log("finishing map - no caves no hills");
                finishMapLocally();
            }
        }
    }



    public IEnumerator allVoxelsLoadedClientSide()
    {
        if (isServer)
        {
            Debug.LogError("trying to do client map opperations on server side");
        }
        yield return new WaitForSeconds(4f);


        foreach (Voxel vox in voxels[0].Values)
        {
            vox.checkNeighbourCount();
            //Debug.Log("adding position for voxel 0, " + vox.columnID);
            try
            {
                voxelPositions.Add(vox.columnID, vox.centreOfObject);
            }
            catch (ArgumentException a)
            {
                Debug.LogError(a);
                Debug.Log("already added this voxel to voxel positions? colID = " + vox.columnID + " old center: " + voxelPositions[vox.columnID] + " new center: " + vox.centreOfObject);
            }
        }


        if (useHills)
        {
            //Debug.Log("creating hills");
            deviateHeights();
        }
        else
        {
            //Debug.Log("finishing map - no caves no hills");
            finishMapLocally();
        }

        CaveManager.manager.gatherCaveVoxels();

    }

    public void finishMapLocally()
    {
        mapDoneLocally = true;
        SmoothVoxels();
        CaveManager.manager.placeCavePortalsArtefacts();
        //finishAssets();
        //LobbyManager.s_Singleton.playerPrefab.transform.position = new Vector3(0, -30, 0);
        //        NetworkManager.singleton.playerPrefab.transform.position = new Vector3(0, -30, 0);
        GameEventManager.singleton.passMessage("waitForMapCompletion", "mapCompleted");
        //localPlayer.transform.position = new Vector3(0, -30, 0);
        GetComponent<MapAssetManager>().genAssets();
        BuildLog.writeLog("Map finished");
        //Debug.Log("Map finished locally on (server = " + isServer + ")");
    }


    private void loadNeighboursMap()
    {
        BuildLog.writeLog("loading neighbours map \n");


        bool found = false;
        string readText = "";
        try
        {
            readText = File.ReadAllText("Assets/Resources/Voxels/split" + splits + ".neiMap");
            if (readText != null && readText.Length > 0)
            {
                found = true;
                BuildLog.writeLog("found map in usual dir");
            }
        }
        catch
        {
        }

        if (!found)
        {
            BuildLog.writeLog("did not find neighbours map in file dir - trying to move back in directory");
            try
            {
                readText = File.ReadAllText("../Assets/Resources/Voxels/split" + splits + ".neiMap");
                if (readText != null && readText.Length > 0)
                {
                    BuildLog.writeLog("found map by going back in dir");
                    found = true;
                }
            }
            catch
            {
            }
        }

        if (!found)
        {
            BuildLog.writeLog("did not find neighbours map in prev dir - trying build folder");
            try
            {
                readText = File.ReadAllText("split" + splits + ".neiMap");
                if (readText != null && readText.Length > 0)
                {
                    BuildLog.writeLog("found map in build folder");
                    found = true;
                }
            }
            catch
            {
            }
        }


        if (found)
        {
            BuildLog.writeLog("reading neighbours map");
            string[] vs = readText.Split('|');
            for (int i = 0; i < vs.Length; i++)
            {
                neighboursMap[i] = new HashSet<int>();
                string[] ns = vs[i].Split(',');
                foreach (String n in ns)
                {
                    neighboursMap[i].Add(int.Parse(n));
                }
            }

            BuildLog.writeLog("loaded neighbours map");
        }
        else
        {
            //gen and save
            foreach (Voxel vox in voxels[0].Values)
            {
                neighboursMap[vox.columnID] = new HashSet<int>();
                vox.gatherAdjacentNeighbours();
            }

            String map = "";
            for (int v = 0; v < voxels[0].Count; v++)
            {
                foreach (int n in neighboursMap[v])
                {
                    map += n + ",";
                }

                map = map.Substring(0, map.Length - 1) + "|";
            }

            map = map.Substring(0, map.Length - 1);

            BuildLog.writeLog("generating neighbours map");

            File.WriteAllText("Assets/Resources/Voxels/split" + splits + ".neiMap", map);
            Debug.Log("generated and saved neighbours map to file");
        }
    }

    // Why is this not at the top dude!?
    float[] frequencies;
    float[] offsets;
    float[] amplitudes;

    static int waveNo = 4;
    static float averageAmp = 0.006f; //0.006
    static float avWaveLength = 10f;

    public void deviateHeights()
    {
        //Debug.Log("creating hills");
        System.Random r = new System.Random(0);


        frequencies = new float[waveNo];
        offsets = new float[waveNo];
        amplitudes = new float[waveNo];

        {
            float ampTot = 0;
            for (int i = 0; i < waveNo; i++)
            {
                frequencies[i] = (float)(r.NextDouble() * avWaveLength + avWaveLength);
                offsets[i] = (float)(r.NextDouble() * avWaveLength + avWaveLength);
                amplitudes[i] = (float)r.NextDouble();
                ampTot += amplitudes[i];
            }

            ampTot /= waveNo;

            for (int i = 0; i < waveNo; i++)
            {
                amplitudes[i] *= averageAmp / ampTot;
            }
        }


        for (int j = 0; j < MapManager.mapLayers; j++)
        {
            foreach (Voxel vox in voxels[j].Values)
            {
                //System.Random rand = vox.rand;

                deviateSingleVoxel(vox);

            }
        }

        //Debug.Log("finisheing map - dug caves - deviated height");
        finishMapLocally();
    }

    public void deviateSingleVoxel(Voxel vox)
    {
        if (vox == null) { return; }

        //Debug.Log("distorting single voxel");

        Vector3[] verts = new Vector3[6];
        MeshFilter filter = vox.gameObject.GetComponent<MeshFilter>();

        float maxHeight = -1;
        float minHeight = float.MaxValue;

        float[] heights = new float[3];

        for (int v = 0; v < 3; v++)
        {
            //Debug.Log("looping through verts");
            Vector3 func = filter.mesh.vertices[v].normalized;

            float height = 0;
            for (int i = 0; i < waveNo; i++)
            {
                //a sinusoidal func of x,y,z
                height += (float)(amplitudes[i] * Math.Sin(frequencies[i] * (func.x - offsets[i])) +
                                   amplitudes[i] *
                                   Math.Sin(frequencies[(i + 1) % waveNo] * (func.y - offsets[(i + 1) % waveNo])) +
                                   amplitudes[i] *
                                   Math.Sin(frequencies[(i + 2) % waveNo] * (func.z - offsets[(i + 2) % waveNo])));
            }

            verts[v] = filter.mesh.vertices[v] + func * height;
            verts[v + 3] = filter.mesh.vertices[v + 3] + func * height;

            heights[v] = height;

            if (height > maxHeight)
            {
                maxHeight = height;
            }
            else {
                if (height < minHeight)
                {
                    minHeight = height;
                }
            }
            
        }
        //Debug.Log("recaulculating voxel stuff");
        if (vox.mainAsset != null)
        {
            Vector3[] facePoints = vox.getMainFaceAtLayer(vox.mainAsset.voxSide);
            Vector3 oldPos = (facePoints[0] + facePoints[1] + facePoints[2]) / 3f;
        }

        //needs to work with actual points
        Vector3 heightNormal = Vector3.Cross(new Vector3(0, heights[0], 0) - new Vector3(0, heights[1], 0), new Vector3(0, heights[1], 0) - new Vector3(0, heights[2], 0));
        if (Vector3.Dot(heightNormal,Vector3.up) < 0) {
            heightNormal *= -1;
        }
        float grad = 90f - (float)Math.Acos(Vector3.Dot(vox.worldCentreOfObject.normalized, heightNormal.normalized));
        //Debug.Log("vox has grad of: " + grad);

        filter.mesh.vertices = verts;
        vox.updateCollider();
        vox.recalcCenters();
        filter.mesh.RecalculateNormals();
        filter.mesh.RecalculateBounds();
        filter.mesh.RecalculateTangents();
        vox.delegateTexture();
        vox.maxGradient = (maxHeight - minHeight);
        vox.resetScale();
        //Debug.Log("vox with max grad = " + vox.maxGradient);

        if (vox.mainAsset != null)
        {
            //Debug.Log("deformed vox with main asset");
            Vector3[]  facePoints = vox.getMainFaceAtLayer(vox.mainAsset.voxSide);
            Vector3 pos = (facePoints[0] + facePoints[1] + facePoints[2]) / 3f;
            vox.mainAsset.CmdMoveTo(pos * (float)(Math.Pow(Voxel.scaleRatio, Math.Abs(vox.layer))) * MapManager.mapSize);
            //vox.mainAsset.CmdMoveBy((pos-oldPos)* Math.Abs(maxHeight-minHeight)*10000);
        }
    }

    public static void SmoothVoxels()
    {
        if (useSmoothingInGen)
        {
            //Debug.Log("smoothing voxels locally");
            for (int k = 0; k < 2; k++)
            {
                for (int i = 0; i < mapLayers; i++)
                {
                    ArrayList keys = new ArrayList();
                    foreach (int n in manager.voxels[i].Keys)
                    {
                        keys.Add(n);
                    }

                    for (int j = 0; j < keys.Count; j++)
                    {
                        manager.voxels[i][(int)keys[j]].smoothBlockInPlace();
                    }
                }
            }
        }
    }


    internal bool doesVoxelExist(int layer, int columnID)
    {
        if (layer >= 0 && layer < mapLayers //within correct range
                       && voxels[layer].ContainsKey(columnID) //has a record in the voxels list
                       && voxels[layer][columnID] != null //should never be null
                       && !voxels[layer][columnID].Equals(MapManager.DeletedVoxel)) //has not been deleted
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    [Command]
    internal void CmdInformDeleted(int layer, int columnID) //block at layer columnID
    {
        RpcInformDeleted(layer, columnID);
        //manager.StartCoroutine(informNeighbours(layer, columnID));
    }

    [ClientRpc]
    private void RpcInformDeleted(int layer, int columnID)
    {
        voxels[layer][columnID] = MapManager.DeletedVoxel;

        foreach (int n in neighboursMap[columnID])
        {
            if (doesVoxelExist(layer, n))
            {
                voxels[layer][n].smoothBlockInPlace();
            }
        }

        if (doesVoxelExist(layer + 1, columnID))
        {
            voxels[layer + 1][columnID].smoothBlockInPlace();
        }

        if (doesVoxelExist(layer - 1, columnID))
        {
            voxels[layer - 1][columnID].smoothBlockInPlace();
        }
    }


    IEnumerator informNeighbours(int layer, int columnID)
    {
        yield return new WaitForSeconds(0.15f);
        Vector3 centre = voxels[layer][columnID].centreOfObject;
        voxels[layer][columnID] = MapManager.DeletedVoxel;

        foreach (int n in neighboursMap[columnID])
        {
            if (doesVoxelExist(layer, n))
            {
                voxels[layer][n].smoothBlockInPlace();
            }
        }

        if (doesVoxelExist(layer + 1, columnID))
        {
            voxels[layer + 1][columnID].smoothBlockInPlace();
        }

        if (doesVoxelExist(layer - 1, columnID))
        {
            voxels[layer - 1][columnID].smoothBlockInPlace();
        }
    }


    [ClientRpc]
    public void RpcDestroyNextSubvoxel(int layer, int columnID, String subID)
    {
        //Debug.Log("client rpc destroy next subvoxel");
        FinalDestroyNextSubVoxel(layer, columnID, subID);
    }

    int destroyID = 0; //local

    void FinalDestroyNextSubVoxel(int layer, int columnID, String subID)
    {
        //Debug.Log("finally destroying sub: '" + subID + "' determines shatter level: " + (subID.Split(',').Length - 1));

        Voxel v = getSubVoxelAt(layer, columnID, subID);

        if (v.shatterLevel < MapManager.manager.shatters)
        {
            //can shatter more
            //Debug.Log("destroyed subvoxel being made a new container");
            v.gameObject.AddComponent<VoxelContainer>();
            VoxelContainer vc = v.gameObject.GetComponent<VoxelContainer>();
            vc.start(v);
            StartCoroutine(v.Melt());
        }
        else
        {
            Destroy(v.gameObject);
        }
    }


    Vector3 shredOrigin;
    int shredNo = 0;
    float radius;

    [Command]
    public void CmdShredMap()
    {
        //StartCoroutine(ShredMap());
        ShredMap();
        RpcUpdateShreddingShell(radius *1.9f,shredOrigin);
    }

    void ShredMap()
    {
        //yield return new WaitForSecondsRealtime(0.1f);

        if (radius > mapSize * 2.5)
        {//reached max shredding
            return;
        }

        //Debug.Log("shredding map");
        if (shredNo == 0)
        {
            shredOrigin = new Vector3(0, mapSize * 2, 0);
            radius = mapSize * 1.8f;
        }

        GameObject mapChunk = Instantiate(Resources.Load<GameObject>("Prefabs/Map/MapChunk"));
        MapChunk chunk = mapChunk.GetComponent<MapChunk>();
        //Debug.Log("chunk: " + chunk);
        Vector3 center = Vector3.zero;
        int count = 0;

        for (int i = 0; i < mapLayers; i++)
        {
            foreach (Voxel vox in voxels[i].Values)
            {
                if (!isDeleted(i, vox.columnID))
                {
                    //try { 
                    if (Vector3.Distance(vox.worldCentreOfObject, shredOrigin) < radius)
                    {
                        chunk.addVoxel(vox);
                        count++;
                        center += vox.worldCentreOfObject;
                    }
                    /*
                }
                catch(Exception e)
                {
                    Debug.Log("world cent: " + vox.worldCentreOfObject + " origin: " + shredOrigin);
                    Debug.Log(e.Message + "\n vox: " + vox + " shredNo: " + shredNo );
                }
                */
                }
            }
        }

        mapChunk.transform.position = center / count;
        chunk.finishChunk(shredOrigin, radius);
        shredNo++;
        radius += mapSize * 0.2f;
    }

    [ClientRpc]
    void RpcUpdateShreddingShell(float radius, Vector3 origin) {
        if (shreddingShell == null) {
            shreddingShell = (GameObject)Instantiate<UnityEngine.Object>(Resources.Load("Prefabs/Map/ShreddingShell"));
            shreddingShell.transform.position = origin;
        }
        shreddingShell.transform.localScale = new Vector3(radius, radius, radius);

    }

    public Voxel getSubVoxelAt(int layer, int columnID, String subID)
    {
        // v should be a voxel container for this to be a valid call to destroy subvoxel
        Voxel v = voxels[layer][columnID];
        int shatterLevel = subID.Split(',').Length - 1;

        for (int i = 1; i <= shatterLevel; i++)
        {
            VoxelContainer vc = v.gameObject.GetComponent<VoxelContainer>();
            int id = -1;
            try
            {
                id = int.Parse(subID.Split(',')[i]);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError("trying to get subvoxel at " + subID + " failed at shatterlevel " + i);
            }
            try
            {
                v = (Voxel)vc.subVoxels[id]; // TODO try catch, this isn't working every time
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError("trying to get subvoxel at " + subID + " failed at shatterlevel " + i + " id = " + id + " num subvoxels = " + vc.subVoxels.Count);
            }

        }

        return v;
    }


    // returns true only if the voxel has been created and destroyed|| or the voxel is in layer -1
    internal bool isDeleted(int l, int columnID)
    {
        if (l <= -1) return true;


        if (voxels.ContainsKey(l))
        {
            if (voxels[l].ContainsKey(columnID))
            {
                if (voxels[l][columnID] == null)
                {
                    return true;
                }

                return voxels[l][columnID].Equals(DeletedVoxel);
            }
            else
            {
                return false; //hasnt even been created yet
            }
        }
        else
        {
            //layer doesnt exist
            return true;
        }
    }

    //returns the position of a voxel whether it exists or not
    public Vector3 getPositionOf(int layer, int columnID)
    {
        float scale = (float)Math.Pow(Voxel.scaleRatio, layer);
        //Debug.Log("rec cen: " + voxelPositions[columnID] + " act cen " + voxels[0][columnID].centreOfObject);
        return voxelPositions[columnID] * scale * mapSize;
    }
}