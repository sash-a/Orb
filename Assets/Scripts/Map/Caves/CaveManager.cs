using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CaveManager : NetworkBehaviour
{
    static int numPortals = 6;
    public static int numAltars = 15;

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
        //Debug.Log("placing cave trinkets");

        Dictionary<Voxel, Voxel> portalCandidates = new Dictionary<Voxel, Voxel>();//first is the floor vox second is the base of the wall vox
        List<Voxel> candidates = new List<Voxel>();

        int requiredHeight = 6;//how high the wall infront of the floor vox has to be to be a candiate for a portal
        int requiredDistance = 60;//a portal is not allowed to be further than this from a cave body to prevent in tunnel portals

        int candidateCount = 0;
        foreach (Voxel wall in caveWalls)
        {
            //Debug.Log("vox grad = " + (vox.maxGradient * 1000));
            if (wall.layer > 3 && wall.mainAsset == null)
                foreach (int nei in MapManager.manager.neighboursMap[wall.columnID])
                {
                    if (MapManager.manager.doesVoxelExist(wall.layer + 1, nei))
                    {
                        Voxel floor = MapManager.manager.voxels[wall.layer + 1][nei];
                        if (caveFloors.Contains(floor) && (floor.maxGradient * 1000) <= 15 && !floor.smoothed && floor.mainAsset == null)
                        {
                            //Debug.Log("found cave border");
                            floor.isCaveBorder = true;
                            bool valid = true;
                            for (int i = 0; i < requiredHeight; i++)
                            {
                                if (!(MapManager.manager.doesVoxelExist(wall.layer - i, wall.columnID) && caveWalls.Contains(MapManager.manager.voxels[wall.layer - i][wall.columnID])))
                                {
                                    //the voxel i above vox is not a wall
                                    valid = false;
                                }
                            }
                            if (valid)
                            {
                                bool closeEnough = false;
                                foreach (CaveBody body in caves)
                                {
                                    double dist = Vector3.Distance(body.center, wall.worldCentreOfObject);
                                    //Debug.Log("comparing " + body.center + " and  " + vox.worldCentreOfObject + " dist: " + dist);

                                    if (dist < requiredDistance)
                                    {
                                        closeEnough = true;
                                    }
                                }
                                if (valid && closeEnough)
                                {
                                    //Debug.Log("found portal candidate");
                                    if (!candidates.Contains(floor))
                                    {
                                        candidateCount++;
                                        candidates.Add(floor);
                                        portalCandidates.Add(floor, wall);
                                    }
                                    //StartCoroutine(neighbour.setTexture(Resources.Load<Material>("Materials/Earth/LowPolyCaveBorder")));
                                }
                            }
                        }
                    }
                }
        }
        //Debug.Log("found " + candidateCount + " portal candidates ");

        int maxTries = candidateCount + 20;

        while (candidates.Count > numPortals && maxTries >0)
        {
            maxTries--;
            candidates = reducePortalCandidates(candidates);
        }
        if (maxTries <= 0) {
            Debug.LogError("failed to reduce portal candidates");
        }

        //Debug.Log("created " + candidates.Count + " portals ");


        foreach (Voxel vox in candidates) {
            placePortal(portalCandidates[vox], vox);
        }
    }

    /// <summary>
    /// finds the closest pair of portals - and removes the one (of the pair) which is closest to any other portal (not in the pair)
    /// </summary>
    /// <param name="candidates"></param>
    /// <returns></returns>
    private List<Voxel> reducePortalCandidates(List<Voxel> candidates)
    {
        double minPairDist = double.MaxValue;
        Voxel[] closestPair = new Voxel[2];

        foreach (Voxel cand1 in candidates)
        {
            foreach (Voxel cand2 in candidates)
            {
                if (cand1 == cand2)
                {
                    continue;
                }
                else
                {
                    double dist = Vector3.Distance(cand1.worldCentreOfObject, cand2.worldCentreOfObject);
                    if (dist < minPairDist)
                    {
                        minPairDist = dist;
                        closestPair = new Voxel[2];
                        closestPair[0] = cand1;
                        closestPair[1] = cand2;
                    }
                }
            }
        }

        double minDist = double.MaxValue;
        int candidateWithMinDist = -1;
        //pair is now the closest pair of portals
        foreach (Voxel cand1 in candidates)
        {
            if (cand1 != closestPair[0] && cand1 != closestPair[1]) {
                double dist1 = Vector3.Distance(cand1.worldCentreOfObject, closestPair[0].worldCentreOfObject);
                double dist2 = Vector3.Distance(cand1.worldCentreOfObject, closestPair[1].worldCentreOfObject);

                double smallDist = Math.Min(dist1, dist2);

                if (smallDist < minDist)
                {
                    minDist = smallDist;
                    if(smallDist == dist1)
                    {
                        candidateWithMinDist = 0;
                    }
                    if (smallDist == dist2)
                    {
                        candidateWithMinDist = 1;
                    }
                }
            }
        }

        if (candidateWithMinDist !=-1) {
            candidates.Remove(closestPair[candidateWithMinDist]);
        }


        return candidates ;
    }

    private void placePortal(Voxel wall, Voxel Base)
    {
        Vector3 pos = (Base.worldCentreOfObject + wall.worldCentreOfObject) / 2.0f;
        Vector3 forwards = (Base.worldCentreOfObject - wall.worldCentreOfObject);
        forwards = Vector3.Cross(-Base.worldCentreOfObject, Vector3.Cross(forwards, -Base.worldCentreOfObject)).normalized;
        if (Vector3.Dot(forwards, (Base.worldCentreOfObject - wall.worldCentreOfObject)) < 0)
        {
            forwards *= -1;
        }
        pos += forwards * 1.4f;
        pos += (Base.worldCentreOfObject).normalized * 1.6f;

        Quaternion orient = Quaternion.LookRotation(forwards, -Base.worldCentreOfObject);
        //Debug.Log("test print");
        //Debug.Log("forwards " + forwards + " orient = " + orient + " base cent: " + Base.worldCentreOfObject + " wall cent: " + wall.worldCentreOfObject    );


        GameObject portal = (GameObject)Instantiate(Resources.Load<UnityEngine.Object>("Prefabs/Map/Portal"), pos, orient);
        portal.transform.GetChild(0).GetComponent<Portal>().voxel = Base;
        if (Base.mainAsset != null)
        {
            NetworkServer.Destroy(Base.mainAsset.gameObject);
        }
        Base.mainAsset = portal.transform.GetChild(0).GetComponent<Portal>();

        NetworkServer.Spawn(portal);
    }

    public void gatherCaveVoxels()
    {
        //Debug.Log("gathering cave voxels locally");
        for (int i = 0; i < MapManager.mapLayers; i++)
        {
            foreach (Voxel vox in MapManager.manager.voxels[i].Values)
            {
                if (vox.isMelted)
                {
                    continue;
                }
                if (vox.columnID == 0)
                {
                    //Debug.Log("found vox at colID=0   layer: " + i);
                    //var myKey = MapManager.manager.voxels[i].FirstOrDefault(x => x.Value == vox).Key;
                    //vox.columnID = MapManager.manager.voxels[i].
                }
                if (MapManager.manager.doesVoxelExist(i, vox.columnID))
                {
                    bool wall = true;
                    if (vox.layer != i)
                    {
                        //Debug.Log("i= " + i + " vox layer = " + vox.layer);
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
        try
        {
            new UIMessage("digging caves 1/2", 3f);
        }
        catch { }

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
                    int dropOffFactor = 100;

                    int count = 0;//number of other cave entrances there are
                    foreach (CaveEntrance ent in entrances)
                    {//points the dir of the new cave entrance away from nearby entrances and cave bodies
                        Vector3 estimatedCavePosition = MapManager.manager.getPositionOf(0, ent.columnID) + ent.direction.normalized * estimatedEntranceDistance * 0.7f;
                        float dist = Vector3.Distance(MapManager.manager.getPositionOf(0, colID), estimatedCavePosition);

                        dir += 0.5f * (MapManager.manager.getPositionOf(0, colID) - estimatedCavePosition).normalized / (float)Math.Pow(Vector3.Distance(MapManager.manager.getPositionOf(0, colID), estimatedCavePosition) / dropOffFactor, 2);//the closer the cave body the more repelling effect it has
                        dir += (MapManager.manager.getPositionOf(0, colID) - MapManager.manager.getPositionOf(0, ent.columnID)).normalized / (float)Math.Pow(Vector3.Distance(MapManager.manager.getPositionOf(0, colID), MapManager.manager.getPositionOf(0, ent.columnID)) / dropOffFactor, 2);//the closer the cave body the more repelling effect it has

                        //Debug.Log("adding " + 0.5f * (MapManager.manager.getPositionOf(0, colID) - estimatedCavePosition).normalized+  " divided by "+ (float)Math.Pow(Vector3.Distance(MapManager.manager.getPositionOf(0, colID), estimatedCavePosition) / dropOffFactor, 2));
                        //Debug.Log("and " + (MapManager.manager.getPositionOf(0, colID) - MapManager.manager.getPositionOf(0, ent.columnID)).normalized + " divided by " + (float)Math.Pow(Vector3.Distance(MapManager.manager.getPositionOf(0, colID), MapManager.manager.getPositionOf(0, ent.columnID)) / dropOffFactor, 2)); 
                        count++;
                    }
                    if (dir.magnitude != 1 && count > 0)
                    {
                        //Debug.LogError("created dir with mag !=1 : " + dir);
                    }
                    dir = planariseDir(colID, dir);
                    if (dir.magnitude != 1 && count > 0)
                    {
                        //Debug.LogError("created planarised dir with mag !=1 : " + dir);
                    }
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
            else
            {//keep digging
                digNextTier();
            }
            //MapManager.SmoothVoxels();

        }
    }

    private static void digNextTier()
    {
        new UIMessage("digging caves 2/2", 3f);


        foreach (CaveBody body in caves)
        {
            if (body.tier == caveTiers - tiersLeft - 1)
            {//if cave body is from tier above
                int num = UnityEngine.Random.Range(1, 3);
                //int num = 2;
                CaveTunnel tunnel = null;
                for (int i = 0; i < num; i++)
                {
                    if (i == 1)
                    {
                        CaveTunnel tunnel2 = new CaveTunnel();
                        tunnel2.tunnelDepth = tunnel.tunnelDepth + 3;
                        tunnel2.tunnelFrom(body, -tunnel.direction);
                    }
                    else
                    {
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
        manager.RpcDoneDigging();
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

    [ClientRpc]
    private void RpcDoneDigging()
    {
        Debug.Log("done digging server = " + isServer);
        BuildLog.writeLog("done digging server = " + isServer);

        MapManager.manager.doneDigging = true;
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
        return newDir.normalized;
    }
}
