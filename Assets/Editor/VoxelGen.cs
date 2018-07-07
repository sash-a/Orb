using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;

/**
 * A script intended to be run to generate a new voxel block gameobject
 * this script is meant to execute and then be done with
 * a standard monobehaviour is added to the voxel gameobject - this script deals with further voxel management
 */

public class VoxelGen
{
    Vector3 sphereCentre = new Vector3(0, 0, 0);
    Vector3 v1, v2, v3;

    public GameObject voxel;
    MeshFilter filter;

    public static int[] triangles;
    Vector3[] vertices;

    public Voxel voxelBehaviour;

    // Constructor takes in 3 triangle points which define 
    public VoxelGen(Vector3 v1, Vector3 v2, Vector3 v3, int layer, int colID)
    {
        String voxelPath = "Assets/Resources/Voxels/Prefabs/Split" + MapGen.splits + "/voxel" + colID + ".prefab";
        String meshPath = "Assets/Resources/Voxels/Meshs/Split" + MapGen.splits + "/Mesh" + colID + ".asset";

        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;
        if (!getPrefab(colID)) // prefab was not loaded - create one
        {
            Debug.Log("Did not find voxel saved - generating it");

            voxel = new GameObject();
            voxel.AddComponent<Voxel>();
            // Mesh stuff
            voxel.AddComponent<MeshFilter>();
            voxel.AddComponent<MeshCollider>();
            voxel.AddComponent<MeshRenderer>();

            // Network stuff
            voxel.AddComponent<NetworkIdentity>();
            voxel.AddComponent<NetHealth>().maxHealth = 10;
            voxel.AddComponent<Telekenisis>().enabled = false; // Enabled if telekenisis is used on voxel
            
            // ----------------- Network transform ------------------------
            var netTrans = voxel.gameObject.AddComponent<NetworkTransform>();
            // netTrans.sendInterval = 16; // this its buggy and sets the threshhold to 0.
            netTrans.transformSyncMode = NetworkTransform.TransformSyncMode.SyncTransform;
            netTrans.movementTheshold = 0.001f;
            netTrans.velocityThreshold = 0.0001f;
            netTrans.snapThreshold = 5f;
            netTrans.interpolateMovement = 1;
            netTrans.syncRotationAxis = NetworkTransform.AxisSyncMode.AxisXYZ;
            netTrans.interpolateRotation = 10;
            netTrans.rotationSyncCompression = NetworkTransform.CompressionSyncMode.None;
            netTrans.syncSpin = true;

            netTrans.enabled = false; // Only enable when needed
            // ------------------------------------------------------------

            voxel.tag = "TriVoxel";
            voxel.name = "TriVoxel";

            voxelBehaviour = voxel.GetComponent<Voxel>();
            voxelBehaviour.layer = layer; // Should always be 0

            vertices = getVolumeVertices();
            voxelBehaviour.centreOfObject = getCentrePoint();

            filter = voxel.GetComponent<MeshFilter>();
            filter.mesh = new Mesh();

            filter.mesh.name = "voxelShape";
            filter.mesh.vertices = vertices;
            filter.mesh.triangles = triangles;
            filter.mesh.uv = new[]
            {
                new Vector2(0.3f, 0.4f), new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.6f),
                new Vector2(0.1f, 0.6f),
                new Vector2(0.1f, 0.9f), new Vector2(0.4f, 0.9f)
            };

            filter.mesh.RecalculateNormals();
            voxel.GetComponent<MeshCollider>().sharedMesh = filter.mesh;

            voxelBehaviour.obtusePoint = getObtusePoint();

            // Saving mesh
            //MeshUtility.Optimize(filter.mesh); // will break smoothing
            Mesh tempMesh = UnityEngine.Object.Instantiate(filter.mesh);
            AssetDatabase.CreateAsset(tempMesh, meshPath);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            // Saving prefab
            UnityEngine.Object pref = PrefabUtility.CreateEmptyPrefab(voxelPath);
            PrefabUtility.ReplacePrefab(voxel, pref, ReplacePrefabOptions.ConnectToPrefab);
        }
    }

    private bool getPrefab(int colID)
    {
        String meshPath = "Assets/Resources/Voxels/Prefabs/Split" + MapGen.splits + "/voxel" + colID + ".prefab";
        GameObject vox = AssetDatabase.LoadAssetAtPath<GameObject>(meshPath);

        if (vox == null) return false;

        vox.GetComponent<Voxel>().layer = 0;
        vox.GetComponent<Voxel>().columnID = colID;

        voxel = vox;
        return true;
    }

    private Vector3 getCentrePoint()
    {
        Vector3 av = new Vector3(0, 0, 0);
        for (int i = 0; i < vertices.Length; i++)
        {
            av += vertices[i];
        }

        return av / vertices.Length;
    }

    private Vector3 getExtrudeDir(Vector3 pos)
    {
        return (sphereCentre - pos).normalized;
    }

    private int getObtusePoint()
    {
        Vector3 triangleCentre;

        //triangleCentre = (v1 + v2 + v3) / 3;//method 1, true centre of triangle

        double[] dists = new double[3]; // method 2 and 3
        dists[0] = Vector3.Distance(v1, v2);
        dists[1] = Vector3.Distance(v2, v3);
        dists[2] = Vector3.Distance(v3, v1);
        if (dists[0] < dists[1])
        {
            //0 out
            if (dists[2] < dists[1])
            {
                //2 out 1 in
                triangleCentre = v1; //dist[1] is longest. dist from v2 to v3 is longest - so obtuse point is at v1
                return 0;
            }
            else
            {
                //1 out 2 in
                triangleCentre = v2; //dist[2] is longest so v2 is obtuse angle
                return 1;
            }
        }
        else
        {
            //1 out
            if (dists[2] < dists[0])
            {
                //2 out 0 in
                triangleCentre = v3; //
                return 2;
            }
            else
            {
                //0 out 2 in
                triangleCentre = v2; //
                return 1;
            }
        }
    }


    private Vector3[] getVolumeVertices()
    {
        //Debug.Log(extrudeFrac);
        Vector3 v4 = v1 - (Voxel.extrudeLength * getExtrudeDir(v1));
        Vector3 v5 = v2 - (Voxel.extrudeLength * getExtrudeDir(v2));
        Vector3 v6 = v3 - (Voxel.extrudeLength * getExtrudeDir(v3));
        //voxel.GetComponent<Voxel>().addOuterPoints(v4, v5, v6);
        return new Vector3[] {v1, v2, v3, v4, v5, v6};
    }

    public static void genVolumeTriangles()
    {
        triangles = new int[24];

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
    }
}