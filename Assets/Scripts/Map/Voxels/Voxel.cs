using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Voxel : NetworkBehaviour
{
    [SyncVar] public bool isBottom;
    [SyncVar] public String subVoxelID;
    [SyncVar] public int shatterLevel;
    public Ray lastHitRay;
    public Vector3 lastHitPosition;

    public static float extrudeLength = 0.014f; //0.02

    // decrease constant to move blocks further away from eachother//0.0025 - overlaps slightly ; 0.00249 leaves minute gap

    //0.002485 overlaps
    public static float scaleRatio;
    public double scale;

    [SyncVar] public int columnID;
    [SyncVar] public int layer;

    // the point on the top side{0,1,2} which is opposite the longest side of the triangle
    public int obtusePoint;

    [SyncVar] public Vector3 centreOfObject; // centre of the object in object space
    [SyncVar] public Vector3 worldCentreOfObject; // the centre of this object as it is in world space
    public float maxGradient;

    public System.Random rand;
    public MeshFilter filter;


    public Dictionary<int, Vector3> origonalPoints;
    public HashSet<int> deletedPoints;

    public String info = "";

    public MapAsset mainAsset;
    public HashSet<MapAsset> secondaryAssets;//secondary assets are not network spawned - they do not fall over or collide with the player

    public bool hasEnergy;
    public bool isCaveFloor;
    public bool isCaveCeiling;
    public bool isCaveBorder;
    public bool isMelted;
    public bool smoothed;

    private void Start()
    {
        float sepFac = 0.00248f;//0.00248f
        extrudeLength = 0.014f * (float)(Math.Pow(0.75, (MapManager.splits + 1) / 2));
        scaleRatio = (sepFac * MapManager.mapSize + extrudeLength * MapManager.mapSize / 200) / (sepFac * MapManager.mapSize);
        isMelted = false;
        if (centreOfObject.Equals(Vector3.zero))
        {
            recalcCenters();
        }

        if (rand == null)
        {
            rand = new System.Random(layer * columnID + columnID);
        }

        GetComponent<MeshRenderer>().receiveShadows = false;
        GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        gameObject.tag = "TriVoxel";
        MapManager man = MapManager.manager;
        GameObject map = man.Map;
        if (transform.parent == null || !transform.parent.gameObject.name.Contains("MapChunk"))
        {
            transform.parent = map.transform.GetChild(1);
        }

        origonalPoints = new Dictionary<int, Vector3>();
        deletedPoints = new HashSet<int>();
        secondaryAssets = new HashSet<MapAsset>();

        filter = gameObject.GetComponent<MeshFilter>();
        GetComponent<MeshCollider>().convex = false;

        scale = Math.Pow(scaleRatio, Math.Abs(layer));
        worldCentreOfObject = centreOfObject * (float)scale * MapManager.mapSize;
        //Debug.Log("Voxel center obj: " + worldCentreOfObject);
        if ((!MapManager.manager.voxels[layer].ContainsKey(columnID)) ||
            MapManager.manager.voxels[layer][columnID] == null)
        {
            MapManager.manager.voxels[layer][columnID] = this;
        }

        transform.localScale = Vector3.one * (float)scale * MapManager.mapSize;
        setColumnID(columnID);
        gameObject.layer = LayerMask.NameToLayer("Voxels");

        if (shatterLevel < 2)
        {
            GetComponent<NetHealth>().setInitialHealth(0);
        }
        else {
            if (hasEnergy) {
                GetComponent<NetHealth>().setInitialHealth(3);
            }
            else {
                GetComponent<NetHealth>().setInitialHealth(10);
            }
        }


        if (!(gameObject.name.Contains("sub") || gameObject.name.Contains("Sub")))
        {
            if (gameObject.name.Equals("Container"))
            {
            }
            else
            {
                cloneMeshFilter();
                restoreVoxel();
                shatterLevel = 0;
                isBottom = false;
                gameObject.name = "TriVoxel";
                hasEnergy = layer > 3 && rand.NextDouble() < 0.1f;
                if (CaveManager.manager != null && CaveManager.manager.caveCeilings != null)
                {
                    isCaveFloor = CaveManager.manager.caveFloors.Contains(this);
                    //isCaveBorder = CaveManager.manager.cav.Contains(this);
                    isCaveCeiling = CaveManager.manager.caveCeilings.Contains(this);
                }

                delegateTexture();

                MapManager.manager.voxelSpawned(columnID);


                if (!isServer && MapManager.manager.mapDoneLocally && MapManager.useHills)
                {
                    MapManager.manager.deviateSingleVoxel(this);
                }
            }

            //Debug.Log("renaming " + gameObject.name + " to trivoxel");
        }
        else
        {
            //is subvoxel
            delegateTexture();
            gameObject.name = "SubVoxel";
        }
    }

    public void resetScale()
    {
        float sepFac = 0.0025f;// - maxGradient/30;//0.00248f
        extrudeLength = 0.014f * (float)(Math.Pow(0.75, (MapManager.splits + 1) / 2));
        scaleRatio = (sepFac * MapManager.mapSize + extrudeLength * MapManager.mapSize / 200) / (sepFac * MapManager.mapSize);

        scale = Math.Pow(scaleRatio, Math.Abs(layer));
        worldCentreOfObject = centreOfObject * (float)scale * MapManager.mapSize;
        transform.localScale = Vector3.one * (float)scale * MapManager.mapSize;
    }

    internal void genGrass(int side)
    {
        int grassLeft = MapAssetManager.grassDensity;
        scale = Math.Pow(scaleRatio, Math.Abs(layer));
        worldCentreOfObject = centreOfObject * (float)scale * MapManager.mapSize;
        HashSet<Vector3> grassPositions = new HashSet<Vector3>();
        int tries = 4;

        while (grassLeft > 0 && tries > 0)
        {
            MapAsset grass = MapAsset.createAsset(this, side, MapAsset.Type.GRASS);
            grass.seedVariable = (grassLeft + 5) * 30 + tries;
            grass.setParent();
            bool farEnough = true;
            foreach (Vector3 pos in grassPositions)
            {
                if (Vector3.Distance(pos, grass.transform.position) < 0.8f)
                {
                    farEnough = false;
                }
            }
            if (farEnough)
            {
                secondaryAssets.Add(grass);
                grassPositions.Add(grass.gameObject.transform.position);
                grassLeft--;
            }
            else
            {
                tries--;
                Destroy(grass.gameObject);
            }
        }
        if (tries <= 0)
        {
            Debug.LogError("failed to place all grass");
        }
    }

    public void setColumnID(int colID)
    {
        columnID = colID;

        if (!MapManager.manager.voxels[layer].ContainsKey(columnID))
        {
            MapManager.manager.voxels[layer][colID] = this;
        }
    }

    public static int earthTexNo = -1;
    public static float texScale = 4.5f;
    //public bool inChunk


    public void delegateTexture()
    {
        if (hasEnergy)
        {
            StartCoroutine(setTexture(Resources.Load<Material>("Materials/Earth/LowPolyEnergy")));
        }
        else
        {
            if (isCaveFloor || CaveManager.manager.caveFloors.Contains(this))
            {
                isCaveFloor = true;
                if (shatterLevel > 0)
                {
                    //Debug.Log("subvoxel has inherited being a cave floor");
                }
                else {
                    //Debug.Log("voxel is cave floor");
                }
                StartCoroutine(setTexture(Resources.Load<Material>("Materials/Earth/LowPolyCaveGrass")));
            }
            else
            {
                if (isCaveCeiling || CaveManager.manager.caveCeilings.Contains(this))
                {
                    isCaveCeiling = true;
                    //Debug.Log("voxel is cave ceiling");
                    StartCoroutine(setTexture(Resources.Load<Material>("Materials/Earth/LowPolyCaveMoss")));
                }
                else
                {
                    if (layer == 0)
                    {
                        StartCoroutine(setTexture(Resources.Load<Material>("Materials/Earth/LowPolyGrass")));
                    }
                    else if (layer == MapManager.mapLayers - 1)
                    {
                        StartCoroutine(setTexture(Resources.Load<Material>("Materials/Earth/LowPolyCrust")));
                    }
                    else
                    {
                        StartCoroutine(setTexture(Resources.Load<Material>("Materials/Earth/LowPolyGround")));
                    }
                }

            }
        }
    }

    /// <summary>
    /// Adds an asset to this voxel so that when the voxel is destroyed so is the asset
    /// </summary>
    internal void addMainAsset(int side, MapAsset.Type type)
    {
        scale = Math.Pow(scaleRatio, Math.Abs(layer));
        worldCentreOfObject = centreOfObject * (float)scale * MapManager.mapSize;
        if (mainAsset == null)
        {
            mainAsset = MapAsset.createAsset(this, side,type);
        }
    }

    public IEnumerator setTexture(Material material)
    {
        var matt = new Material(material);
        var colour = matt.color;

        if (filter == null)
        {
            filter = GetComponent<MeshFilter>();
        }

        // Setting the colour depending on the steepness of the angle of the voxel


        if (filter == null || filter.mesh.vertices.Length < 6)
        {
            Debug.LogError("error with mesh filter on set texture");
            yield return new WaitForSecondsRealtime(2);
            filter = GetComponent<MeshFilter>();
        }

        Vector3 norm;
        try
        {
             norm = Vector3.Cross(filter.mesh.vertices[0] - filter.mesh.vertices[1], filter.mesh.vertices[2] - filter.mesh.vertices[1]).normalized;
        }
        catch {
            norm = -transform.position.normalized;
        }
        var angle = Vector3.Angle(norm, centreOfObject);
        if (angle > 90) angle = 180 - angle;


        //Debug.Log("found vox with grad = " + angle + " taking colour from: " + colour.r + "," + colour.g + "," + colour.b);


        colour.r += 0.3f * (angle / 90);
        colour.g -= 0.5f * (angle / 90);
        colour.b -= 0.3f * (angle / 90);

        //Debug.Log("to:  " + colour.r + "," + colour.g + "," + colour.b);

        matt.SetColor("_Color", colour); //this feild HAS to be "_Color" otherwise call is ignored
        gameObject.GetComponent<MeshRenderer>().material = matt;

    }

    [Command]
    internal void CmdDestroyVoxel()
    {
        //Debug.Log("destroy voxel called");  
        if (mainAsset != null)
        {
            //asset.changeParent(null);
        }
        if (!isServer)
        {
            Debug.LogError("destroy vox called from in vox");
        }


        if (MapManager.manager.shatters > 0) //using shattering
        {
            if (gameObject.name != "SubVoxel") //not subvoxel - regular voxel
            {
                //Debug.Log("shattering a TriVoxel");
                showNeighbours(false);
                RpcShatterVoxel();
            }
            else
            {
                //is subVoxel
                //Debug.Log("destroying a subVoxel");
                MapManager.manager.RpcDestroyNextSubvoxel(layer, columnID, subVoxelID);
            }


            if (mainAsset != null)
            {
                mainAsset.voxel = null;
                mainAsset = null;
            }
        }
        else
        {
            CmdDestroyVoxelNoShatter();
        }

    }

    public Vector3[] getMainFaceAtLayer(int diff)
    {
        if (diff != -1 && diff != 1)
        {
            Debug.LogError("must pick 1 or -1 (up or down) for finding which main side value given: " + diff);
            return null;
        }

        if (filter == null)
        {
            filter = gameObject.GetComponent<MeshFilter>();
        }

        Vector3[] verts = new Vector3[3];

        HashSet<int> points = new HashSet<int>();
        if (diff == -1)
        {//up
            points.Add(0);
            points.Add(1);
            points.Add(2);
        }
        if (diff == 1)//bottom
        {
            points.Add(3);
            points.Add(4);
            points.Add(5);
        }

        int count = 0;
        foreach (int i in points)
        {
            if (deletedPoints != null && deletedPoints.Contains(i))
            {//add the other side
                int id = (i + 3) % 6;
                verts[count] = filter.mesh.vertices[id];
                //Debug.Log("landed on smoothed vox");
            }
            else
            {
                verts[count] = filter.mesh.vertices[i];
            }
            count++;
        }

        return verts;
    }

    [Command]
    internal void CmdDestroyVoxelNoShatter()
    {
        if (!isServer)
        {
            Debug.LogError("destroy vox called from in vox");
        }


        //Debug.Log("destroying voxel at layer: " + layer + "  no shattering");
        showNeighbours(true);
        if (mainAsset != null)
        {
            //Debug.Log("destroying voxels asset");
            NetworkServer.Destroy(mainAsset.gameObject);
            Destroy(mainAsset.gameObject);
        }

        NetworkServer.Destroy(gameObject);


        if (mainAsset != null)
        {
            mainAsset.voxel = null;
            mainAsset = null;
        }

    }

    [ClientRpc]
    private void RpcShatterVoxel()
    {
        //Debug.Log("shattering voxel: shatters="+ MapManager.shatters);
        gameObject.AddComponent<VoxelContainer>();
        VoxelContainer vc = gameObject.GetComponent<VoxelContainer>();
        vc.start(this);
        MapManager.manager.voxels[layer][columnID] = vc;

        StartCoroutine(Melt());
    }


    public IEnumerator Melt()
    {
        /*
        if (transform.childCount > 0) {
            int count = transform.childCount;
            for (int i = 0; i < count; i++)
            {
                Transform trans = transform.GetChild(0);
                Vector3 absPos = trans.position;
                trans.parent = null;
                trans.position = absPos;
            }
        }
        */
        isMelted = true;
        transform.position = Vector3.one * 1000f; //moves far away
        yield return new WaitForSeconds(1.5f); //waits for subvoxels to generate on all systems
        Destroy(gameObject.GetComponent<MeshCollider>()); //deconstruct this voxel locally

        Destroy(gameObject.GetComponent<MeshRenderer>());
        Destroy(gameObject.GetComponent<MeshFilter>());

        transform.position = Vector3.zero; //moves back
    }

    internal void checkNeighbourCount()
    {
        //Debug.Log("checking neighbour count");
        if (MapManager.manager.neighboursMap[columnID].Count != 3)
        {
            Debug.LogError("column " + columnID + " has " + MapManager.manager.neighboursMap[columnID].Count +
                           " neighbours");
        }
    }


    GameObject lastHit;

    public void gatherAdjacentNeighbours() //finds adjacent neighbours- should be called 1 layer at a time
    {
        if (layer == 0)
        {
            //manually determine all adjacent neighbours neighbours
            //Debug.Log("innermost layer- finding adjacent neighbouors via raycasting");
            gameObject.SetActive(false); //so rayCasting doent collide with this voxel gameobject

            for (int i = 0; i < 3; i++)
            {
                Vector3 dir = centreOfObject - (filter.sharedMesh.vertices[i] + filter.sharedMesh.vertices[i + 3]) / 2;
                RaycastHit hit;
                Ray ray = new Ray(centreOfObject, dir);

                if (Physics.Raycast(ray, out hit) && false)
                {
                    if (lastHit != null && hit.collider.gameObject == lastHit)
                    {
                        //Debug.Log("hit last thing again");
                    }

                    //Debug.Log("hit neighbour");
                    Voxel v = hit.collider.gameObject.GetComponent<Voxel>();
                    if (v != null)
                    {
                        addNeighbour(v);
                    }
                    else
                    {
                        Debug.LogError("no voxel comoponent on neighbour ray hit -  hit " + hit.collider.gameObject.name);
                    }

                    lastHit = hit.collider.gameObject;
                }
                else
                {
                    //Debug.Log("missed normal ray casting");
                    int[] other = findRemainingOnSide(new int[] { i });
                    dir = ((filter.sharedMesh.vertices[other[0]] +
                            filter.sharedMesh.vertices[other[0] + 3] +
                            filter.sharedMesh.vertices[other[1]] +
                            filter.sharedMesh.vertices[other[1] + 3]) / 4) - centreOfObject;
                    ray = new Ray(centreOfObject, dir * 10f);
                    if (Physics.Raycast(ray, out hit))
                    {
                        Voxel v = hit.collider.gameObject.GetComponent<Voxel>();
                        if (v != null)
                        {
                            addNeighbour(v);
                        }
                    }
                }
            }

            gameObject.SetActive(true);
        }

        //        worldCentreOfObject = centreOfObject * MapManager.manager.mapSize;
    }

    public void addNeighbour(Voxel n)
    {
        //Debug.Log("adding neighbour " + n.columnID + " to vox " + columnID);

        if (!MapManager.manager.neighboursMap.ContainsKey(columnID))
        {
            //.Log("recreating element for " + columnID + " in neighbours map-------------------------------------");
            MapManager.manager.neighboursMap.Add(columnID, new HashSet<int>());
        }

        if (!MapManager.manager.neighboursMap.ContainsKey(n.columnID))
        {
            //Debug.Log("recreating element for " + n.columnID + " in neighbours map-------------------------------------");
            MapManager.manager.neighboursMap.Add(n.columnID, new HashSet<int>());
        }

        MapManager.manager.neighboursMap[columnID].Add(n.columnID);
        MapManager.manager.neighboursMap[n.columnID].Add(columnID);
    }

    internal void releaseVoxel()
    {
        if (mainAsset != null)
        {
            NetworkServer.Destroy(mainAsset.gameObject);
        }
        gameObject.GetComponent<MeshCollider>().convex = true;
        if (gameObject.GetComponent<Rigidbody>() == null)
        {
            gameObject.AddComponent<Rigidbody>();
        }
        gameObject.AddComponent<Gravity>();
        showNeighbours(true); //will be destroyed shortly
    }

    public void showNeighbours(bool deleted)
    {
        if (!isServer)
        {
            if (deleted)
            {
                MapManager.manager.CmdInformDeleted(layer, columnID);
            }
            //Debug.Log("show neighbours called on client");
            return;
        }

        /*Voxel down = */
        createNewVoxel(1); // when i am deleted- create block below
        /*Voxel up = */
        createNewVoxel(-1); // when i am deleted- create block above

        foreach (int newColID in MapManager.manager.neighboursMap[columnID])
        {
            //for all of the voxels adjacent to this one

            if (!MapManager.manager.doesVoxelExist(layer, newColID) && //doesnt exist right now
                !(MapManager.manager.voxels[layer].ContainsKey(newColID) &&
                  MapManager.manager.voxels[layer][newColID].Equals(MapManager.DeletedVoxel)))
            {
                // hasn't existed before
                bool created = false;
                if (MapManager.manager.doesVoxelExist(layer - 1, newColID))
                {
                    // my soon to be created neighbour has an existing above voxel- make it create down
                    created = MapManager.manager.voxels[layer - 1][newColID].createNewVoxel(1);
                }

                if (MapManager.manager.doesVoxelExist(layer + 1, newColID))
                {
                    //my soon to be created neighbour has an existing below voxel- make it create up
                    created = MapManager.manager.voxels[layer + 1][newColID].createNewVoxel(-1);
                }

                if (!created)
                {
                    //loop through the entire column and find a voxel that exists and use it to create this voxel
                    for (int i = 0; i < MapManager.mapLayers; i++)
                    {
                        if (MapManager.manager.doesVoxelExist(i, newColID) &&
                            MapManager.manager.voxels[i][newColID].createNewVoxel(layer - i))
                        {
                            break;
                        }
                    }
                }
            }
        }

        if (deleted)
        {
            MapManager.manager.CmdInformDeleted(layer, columnID);
        }
    }


    /*
     * Returns true if voxel created on server false otherwise
     */
    public bool createNewVoxel(int dir) //down is dir=1; up is dir = -1
    {
        if (!isServer)
        {
            return false;
        }
        //Debug.Log("creeating new voxel");
        int newVoxelLayer = layer + dir;
        // Don't create this block if it has existed before and has been deleted or if it exists right now
        if (newVoxelLayer > 0 && newVoxelLayer < MapManager.mapLayers &&
            !MapManager.manager.voxels[newVoxelLayer].ContainsKey(columnID))
        {

            MapAsset ass = null;
            if (mainAsset != null)
            {
                ass = mainAsset;
                mainAsset = null;
                //ass.changeParent(null);
            }
            GameObject childObject = Instantiate(gameObject, gameObject.transform.position,
                gameObject.transform.localRotation);

            Voxel childScript = childObject.GetComponent<Voxel>();
            childScript.layer = newVoxelLayer;
            childScript.origonalPoints = origonalPoints;
            childScript.cloneMeshFilter();
            childScript.mainAsset = null;
            MapManager.manager.voxels[newVoxelLayer][childScript.columnID] = childScript;


            NetworkServer.Spawn(childObject);

            if (ass != null)
            {
                mainAsset = ass;
                //asset.gameObject.transform.parent = gameObject.transform;
                //asset.changeParent(gameObject.transform);
            }
            return true;
        }

        return false;
    }

    public void cloneMeshFilter()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        Mesh meshCopy = Mesh.Instantiate(mf.sharedMesh) as Mesh; //make a deep copy
        GetComponent<MeshFilter>().mesh = meshCopy;
    }

    internal void smoothBlockInPlace()
    {
        //implement order independant simplified smoothing
        if ((!MapManager.useSmoothingInGame &&MapManager.manager.mapDoneLocally) || (!MapManager.useSmoothingInGen && !MapManager.manager.mapDoneLocally))
        {
            return;
        }
        //Debug.Log("smoothing block");
        bool smoothed = false;
        if (MapManager.manager.isDeleted(layer + 1, columnID) || MapManager.manager.isDeleted(layer - 1, columnID))
        {
            try
            {
                int deletedAdjacents = 0;
                ArrayList neighbourIDs = new ArrayList();
                foreach (int nei in MapManager.manager.neighboursMap[columnID])
                {
                    if (MapManager.manager.isDeleted(layer, nei))
                    {
                        deletedAdjacents++;
                    }
                    else
                    {
                        neighbourIDs.Add(nei);
                    }
                }

                info = "del adj=" + deletedAdjacents;
                if (MapManager.manager.isDeleted(layer + 1, columnID) &&
                    MapManager.manager.isDeleted(layer - 1, columnID))
                {
                    //up and down deleted
                    if (deletedAdjacents >= 1)
                    {
                        releaseVoxel();
                    }
                }
                else
                {
                    //exactly 1 vertical neighbour is deleted

                    if (deletedAdjacents == 1 && MapManager.use1factorSmoothing)
                    {
                        Vector3[] verts = filter.mesh.vertices;
                        showNeighbours(false);


                        int dir = 1;
                        int[] closestIDs = new int[] { -1, -1 };
                        for (int c = 0; c < 2; c++)
                        {
                            //down is deleted, suck in 2 points
                            Vector3 smoothDirVec = Vector3.zero;

                            foreach (int n in neighbourIDs)
                            {
                                smoothDirVec -= (MapManager.manager.voxels[layer][n].centreOfObject - centreOfObject)
                                    .normalized;
                            }

                            smoothDirVec.Normalize();
                            //now find the two closest points and shrink them in
                            double[] closestDs = new double[] { double.MaxValue, double.MaxValue };
                            closestIDs = new int[] { -1, -1 };

                            String debug = "";
                            for (int i = 0; i < 6; i++)
                            {
                                double d = (verts[i] - smoothDirVec).magnitude;
                                if (d < closestDs[0])
                                {
                                    closestDs[0] = d;
                                    closestIDs[0] = i;
                                    if (d < closestDs[1])
                                    {
                                        closestDs[0] = closestDs[1];
                                        closestDs[1] = d;

                                        closestIDs[0] = closestIDs[1];
                                        closestIDs[1] = i;
                                    }
                                }

                                debug += "{0:" + closestIDs[0] + " 1:" + closestIDs[1] + "}";
                            }

                            if (closestIDs[0] == closestIDs[1] || closestIDs[0] == (closestIDs[1] + 3 % 6))
                            {
                                //invalid search
                                //closest0 is the closest
                                int[] id = findRemainingOnSide(new int[] { closestIDs[1] });

                                if ((verts[id[0]] - smoothDirVec).magnitude > (verts[id[1]] - smoothDirVec).magnitude)
                                {
                                    //verts 1 closer
                                    closestIDs[0] = id[1];
                                }
                                else
                                {
                                    closestIDs[0] = id[0];
                                }

                                //info += debug;
                            }

                            //dir=+1 -> id e{3,4,5} ; dir=-1 -> id e{0,1,2} 
                            //Math.Sign(2.5 - closestID) == 1  when closest id in {0,1,2}
                            if (Math.Sign(2.5 - closestIDs[0]) == dir)
                            {
                                //needs to flip
                                //Debug.Log("flipping closest id from " + closestID + " to " + ((closestID + 3) % 6));
                                closestIDs[0] = (closestIDs[0] + 3) % 6;
                            }

                            if (Math.Sign(2.5 - closestIDs[1]) == dir)
                            {
                                //needs to flip
                                closestIDs[1] = (closestIDs[1] + 3) % 6;
                            }

                            if (MapManager.manager.isDeleted(layer + dir, columnID))
                            {
                                info += "|1 fac smoothing|";
                                if (Math.Sign(2.5 - closestIDs[0]) == Math.Sign(2.5 - closestIDs[1]))
                                {
                                    //both points on same side of voxel- if not there has been an error
                                    int corner = findRemainingOnSide(new int[] { closestIDs[0], closestIDs[1] })[0];

                                    //receedPoint(closestIDs[0], corner);
                                    //receedPoint(closestIDs[1], corner);

                                    deletePoint(closestIDs[0]);
                                    deletePoint(closestIDs[1]);
                                    smoothNeighbours(closestIDs[0], corner);
                                    smoothNeighbours(closestIDs[1], corner);

                                    updateCollider();
                                    hasEnergy = false;
                                    delegateTexture();
                                }
                                else
                                {
                                    Debug.LogError("error finding shrink points for adj=1");
                                }
                            }

                            dir = -1;
                        }
                    }

                    if (deletedAdjacents == 2 && MapManager.use2factorSmoothing)
                    {
                        //ready to smooth if up or down is deleted
                        Vector3[] verts = gameObject.GetComponent<MeshFilter>().mesh.vertices;
                        int dir = 1;
                        for (int c = 0; c < 2; c++)
                        {
                            //down is deleted, suck in point
                            //Debug.Log("down and 2 adjacent smoothing");

                            Vector3 smoothDirVec = centreOfObject -
                                                   MapManager.manager.voxels[layer][(int)neighbourIDs[0]]
                                                       .centreOfObject;


                            smoothDirVec.Normalize();

                            //smoothDirVec = worldCentreOfObject + smoothDirVec;//is now a point which should be closest to the vert we wish to shrink
                            double closestD = double.MaxValue;
                            int closestID = -1;
                            for (int i = 0; i < 6; i++)
                            {
                                double d = (verts[i] - smoothDirVec).magnitude;
                                if (d < closestD)
                                {
                                    closestD = d;
                                    closestID = i;
                                }
                            }

                            //dir=+1 -> id e{3,4,5} ; dir=-1 -> id e{0,1,2} 
                            //Math.Sign(2.5 - closestID) == 1  when closest id in {0,1,2}
                            if (Math.Sign(2.5 - closestID) == dir)
                            {
                                //needs to flip
                                //Debug.Log("flipping closest id from " + closestID + " to " + ((closestID + 3) % 6));
                                closestID = (closestID + 3) % 6;
                            }

                            //shrink(closestID, 0.3f);
                            if (MapManager.manager.isDeleted(layer + dir, columnID))
                            {
                                deletePoint(closestID);
                                info += "|using 2fac smoothing|";
                                hasEnergy = false;
                                delegateTexture();
                            }


                            dir = -1;
                        }
                    } //smooth 1 point

                    if (deletedAdjacents == 3 && MapManager.use3factorSmoothing)
                    {
                        restoreVoxel();

                        if (MapManager.manager.isDeleted(layer + 1, columnID))
                        {
                            //down is deleted, suck in bottom 3 points
                            shrink(3, 0.5f);
                            shrink(4, 0.5f);
                            shrink(5, 0.5f);
                            info += "|using 3fac smoothing|";
                            hasEnergy = false;
                            delegateTexture();
                        }

                        if (MapManager.manager.isDeleted(layer - 1, columnID))
                        {
                            //up is deleted, suck in top 3 points
                            shrink(0, 0.5f);
                            shrink(1, 0.5f);
                            shrink(2, 0.5f);
                            info += "|using 3fac smoothing|";
                            hasEnergy = false;
                            delegateTexture();
                        }
                    }
                }

                if (filter == null)
                {
                    filter = gameObject.GetComponent<MeshFilter>();
                }

                filter.mesh.RecalculateNormals();
            }
            catch
            {
            }
        }
    }


    private void smoothNeighbours(int pID, int towards)
    {
        Vector3[] verts = filter.mesh.vertices;

        foreach (int neighbourCol in MapManager.manager.neighboursMap[columnID])
        {
            if (MapManager.manager.doesVoxelExist(layer, neighbourCol)) //if neighbour exists rn
            {
                Voxel neighbour = MapManager.manager.voxels[layer][neighbourCol];
                int sameID;
                if (origonalPoints.ContainsKey(pID))
                {
                    sameID = neighbour.findVertIn(origonalPoints[pID]);
                }
                else
                {
                    sameID = neighbour.findVertIn(verts[pID]);
                }
                //int sameID = neighbour.findVertIn(origonalPoints[pID]);

                //if (sameID != -1 && !neighbour.origonalPoints.ContainsKey(sameID))//shared point exists and it hasnt been mutated already
                if (sameID != -1
                ) // && neighbour.deletedAdjacents == 0)//shared point exists and it hasnt been mutated already
                {
                    //found the shared vert in neighbour vox
                    //if ((MapGen.isDeleted(layer + 1, columnID) == MapGen.isDeleted(layer + 1, neighbour.columnID) && pID<=2)
                    //|| (MapGen.isDeleted(layer - 1, columnID) == MapGen.isDeleted(layer - 1, neighbour.columnID) && pID>2))
                    if ((MapManager.manager.isDeleted(layer + 1, columnID) ==
                         MapManager.manager.isDeleted(layer + 1, neighbour.columnID))
                        && (MapManager.manager.isDeleted(layer - 1, columnID) ==
                            MapManager.manager.isDeleted(layer - 1, neighbour.columnID)))
                    {
                        //the neighbour has the same up/down block situation
                        int otherSameId = neighbour.findVertIn(verts[towards]);
                        //MapGen.voxels[layer][neighbourCol].makePlanar(sameID);
                        if (otherSameId != -1)
                        {
                            //found the towards point in neighbour vox

                            neighbour.deletePoint(sameID);
                            neighbour.smoothNeighbours(sameID, findRemainingOnSide(new int[] { otherSameId, sameID })[0]);
                            neighbour.showNeighbours(false);
                            //MapGen.voxels[layer][neighbourCol].shrink(sameID, 0);
                        }
                        else
                        {
                            //point wants to be smoothed but otherID not found
                            //Debug.Log("tried to smooth point but couldnt find otherSame - its delt adj = " + neighbour.deletedAdjacents + " colID = " + neighbour.columnID);
                            if (neighbour.getDeletedAdjacentCount() == 2)
                            {
                                //Debug.Log("vox had 2 deleted adj - deleting it");
                                neighbour.CmdDestroyVoxel();
                            }
                        }
                    }
                }
            }
        }
    }


    private void deletePoint(int pID)
    {
        //Debug.Log("deleting point " + pID + " from vox " + columnID);
        if (!deletedPoints.Contains(pID))
        {
            //hasnt deleted this point already
            smoothed = true;
            if (mainAsset != null)
            {
                mainAsset.transform.position += (transform.position).normalized;
                //mainAsset.CmdMoveBy((transform.position).normalized);
            }
            if (filter.mesh.triangles.Length == 24)
            {
                //info += "|deleting:" + pID + "|";
                //has not had a point deleted yet
                deletedPoints.Add(pID);
                int horn = (pID + 3) % 6;
                //info += "|horn:" + horn + "|";


                int[] sameCorners = findRemainingOnSide(new int[] { pID });
                //info += "|ss corners: " + sameCorners[0] + "," + sameCorners[1] + "|";

                if (pID == 1 || pID == 4)
                {
                    //split natural order in half
                    int temp = sameCorners[0];
                    sameCorners[0] = sameCorners[1];
                    sameCorners[1] = temp;
                }

                int[] otherCorners = new int[] { (sameCorners[0] + 3) % 6, (sameCorners[1] + 3) % 6 };
                //info += "|os corners: " + otherCorners[0] + "," + otherCorners[1] + "|";


                int[] triangles = new int[6 * 3];


                triangles[0] = horn; //top
                triangles[1] = sameCorners[1]; //top
                triangles[2] = sameCorners[0]; //top

                triangles[3] = horn; //bottom
                triangles[4] = otherCorners[0]; //bottom
                triangles[5] = otherCorners[1]; //bottom

                triangles[6] = horn; //side
                triangles[7] = sameCorners[0]; //side
                triangles[8] = otherCorners[0]; //side

                triangles[9] = horn; //side
                triangles[10] = otherCorners[1]; //side
                triangles[11] = sameCorners[1]; //side

                triangles[12] = sameCorners[0]; //back
                triangles[13] = sameCorners[1]; //back
                triangles[14] = otherCorners[0]; //back

                triangles[15] = sameCorners[1]; //back
                triangles[16] = otherCorners[1]; //back
                triangles[17] = otherCorners[0]; //back

                if (pID > 2)
                {
                    for (int i = 0; i < 18; i += 3)
                    {
                        int temp = triangles[i + 1];
                        triangles[i + 1] = triangles[i + 2];
                        triangles[i + 2] = temp;
                    }
                }

                filter.mesh.triangles = triangles;
            }
            else if (deletedPoints.Count == 1)
            {
                // Debug.Log("doing a second delete vox:" + columnID + "; triangles size:" + filter.mesh.triangles.Length);
                int delt = -1;
                foreach (int d in deletedPoints)
                {
                    delt = d;
                }

                if ((delt <= 2) == (pID <= 2))
                {
                    //points on same side of vox
                    int tip = findRemainingOnSide(new int[] { pID, delt })[0];
                    int underTip = (tip + 3) % 6;
                    int[] rest = findRemainingOnSide(new int[] { underTip });

                    if (underTip == 1 || underTip == 4)
                    {
                        int temp = rest[0];
                        rest[0] = rest[1];
                        rest[1] = temp;
                    }

                    int[] triangles = new int[4 * 3];

                    triangles[0] = tip;
                    triangles[1] = rest[1];
                    triangles[2] = rest[0];

                    triangles[3] = tip;
                    triangles[4] = rest[0];
                    triangles[5] = underTip;

                    triangles[6] = underTip;
                    triangles[7] = rest[0];
                    triangles[8] = rest[1];

                    triangles[9] = tip;
                    triangles[10] = underTip;
                    triangles[11] = rest[1];

                    if (pID > 2)
                    {
                        for (int i = 0; i < 12; i += 3)
                        {
                            int temp = triangles[i + 1];
                            triangles[i + 1] = triangles[i + 2];
                            triangles[i + 2] = temp;
                        }
                    }

                    filter.mesh.triangles = triangles;
                    deletedPoints.Add(pID);
                }
            }
            else if (deletedPoints.Count == 2)
            {
                //Debug.Log("deleting 3rd point- deleting voxel");
                CmdDestroyVoxel();
            }
        }

        updateCollider();
        
    }

    public void updateCollider()
    {
        Mesh m = new Mesh();
        Vector3[] verts = new Vector3[filter.mesh.vertices.Length];
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = filter.mesh.vertices[i] + Vector3.zero;
        }

        m.vertices = verts;

        int[] tris = new int[filter.mesh.triangles.Length];
        for (int i = 0; i < tris.Length; i++)
        {
            tris[i] = filter.mesh.triangles[i] + 0;
        }

        m.triangles = tris;
        gameObject.GetComponent<MeshCollider>().sharedMesh = m;

        //gameObject.GetComponent<MeshCollider>().sharedMesh.RecalculateNormals();
        gameObject.GetComponent<MeshCollider>().convex = !gameObject.GetComponent<MeshCollider>().convex;
        //gameObject.GetComponent<MeshCollider>().sharedMesh.RecalculateNormals();
        gameObject.GetComponent<MeshCollider>().convex = !gameObject.GetComponent<MeshCollider>().convex;
    }

    private void restoreVoxel()
    {
        if (filter == null)
        {
            filter = gameObject.GetComponent<MeshFilter>();
        }

        Vector3[] newVerts = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {
            newVerts[i] = filter.mesh.vertices[i] + Vector3.zero;
        }

        foreach (int id in origonalPoints.Keys)
        {
            //for each morphed point
            //Debug.Log("vox " + columnID + " restoring point from "  + newVerts[id] + );
            newVerts[id] = origonalPoints[id] + Vector3.zero;
        }

        //filter.mesh.vertices = newVerts;
        if (origonalPoints.Keys.Count > 0)
        {
            //there was morphing
            //gameObject.GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().mesh;//return collider to origonal
            origonalPoints = new Dictionary<int, Vector3>();
        }

        filter.mesh.triangles = genVolumeTriangles();


        Mesh m = new Mesh();
        Vector3[] verts = new Vector3[filter.mesh.vertices.Length];
        for (int i = 0; i < verts.Length; i++)
        {
            if (origonalPoints.ContainsKey(i))
            {
                verts[i] = origonalPoints[i];
                Debug.Log("using orig point in col erf");
            }
            else
            {
                verts[i] = filter.mesh.vertices[i] + Vector3.zero;
            }
        }

        m.vertices = verts;
        m.triangles = genVolumeTriangles();
        gameObject.GetComponent<MeshCollider>().sharedMesh = m;

        //gameObject.GetComponent<MeshCollider>().sharedMesh.RecalculateNormals();
        gameObject.GetComponent<MeshCollider>().convex = !gameObject.GetComponent<MeshCollider>().convex;
        //gameObject.GetComponent<MeshCollider>().sharedMesh.RecalculateNormals();
        gameObject.GetComponent<MeshCollider>().convex = !gameObject.GetComponent<MeshCollider>().convex;
    }

    private void shrink(int index, float f)
    {
        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        //if (!smoothedVerts[index])
        if (!origonalPoints.ContainsKey(index)) // dont shrink morphed points
        {
            Vector3 diff = filter.mesh.vertices[index] - centreOfObject;
            diff *= f;
            //Debug.Log("moving vert " + index + " from " + filter.mesh.vertices[index] + " to " + (centreOfObject + diff) + " object centre at: " + centreOfObject);
            Vector3[] newVerts = new Vector3[6];
            for (int i = 0; i < 6; i++)
            {
                if (i == index)
                {
                    newVerts[i] = centreOfObject + diff;
                }
                else
                {
                    newVerts[i] = filter.mesh.vertices[i] + Vector3.zero;
                }
            }

            origonalPoints.Add(index, filter.mesh.vertices[index]);
            filter.mesh.vertices = newVerts;
        }
    }

    public static int[] findRemainingOnSide(int[] points)
    {
        //assuming points on same side of vox- finds the third point on that same side
        string debug = "";
        HashSet<int> set;
        if (Math.Sign(2.5 - points[0]) > 0)
        {
            //point is part of {0,1,2}
            set = new HashSet<int> { 0, 1, 2 };
            debug = "set started 0,1,2 ";
        }
        else
        {
            set = new HashSet<int> { 3, 4, 5 };
            debug = "set started 3,4,5 ";
        }

        debug += "remoing " + points.Length;

        for (int i = 0; i < points.Length; i++)
        {
            debug += " remo" + i + ":" + points[i];
            set.Remove(points[i]);
        }

        int[] remaining = new int[3 - points.Length];

        int count = 0;
        foreach (int s in set)
        {
            try
            {
                debug += " rema:" + s;
                remaining[count] = s;
            }
            catch
            {
                //Debug.LogError(debug);
            }

            count++;
        }

        return remaining;
    }

    private int findVertIn(Vector3 vert)
    {
        if (filter == null)
        {
            filter = GetComponent<MeshFilter>();
        }

        for (int i = 0; i < 6; i++)
        {
            if (Vector3.Distance(filter.mesh.vertices[i], vert) < 0.01 &&
                Vector3.Distance(filter.mesh.vertices[i], vert) > 0)
            {
                Debug.Log("checking if two points be equal. dist = " + Vector3.Distance(filter.mesh.vertices[i], vert));
            }

            if (filter.mesh.vertices[i] == vert)
            //if(Vector3.Distance(filter.mesh.vertices[i], vert)< 0.05)
            {
                return i;
            }
        }

        return -1;
    }

    public void replaceVert(int vertID, Vector3 newVert)
    {
        Vector3[] newVerts = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {
            if (i == vertID)
            {
                //newVerts[i] = filter.mesh.vertices[i] + moveDir;
                newVerts[i] = newVert;
            }
            else
            {
                newVerts[i] = filter.mesh.vertices[i] + Vector3.zero;
            }
        }

        filter.mesh.vertices = newVerts;
    }


    public int[] genVolumeTriangles()
    {
        int[] triangles = new int[24];

        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1; //end of triangle 1 base
        triangles[3] = 3;
        triangles[4] = 4;
        triangles[5] = 5; //end of triangle 2 top

        triangles[6] = 1;
        triangles[7] = 3;
        triangles[8] = 0; //end of triangle 3 
        triangles[9] = 1;
        triangles[10] = 4;
        triangles[11] = 3; //end of triangle 4
        triangles[12] = 2;
        triangles[13] = 4;
        triangles[14] = 1; //end of triangle 5
        triangles[15] = 2;
        triangles[16] = 5;
        triangles[17] = 4; //end of triangle 6
        triangles[18] = 0;
        triangles[19] = 3;
        triangles[20] = 5; //end of triangle 7
        triangles[21] = 0;
        triangles[22] = 5;
        triangles[23] = 2; //end of triangle 8

        return triangles;
    }

    public int getDeletedAdjacentCount()
    {
        int deletedAdjacents = 0;
        foreach (int nei in MapManager.manager.neighboursMap[columnID])
        {
            if (MapManager.manager.isDeleted(layer, nei))
            {
                deletedAdjacents++;
            }
        }

        return deletedAdjacents;
    }

    public void recalcCenters()
    {
        Vector3[] vertices = filter.mesh.vertices;
        Vector3 av = new Vector3(0, 0, 0);
        for (int i = 0; i < vertices.Length; i++)
        {
            av += vertices[i];
        }

        centreOfObject = av / vertices.Length;
        scale = Math.Pow(scaleRatio, Math.Abs(layer));
        worldCentreOfObject = centreOfObject * (float)scale * MapManager.mapSize;
    }

    [Command]
    public void CmdAddNetworkTransform()
    {
        RpcAddNetworkTransform();
    }

    [ClientRpc]
    private void RpcAddNetworkTransform()
    {
        if (gameObject.GetComponent<NetworkTransform>() == null)
        {
            gameObject.AddComponent<NetworkTransform>();
        }
    }
}