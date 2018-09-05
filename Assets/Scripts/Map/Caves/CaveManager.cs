using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CaveManager : NetworkBehaviour
{
    static int caveTiers = 2;
    static int tiersLeft;

    public static HashSet<Digger> diggers;
    static int shatters;

    public static UnityEngine.Object diggerPrefab;

    public static HashSet<CaveBody> caves;
    public static HashSet<CaveEntrance> entrances;
    public static HashSet<CaveTunnel> tunnels;

    static System.Random rand;

    public static CaveManager manager;

    public HashSet<Voxel> caveWalls;
    public HashSet<Voxel> caveFloors;
    public HashSet<Voxel> caveCeilings;


    // Use this for initialization
    void Start()
    {
        //Debug.Log("trying to start cave manager");
        tiersLeft = caveTiers;
        //Debug.Log("starting cave manager");
        caves = new HashSet<CaveBody>();
        entrances = new HashSet<CaveEntrance>();
        diggers = new HashSet<Digger>();
        tunnels = new HashSet<CaveTunnel>();
        diggerPrefab = Resources.Load("Prefabs/Map/Digger");
        manager = this;

        caveFloors = new HashSet<Voxel>();
        caveCeilings = new HashSet<Voxel>();
        caveWalls = new HashSet<Voxel>();
        //Debug.Log("found digger : " + diggerPrefab);
    }


    public void placeCavePortalsArtefacts()
    {
        Dictionary<Voxel, Voxel> portalCandidates = new Dictionary<Voxel, Voxel>();//first is the floor vox second is the base of the wall vox
        int requiredHeight = 6;//how high the wall infront of the floor vox has to be to be a candiate for a portal
        int requiredDistance = 60;//a portal is not allowed to be further than this from a cave body to prevent in tunnel portals
        foreach (Voxel vox in caveWalls)
        {
            //Debug.Log("vox grad = " + (vox.maxGradient * 1000));
            if (vox.layer > 3 && vox.mainAsset == null )
            foreach (int nei in MapManager.manager.neighboursMap[vox.columnID])
            {
                if (MapManager.manager.doesVoxelExist(vox.layer + 1, nei))
                {
                    Voxel neighbour = MapManager.manager.voxels[vox.layer + 1][nei];
                    if (caveFloors.Contains(neighbour) &&(neighbour.maxGradient * 1000) <= 15 && !neighbour.smoothed && neighbour.mainAsset==null)
                    {
                        //Debug.Log("found cave border");
                        neighbour.isCaveBorder = true;
                        bool valid = true;
                        for (int i = 0; i < requiredHeight; i++)
                        {
                            if (!(MapManager.manager.doesVoxelExist(vox.layer - i, vox.columnID) && caveWalls.Contains(MapManager.manager.voxels[vox.layer - i][vox.columnID]))) {
                                //the voxel i above vox is not a wall
                                valid = false;
                            }
                        }
                        if (valid)
                        {
                            bool closeEnough = false;
                            foreach (CaveBody body in caves)
                            {
                                double dist = Vector3.Distance(body.center, vox.worldCentreOfObject);
                                //Debug.Log("comparing " + body.center + " and  " + vox.worldCentreOfObject + " dist: " + dist);

                                if (dist < requiredDistance)
                                {
                                    closeEnough = true;
                                }
                            }
                            if (valid && closeEnough)
                            {
                                Debug.Log("found portal candidate");
                                placePortal(vox, neighbour);
                                //StartCoroutine(neighbour.setTexture(Resources.Load<Material>("Materials/Earth/LowPolyCaveBorder")));
                            }
                        }
                    }
                }
            }
        }
    }

    private void placePortal(Voxel wall, Voxel Base)
    {
        Vector3 pos = (Base.worldCentreOfObject + wall.worldCentreOfObject)/2.0f;
        Vector3 forwards = (Base.worldCentreOfObject - wall.worldCentreOfObject);
        forwards = Vector3.Cross(-Base.worldCentreOfObject, Vector3.Cross(forwards, -Base.worldCentreOfObject)).normalized;
        if (Vector3.Dot(forwards, (Base.worldCentreOfObject - wall.worldCentreOfObject)) < 0) {
            forwards *= -1;
        }
        pos += forwards * 1.4f;
        pos += (Base.worldCentreOfObject).normalized * 1.6f;

        Quaternion orient = Quaternion.LookRotation(forwards, -Base.worldCentreOfObject);
        //Debug.Log("test print");
        //Debug.Log("forwards " + forwards + " orient = " + orient + " base cent: " + Base.worldCentreOfObject + " wall cent: " + wall.worldCentreOfObject    );


        GameObject portal =  (GameObject)Instantiate(Resources.Load<UnityEngine.Object>("Prefabs/Map/Portal"),pos,orient);
        portal.transform.GetChild(0).GetComponent<Portal>().voxel = Base;
        if (Base.mainAsset != null) {
            NetworkServer.Destroy(Base.mainAsset.gameObject);
            Base.mainAsset = portal.transform.GetChild(0).GetComponent<Portal>();
        }
        NetworkServer.Spawn(portal);
    }

    public void gatherCaveVoxels()
    {
        //Debug.Log("gathering cave voxels locally");
        for (int i = 0; i <MapManager.mapLayers; i++)
        {
            foreach (Voxel vox in MapManager.manager.voxels[i].Values)
            {
                if (vox.columnID == 0) {
                    Debug.Log("found vox at colID=0");
                    //var myKey = MapManager.manager.voxels[i].FirstOrDefault(x => x.Value == vox).Key;
                    //vox.columnID = MapManager.manager.voxels[i].
                }
                if (MapManager.manager.doesVoxelExist(i, vox.columnID))
                {
                    bool wall = true;
                    if (vox.layer != i) {
                        Debug.Log("i= " + i + " vox layer = " + vox.layer);
                        vox.layer = i;
                    }
                    if (MapManager.manager.isDeleted(i - 1, vox.columnID) && vox.layer > 0)
                    {
                        //Debug.Log("found cave floor vox");
                        wall = false;
                        caveFloors.Add(vox);
                        vox.hasEnergy = false;
                        vox.isCaveFloor = true;
                        StartCoroutine(vox.setTexture(Resources.Load<Material>("Materials/Earth/LowPolyCaveGrass")));
                    }
                    if (MapManager.manager.isDeleted(i + 1, vox.columnID) && vox.layer > 0)
                    {
                        //Debug.Log("found cave ceiling vox");                        
                        wall = false;
                        caveCeilings.Add(vox);
                        vox.hasEnergy = false;
                        vox.isCaveCeiling = true;
                        StartCoroutine(vox.setTexture(Resources.Load<Material>("Materials/Earth/LowPolyCaveMoss")));
                    }
                    if (wall && vox.layer > 1)
                    {
                        caveWalls.Add(vox);
                        //StartCoroutine(vox.setTexture(Resources.Load<Material>("Materials/Earth/LowPolyCaveWalls")));
                    }
                }
            }
        }
    }


    internal static GameObject getNewDigger()
    {
        GameObject dig = (GameObject)Instantiate(diggerPrefab, new Vector3(0, 0, 0), Quaternion.LookRotation(new Vector3(0, 0, 1)));
        diggers.Add(dig.GetComponent<Digger>());
        return dig;
    }

    public static void digTierZeroCaves()
    {
        float estimatedEntranceDistance = 0.5f * MapManager.mapSize / (float)Math.Pow(2, MapManager.splits);

        rand = new System.Random();
        //Debug.Log("cave manager digging caves");
        shatters = MapManager.manager.shatters;
        MapManager.manager.shatters = 0;
        for (int i = 0; i < MapManager.noSurfaceCaves; i++)
        {
            CaveEntrance entrance = new CaveEntrance();
            int colID = rand.Next(0, MapManager.manager.voxels[0].Count - 1);
            bool farEnough = false;

            int remainingTries = 100;

            while (!farEnough && remainingTries > 0)
            {
                farEnough = true;
                foreach (CaveEntrance ent in entrances)
                {//checks if proposed position of new cave entrance is too close to any existing cave entrance or an estimated position of a cave body
                    if (Vector3.Distance(MapManager.manager.getPositionOf(0, colID), MapManager.manager.getPositionOf(0, ent.columnID)) < 160 ||
                        Vector3.Distance(MapManager.manager.getPositionOf(0, colID), MapManager.manager.getPositionOf(0, ent.columnID) + ent.direction.normalized * estimatedEntranceDistance * 0.7f) < 130
                        )
                    {
                        farEnough = false;
                        //Debug.Log("found another entrance too close: " + dist);
                    }
                }
                if (farEnough)//the new cave entrance is far enough away from all other cave entrances
                {
                    Vector3 dir = Vector3.zero;

                    int count = 0;
                    foreach (CaveEntrance ent in entrances)
                    {//points the dir of the new cave entrance away from nearby entrances and cave bodies
                        Vector3 estimatedCavePosition = MapManager.manager.getPositionOf(0, ent.columnID) + ent.direction.normalized * estimatedEntranceDistance * 0.7f;
                        float dist = Vector3.Distance(MapManager.manager.getPositionOf(0, colID), estimatedCavePosition);
                        dir += 0.5f * (MapManager.manager.getPositionOf(0, colID) - estimatedCavePosition).normalized / (float)Math.Pow(Vector3.Distance(MapManager.manager.getPositionOf(0, colID), estimatedCavePosition), 3);//the closer the cave body the more repelling effect it has
                        dir += (MapManager.manager.getPositionOf(0, colID) - MapManager.manager.getPositionOf(0, ent.columnID)).normalized / (float)Math.Pow(Vector3.Distance(MapManager.manager.getPositionOf(0, colID), MapManager.manager.getPositionOf(0, ent.columnID)), 3);//the closer the cave body the more repelling effect it has

                        count++;
                    }
                    dir = planariseDir(colID, dir);
                    //dir is now a direction which is pointing away from all other


                    foreach (CaveEntrance ent in entrances)
                    {
                        double dist = Vector3.Distance(MapManager.manager.getPositionOf(0, colID), MapManager.manager.getPositionOf(0, ent.columnID));
                        if (Vector3.Distance(MapManager.manager.getPositionOf(0, ent.columnID), MapManager.manager.getPositionOf(0, colID) + dir.normalized * estimatedEntranceDistance) < 110)
                        {//the projected destination of this cave entrance is too close to another cave entrance
                            farEnough = false;
                        }
                    }

                    //dir = planariseDir(colID, dir);
                    if (farEnough)
                    {
                        if (count > 0)
                        {
                            //Debug.Log("creating cave entrance with direction dervied from surrounding cave bodies");
                            entrance.createEntranceAt(colID, dir);
                        }
                        else
                        {//this is the first cave entrance
                            entrance.createEntranceAt(colID);
                        }
                    }
                    else
                    {
                        remainingTries--;
                    }
                }
                else
                {
                    colID = rand.Next(0, MapManager.manager.voxels[0].Count - 1);
                    remainingTries--;
                }

            }
            if (remainingTries == 0)
            {
                Debug.LogError("cave manager failed to place a cave entrance");
            }
        }
    }



    public void removeDigger(Digger d)
    {
        diggers.Remove(d);
        Destroy(d.gameObject);
        if (diggers.Count <= 0)
        {
            tiersLeft--;
            gatherCaveVoxels();
            //Debug.Log("finished cave tier " + (caveTiers - tiersLeft - 1));
            if (tiersLeft <= 0)
            {
                finishDigging();
            }
            else {//keep digging
                digNextTier();
            }
            //MapManager.SmoothVoxels();

        }
    }

    private static void digNextTier()
    {
        foreach (CaveBody body in caves) {
            if (body.tier == caveTiers - tiersLeft - 1) {//if cave body is from tier above
                int num = UnityEngine.Random.Range(1, 3);
                //int num = 2;
                CaveTunnel tunnel = null;
                for (int i = 0; i < num; i++)
                {
                    if (i == 1)
                    {
                        CaveTunnel tunnel2 = new CaveTunnel();
                        tunnel2.tunnelDepth = 9;
                        tunnel2.tunnelFrom(body,-tunnel.direction);
                    }
                    else {
                        tunnel = new CaveTunnel();
                        tunnel.tunnelFrom(body);
                    }

                }
            }
        }
    }

    private static void finishDigging()
    {
        MapManager.manager.doneDigging = true;
        if (MapManager.useHills)
        {
            MapManager.manager.deviateHeights();
        }
        else
        {
            Debug.Log("finisheing map - dug caves - not making hills");
            MapManager.manager.finishMapLocally();
        }
        manager.StartCoroutine(manager.RestoreShatters());
    }

    IEnumerator RestoreShatters()
    {
        yield return new WaitForSecondsRealtime(1);
        MapManager.manager.shatters = shatters;

    }

    // Update is called once per frame
    void Update()
    {

    }

    public static Vector3 planariseDir(int colID, Vector3 dir)
    {
        Vector3 cross = Vector3.Cross(dir, MapManager.manager.getPositionOf(0, colID)).normalized;
        Vector3 newDir = Vector3.Cross(cross, MapManager.manager.getPositionOf(0, colID)).normalized;
        if (Vector3.Dot(newDir, dir) < 0)
        {//new dir facing wrong way
            newDir = newDir * -1;
        }
        //Debug.Log("planarised dir from " + dir.normalized + " to " + newDir);
        return newDir;
    }
}
