using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//Used to orientate a gameObject with a rigidBody correctly in the circle
//Colab is a bitch
[RequireComponent( typeof(NetworkIdentity))]
[RequireComponent(typeof(Rigidbody))]


public class MapAsset : NetworkBehaviour
{
    public enum Type { MAIN, SECONDARY, ALTAR, CRITTERSPANWER, RESPAWNPORTAL };
    [SyncVar] Type type;


    static float heightVariation = 0.5f;
    static float widthVariation = 0.3f;
    static float rotateVariation = 5f;

    Rigidbody rb;
    GameObject asset;
    public Voxel voxel;
    [SyncVar] int layer;
    [SyncVar] int colID;
    [SyncVar] public int voxSide;//which side of the voxel this assset is attached to


    bool falling = false;
    bool ready = false;
    public bool united;

    public int seedVariable = 0;


    void Start()
    {


        united = !(type.Equals(Type.MAIN) || type.Equals(Type.ALTAR) || type.Equals(Type.RESPAWNPORTAL));
        if (type.Equals(Type.MAIN) || type.Equals(Type.ALTAR) || type.Equals(Type.RESPAWNPORTAL))
        {
            //Debug.Log("setting asset to voxel");
            if (!MapManager.manager.voxels[layer].ContainsKey(colID))
            {
                Debug.LogError("no voxel at: " + layer + " , " + colID + " cannot assign voxels main asset ");
            }
            else
            {
                MapManager.manager.voxels[layer][colID].mainAsset = this;
            }
            united = isServer;//server side assets are united by default
        }

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        Vector3 forward = getFoward(-transform.position);

        try
        {
            //if (!transformed) {
            if (!forward.Equals(Vector3.zero) && !(-transform.position.normalized).Equals(Vector3.zero))
            {
                rb.MoveRotation(Quaternion.LookRotation(forward, -transform.position.normalized));//stand up straight
            }
            //}
            rb.isKinematic = true;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
        //Debug.Log("starting map asset at: " + transform.position);
        gameObject.tag = "MapAsset";
        if (isServer)//if client - the asset uniter will indicate when it is time to prepare this asset
        {
            StartCoroutine(waitNSet());
        }
    }

    public static MapAsset createAsset(Voxel vox, int side, Type tp)
    {
        //Debug.Log("creating map asset at: " + vox.worldCentreOfObject);
        GameObject ass = null;


        //ass = Instantiate(Resources.Load<GameObject>("Prefabs/Map/MapAssets/Palm_Tree"), vox.worldCentreOfObject, Quaternion.identity);
        if (tp.Equals(Type.MAIN) || tp.Equals(Type.ALTAR) || tp.Equals(Type.CRITTERSPANWER) || tp.Equals(Type.RESPAWNPORTAL))
        {
            ass = spawnMainAsset(vox, side, tp);
        }
        if (tp.Equals(Type.SECONDARY))
        {
            ass = spawnGrass(vox, side);
        }


        ass.GetComponent<MapAsset>().voxel = vox;
        ass.GetComponent<MapAsset>().colID = vox.columnID;
        ass.GetComponent<MapAsset>().layer = vox.layer;
        ass.GetComponent<MapAsset>().voxSide = side;


        if (tp.Equals(Type.MAIN) || tp.Equals(Type.ALTAR) || tp.Equals(Type.RESPAWNPORTAL) || tp.Equals(Type.CRITTERSPANWER))
        {
            NetworkServer.Spawn(ass);
        }




        return ass.GetComponent<MapAsset>();
    }

    private static GameObject spawnGrass(Voxel vox, int side)
    {
        System.Random rand = new System.Random(vox.layer * vox.columnID + vox.columnID);
        GameObject model = null;


        string folder = "";
        if (vox.layer == 0)
        {
            folder = "SecondarySurfaceAssets";
        }
        else
        {
            folder = "SecondaryCaveAssets";
            //Debug.Log("placing cave floor asset");
        }

        UnityEngine.Object[] assets = Resources.LoadAll<GameObject>("Prefabs/Map/MapAssets/" + folder);
        int idx = rand.Next(0, assets.Length);
        GameObject grass = (GameObject)Instantiate(assets[idx], vox.worldCentreOfObject, Quaternion.identity);


        grass.GetComponent<MapAsset>().type = Type.SECONDARY;
        grass.GetComponent<MapAsset>().united = true;
        return grass;
    }

    private static GameObject spawnMainAsset(Voxel vox, int side, Type tp)
    {
       


        System.Random rand = new System.Random(vox.layer * vox.columnID + vox.columnID);
        GameObject model = null;
        GameObject ass = (GameObject)Instantiate(Resources.Load<UnityEngine.Object>("Prefabs/Map/MapAssets/MainAsset"), vox.worldCentreOfObject, Quaternion.identity);
        if (tp.Equals(Type.MAIN))
        {
            string folder = "";
            if (vox.layer == 0)
            {
                folder = "MainSurfaceAssets";
            }
            else
            {
                folder = "MainCaveFloorAssets";
                //Debug.Log("placing cave floor asset");
            }

            UnityEngine.Object[] assets = Resources.LoadAll<GameObject>("Prefabs/Map/MapAssets/" + folder);
            int idx = rand.Next(0, assets.Length);
            model = (GameObject)Instantiate(assets[idx], vox.worldCentreOfObject, Quaternion.identity);
        }
        if (tp.Equals(Type.ALTAR))
        {
            model = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/Map/MapAssets/Altar"), vox.worldCentreOfObject, Quaternion.identity);
            if (tp.Equals(Type.ALTAR))
            {
                Altar a = model.GetComponent<Altar>();
                if (a != null)
                {
                    MapManager.manager.altars.Add(a);
                }
                else
                {
                    Debug.LogError("spawned altar object withoiut altar script");
                }

            }
        }
        if (tp.Equals(Type.CRITTERSPANWER))
        {
            model = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/Map/MapAssets/CritterSpawner"), vox.worldCentreOfObject, Quaternion.identity);
        }
        if (tp.Equals(Type.RESPAWNPORTAL)) {
            model = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/Map/RespawnPortal"), vox.worldCentreOfObject, Quaternion.identity);
            if (tp.Equals(Type.ALTAR))
            {
                Altar a = model.GetComponent<Altar>();
                if (a != null)
                {
                    MapManager.manager.altars.Add(a);
                }
                else
                {
                    Debug.LogError("spawned altar object withoiut altar script");
                }

            }
        }
        ass.GetComponent<MapAsset>().type = tp;
        ass.GetComponent<MapAsset>().united = false;
        ass.GetComponent<MapAsset>().voxSide = side;


        //Debug.Log("loading asset idx " + idx+ "from asset list of length: " + assets.Length);
        model.GetComponent<AssetModelUniter>().layer = vox.layer;
        model.GetComponent<AssetModelUniter>().colID = vox.columnID;
        model.transform.parent = ass.transform;
        NetworkServer.Spawn(model);

        return ass;
    }

    public IEnumerator waitNSet()
    {
        yield return new WaitForSecondsRealtime(3f);
        setParent();

    }

    public void setParent()
    {

        MapManager man = MapManager.manager;
        GameObject map = man.Map;
        //transform.parent = map.transform.GetChild(2);
        if (voxel == null)
        {
            voxel = man.voxels[layer][colID];
        }

        setTransform();

        if (voxel.mainAsset != null && voxel.mainAsset != this && (type.Equals(Type.MAIN) || type.Equals(Type.ALTAR)))
        {
            //Debug.Log("removing duplicate tree");
            NetworkServer.Destroy(gameObject);
            return;
        }
        if (type.Equals(Type.MAIN) || type.Equals(Type.ALTAR))
        {
            voxel.mainAsset = this;
        }
        //changeParent(voxel.gameObject.transform);
        changeParent(MapManager.manager.Map.transform.GetChild(3));

        if (voxel.Equals(MapManager.DeletedVoxel))
        {
            //Debug.Log("removing straggler tree");
            NetworkServer.Destroy(gameObject);
        }

        ready = true;

        if (type.Equals(Type.MAIN))
        {
            //Debug.Log("main asset final pos: " + transform.position );
        }

    }

    bool transformed = false;
    public void setTransform()
    {
        if (!united && (type.Equals(Type.MAIN) || type.Equals(Type.ALTAR)))
        {
            Debug.Log("not united yet - cant transform " + type.ToString());
            return;
        }

        if (transformed)
        {
            Debug.Log("resetting map transform again type: " + type.ToString() + " united: " + united + "id: " + layer + " ; " + colID);
            return;
        }

        if (type.Equals(Type.SECONDARY))
        {
            //Debug.Log("transforming grass");
        }
        else
        {
            //Debug.Log("transforming local non grass asset: " + type.ToString() + " united: " + united + "id: " + layer + " ; " + colID);
        }

        Vector3[] facePoints = voxel.getMainFaceAtLayer(voxSide);
        Vector3 pos = (facePoints[0] + facePoints[1] + facePoints[2]) / 3f;

        transform.position = pos * (float)(Math.Pow(Voxel.scaleRatio, Math.Abs(layer))) * MapManager.mapSize;
        if (type.Equals(Type.MAIN))
        {
            //Debug.Log("moving main asset to: " + transform.position + " children: " + transform.childCount + " united: " + united);
        }

        Vector3 up = Vector3.Cross(facePoints[0] - facePoints[1], facePoints[1] - facePoints[2]).normalized;
        if (Vector3.Dot(-transform.position, up) < 0)
        {
            up *= -1;
        }
        Vector3 forward = getFoward(up);
        Vector3 right = Vector3.Cross(forward, up).normalized;
        System.Random rand = new System.Random(voxel.layer * voxel.columnID + voxel.columnID + seedVariable);
        bool moveAlongFace = (type.Equals(Type.MAIN) && voxel.layer > 0) || type.Equals(Type.SECONDARY);
        if (moveAlongFace)
        {
            //Debug.Log("varying grass pos. pos before:  " + transform.position);
            float mag = 0.055f * MapManager.mapSize / (float)Math.Pow(2, MapManager.splits);
            transform.position += forward * (float)(rand.NextDouble() - 0.5f) * mag + right * (float)(rand.NextDouble() - 0.5f) * mag;
            //Debug.Log("pos after: " + transform.position);
        }

        bool varySize = type.Equals(Type.MAIN) || type.Equals(Type.SECONDARY);
        float size;
        if (varySize)
        {
            size = (float)(rand.NextDouble() * 0.6f + 0.4);//supposed to be height*wdth
        }
        else
        {
            size = 0.6f;
        }

        if (type.Equals(Type.MAIN) && layer == 0)
        {
            transform.position += -transform.localScale.y * transform.position * 0.003f * voxSide;
        }
        if (type.Equals(Type.CRITTERSPANWER))
        {
            transform.position += -transform.localScale.y * transform.position * 0.001f * voxSide;
        }


        float width = 2f * size + (float)(rand.NextDouble() * widthVariation + widthVariation * 0.5f);
        float height = size + (float)(rand.NextDouble() * heightVariation + heightVariation * 0.5f) + 0.2f;
        transform.localScale = new Vector3(
            transform.localScale.x * width,
            transform.localScale.y * height,
            transform.localScale.z * width);
        if (!(type.Equals(Type.ALTAR) || type.Equals(Type.CRITTERSPANWER)))
        {
            transform.Rotate(new Vector3((float)(rand.NextDouble() * rotateVariation + rotateVariation * 0.5f), (float)(rand.NextDouble() * rotateVariation + rotateVariation * 0.5f), (float)(rand.NextDouble() * rotateVariation + rotateVariation * 0.5f)));
        }
        transformed = true;
        //Debug.Log("resized local asset to: " + (new Vector3( transform.localScale.x * width,transform.localScale.y * height,transform.localScale.z * width)));
        if (type.Equals(Type.ALTAR))
        {
            //Debug.Log("altar setting transform");
            transform.GetChild(0).gameObject.GetComponent<Altar>().spawnCollectable();
        }
    }

    public void changeParent(Transform tran)
    {
        Vector3 absPos = transform.position;
        transform.parent = tran;
        transform.position = absPos;
    }

    public Vector3 getFoward(Vector3 up)
    {
        var foward = Vector3.Cross(up, transform.right);

        if (Vector3.Dot(foward, transform.forward) < 0)
        {
            foward *= -1;
        }

        return foward.normalized;
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (!falling && ready)
        {
            //Debug.Log("map asset of type: " + type.ToString() + " collided with: " + collision.gameObject + " tag: " + collision.gameObject.tag);
            if (collision.gameObject.tag.Equals("TriVoxel"))
            {
                voxel = collision.gameObject.GetComponent<Voxel>();
                voxel.mainAsset = this;
                //Debug.Log("reassigning  map asset to new voxel: " + voxel + " ");
                rb.isKinematic = true;
                //transform.parent = collision.gameObject.transform;
            }
        }
    }


    private void Update()
    {
        if ((voxel == null || voxel.isMelted) && type.Equals(Type.SECONDARY))
        {
            //Debug.Log("grass had vox removed - falling: " + falling + " ready: " + ready);
        }

        if ((voxel == null || voxel.isMelted) && !falling && ready)
        {
            if (rb.isKinematic)
            {
                rb.isKinematic = false;
                if (type.Equals(Type.SECONDARY))
                {
                    //Debug.Log("assets voxel has been deleted");
                }
            }
            else
            {
                if (type.Equals(Type.SECONDARY))
                {
                    Destroy(gameObject);
                }
                else
                {
                    CmdAddGravity();
                    rb.AddForce(transform.forward, ForceMode.Acceleration);
                }
            }
        }
    }

    [Command]
    public void CmdAddGravity()
    {
        RpcAddGravity();
        gameObject.AddComponent<Gravity>();
        gameObject.GetComponent<NetworkTransform>().enabled = true;
        rb.AddForce(transform.forward, ForceMode.Acceleration);
        falling = true;
    }

    [ClientRpc]
    private void RpcAddGravity()
    {
        if (gameObject.GetComponent<Gravity>() == null)
        {
            gameObject.AddComponent<Gravity>();
            gameObject.GetComponent<NetworkTransform>().enabled = true;
            falling = true;
            //rb.AddForce(transform.forward, ForceMode.Acceleration);
        }

    }

    [Command]
    internal void CmdMoveBy(Vector3 vector3)
    {
        Debug.Log("cmd moving by : " + vector3);
        RpcMoveBy(vector3);
    }

    [ClientRpc]
    private void RpcMoveBy(Vector3 pos)
    {
        transform.position += pos;
        Debug.Log("moving by : " + pos);
    }

    [Command]
    internal void CmdMoveTo(Vector3 pos)
    {
        RpcMoveTo(pos);
    }

    [ClientRpc]
    private void RpcMoveTo(Vector3 pos)
    {
        transform.position = pos;
    }
}