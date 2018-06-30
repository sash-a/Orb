using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;
using UnityEditor;
using System.IO;

public class MapManager : NetworkBehaviour
{
    public static bool useSmoothing = false;
    public static bool use1factorSmoothing = true;
    public static bool use2factorSmoothing = true;
    public static bool use3factorSmoothing = true;

    public static int shatters = 0;//make zero to turn off shattering
    public static bool useHills = false;
    public static bool digCaves = false;

    public static int mapLayers = 15;
    public static int mapSize = 200;
    public static int splits = 0;


    public bool mapDoneLocally;
    public HashSet<int> spawnedVoxels;


    public Dictionary<int, Dictionary<int, Voxel>> voxels; // indexed like voxels[layer][column]
    public Dictionary<int, Vector3> voxelPositions; // the object centers of the layer=0 voxels

    public Dictionary<int, HashSet<int>> neighboursMap; //maps column id's onto a set of all column ids that that column is adjacent to

    public static Voxel DeletedVoxel;

    //public Dictionary<int,Column> columns;

    public GameObject Map;
    public static MapManager manager;

    //public static Digger digger;
    public HashSet<Portal> portals;
    public GameObject localPlayer;

    public override void OnStartClient()
    {
        if (!isServer)
        {
            start();
        }
    }
    // Use this for initialization

    public void start()
    {
        spawnedVoxels = new HashSet<int>();
        mapDoneLocally = false;
        manager = this;
        Map = GameObject.Find("Map");

        if (Map == null)
        {
            Debug.LogError("You fucked up shane");
            return;
        }
        Debug.Log("starting map manager");
        //RegisterPrefabs.registerVoxelPrefabs(MapManager.splits);


        DeletedVoxel = new Voxel();
        voxels = new Dictionary<int, Dictionary<int, Voxel>>();
        voxelPositions = new Dictionary<int, Vector3>();
        neighboursMap = new Dictionary<int, HashSet<int>>();
        portals = new HashSet<Portal>();
        for (int i = 0; i < mapLayers; i++)
        {
            voxels[i] = new Dictionary<int, Voxel>();
        }

        // Shane what does this do?
        Map.transform.localScale *= mapSize;
        //deleteIDs = new ArrayList();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {


            Voxel.earthTexNo = -1;
            for (int i = 0; i < voxels.Count; i++)
            {
                foreach (Voxel vox in voxels[i].Values)
                {
                    try
                    {
                        vox.setTexture();
                    }
                    catch { }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.N)) {
            Voxel.texScale  *=1.4f;
            for (int i = 0; i < voxels.Count; i++)
            {
                foreach (Voxel vox in voxels[i].Values)
                {
                    try
                    {
                        vox.setTexture();
                    }
                    catch { }
                }
            }
            Debug.Log("texture scale " + Voxel.texScale);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            Voxel.texScale *= 0.7f;
            for (int i = 0; i < voxels.Count; i++)
            {
                foreach (Voxel vox in voxels[i].Values)
                {
                    try
                    {
                        vox.setTexture();
                    }
                    catch { }
                }
            }
            Debug.Log("texture scale " + Voxel.texScale);

        }
    }

    internal void replaceSubVoxel(Voxel spawnedVox)
    {
        Voxel v = voxels[spawnedVox.layer][spawnedVox.columnID];//v should be a voxel container for this to be a valid call to destroy subvoxel
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

    bool loaded = false;
    internal void voxelSpawned(int columnID)
    {
        spawnedVoxels.Add(columnID);
        if (!loaded && spawnedVoxels.Count == 768 * Math.Pow(2, splits))
        {
            Debug.Log("all voxels spawned and stored in mapManager");
            loaded = true;
            if (NetworkMapGen.mapGen == null)
            {
                Debug.LogError("Null network map gen reference");
                NetworkMapGen.mapGen = GameObject.Find("NetMapGen").GetComponent<NetworkMapGen>();
            }
            StartCoroutine(NetworkMapGen.mapGen.InitVoxels());//in the event that a voxel from each column has been spawned - but there are extra voxels undrground which have not yet been spawned - the system waits an additional realtime second for the remaining voxels to arrive
        }
    }

    public void voxelsLoaded()
    {
        //Debug.Log("voxels loaded called");
        Debug.Log("voxels loaded - found " + spawnedVoxels.Count + " initialised  voxels");
        loadNeighboursMap();

        foreach (Voxel vox in voxels[0].Values)
        {
            vox.gameObject.transform.localScale *= MapManager.mapSize;
            vox.checkNeighbourCount();
            voxelPositions.Add(vox.columnID, vox.centreOfObject);
            vox.gameObject.name = "TriVoxel";
        }



        if (digCaves && isServer)
        {
            CaveManager.digCaves();
        }
        else
        {
            if (useHills)
            {
                deviateHeights();
            }
            else
            {
                finishMap();
            }
        }


    }

    public void finishMap()
    {
        mapDoneLocally = true;
        localPlayer.transform.position = new Vector3(0, -30, 0);
        SmoothVoxels();
    }

    private void loadNeighboursMap()
    {


        bool found = false;
        string readText = "";
        try
        {
            readText = File.ReadAllText("Assets/Resources/Voxels/split" + splits + ".neiMap");
            if (readText != null && readText.Length > 0)
            {
                found = true;
            }
        }
        catch
        {

        }
        if (found)
        {//load
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
            //Debug.Log("read in neighbours map from file");
        }
        else
        {//gen and save
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

            File.WriteAllText("Assets/Resources/Voxels/split" + splits + ".neiMap", map);
            Debug.Log("generated and saved neighbours map to file");

        }

    }

    float[] frequencies;
    float[] offsets;
    float[] amplitudes;

    static int waveNo = 4;
    static float averageAmp = 0.006f;//0.006
    static float avWaveLength = 10f;

    public void deviateHeights()
    {
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
                try
                {
                    deviateSingleVoxel(vox);
                }
                catch
                {
                }
            }
        }

        finishMap();
    }

    public void deviateSingleVoxel(Voxel vox)
    {
        Vector3[] verts = new Vector3[6];
        MeshFilter filter = vox.gameObject.GetComponent<MeshFilter>();

        for (int v = 0; v < 3; v++)
        {
            Vector3 func = filter.mesh.vertices[v].normalized;

            float height = 0;
            for (int i = 0; i < waveNo; i++)
            {
                //a sinusoidal func of x,y,z
                height += (float)(amplitudes[i] * Math.Sin(frequencies[i] * (func.x - offsets[i])) + amplitudes[i] * Math.Sin(frequencies[(i + 1) % waveNo] * (func.y - offsets[(i + 1) % waveNo])) + amplitudes[i] * Math.Sin(frequencies[(i + 2) % waveNo] * (func.z - offsets[(i + 2) % waveNo])));
            }

            verts[v] = filter.mesh.vertices[v] + func * height;
            verts[v + 3] = filter.mesh.vertices[v + 3] + func * height;
        }
        filter.mesh.vertices = verts;
        vox.updateCollider();
        vox.recalcCenters();
        filter.mesh.RecalculateNormals();
        filter.mesh.RecalculateBounds();
        filter.mesh.RecalculateTangents();
        vox.setTexture();
        vox.asset.gameObject.transform.position = vox.worldCentreOfObject;
    }

    public static void SmoothVoxels()
    {
        bool use = useSmoothing;
        useSmoothing = true;
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
        //Debug.Log("smoothed voxels; shatters = " + MapManager.shatters);

        useSmoothing = use;
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

    internal void informDeleted(int layer, int columnID) //block at layer columnID
    {

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
        //manager.StartCoroutine(informNeighbours(layer, columnID));
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

    int destroyID = 0;//local
    void FinalDestroyNextSubVoxel(int layer, int columnID, String subID)
    {

        //Debug.Log("finally destroying sub: '" + subID + "' determines shatter level: " + (subID.Split(',').Length - 1));

        Voxel v = getSubVoxelAt(layer, columnID, subID);

        if (v.shatterLevel < MapManager.shatters)
        {//can shatter more
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

    public Voxel getSubVoxelAt(int layer, int columnID, String subID)
    {
        Voxel v = voxels[layer][columnID];//v should be a voxel container for this to be a valid call to destroy subvoxel
                                          //Debug.Log("found top level container: " + v + " of type: " + v.GetType());
        int shatterLevel = subID.Split(',').Length - 1;

        for (int i = 1; i <= shatterLevel; i++)
        {
            VoxelContainer vc = v.gameObject.GetComponent<VoxelContainer>();
            //Debug.Log("opening container " + vc + " - " + vc.subVoxelID);
            v = (Voxel)vc.subVoxels[int.Parse(subID.Split(',')[i])];
            //Debug.Log("opening contained subVoxel " + v + " - " + v.subVoxelID);
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