using UnityEditor;
using UnityEngine;

public class MapGen
{
    public static int splits = 0;
    static int columnID = 0;

    static Mesh mesh;
    static Vector3[] vertices;

    [MenuItem("Voxel/Generation/Split 0")]
    static void splitZero()
    {
        splits = 0;
        columnID = 0;

        generateVoxels();
    }

    [MenuItem("Voxel/Generation/Split 1")]
    static void splitOne()
    {
        splits = 1;
        columnID = 0;

        generateVoxels();
    }

    [MenuItem("Voxel/Generation/Split 2")]
    static void splitTwo()
    {
        splits = 2;
        columnID = 0;

        generateVoxels();
    }

    [MenuItem("Voxel/Generation/Split 3")]
    static void splitThree()
    {
        splits = 3;
        columnID = 0;

        generateVoxels();
    }

    [MenuItem("Voxel/Generation/Split 4")]
    static void splitFour()
    {
        splits = 4;
        columnID = 0;

        generateVoxels();
    }
    
    [MenuItem("Voxel/Generation/Split 2 3 4")]
    static void splitMulti()
    {
        splitFour();
        splitThree();
        splitTwo();
    }

    static void generateVoxels()
    {
        string spherePath = "Assets/Resources/Voxels/Sphere.prefab";
        createSpherePrimitive(spherePath);
        var sphere = AssetDatabase.LoadAssetAtPath<GameObject>(spherePath);

        if (sphere == null)
        {
            Debug.LogError("shpere null either can't find or mesh filter is empty please regenerate in folder:\n" +
                           "Assets/Resources/Voxels/Sphere.prefab");
            return;
        }

        copyMesh(sphere);
        vertices = mesh.vertices;
        VoxelGen.genVolumeTriangles();

        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int[] triangles = mesh.GetTriangles(i);
            Vector3 v1 = new Vector3();
            Vector3 v2 = new Vector3();
            Vector3 v3 = new Vector3();

            for (int j = 0; j < triangles.Length; j++)
            {
                if (j % 3 == 0)
                {
                    v1 = vertices[triangles[j]];
                }

                if (j % 3 == 1)
                {
                    v2 = vertices[triangles[j]];
                }

                if (j % 3 == 2)
                {
                    //finished triangle
                    v3 = vertices[triangles[j]];
                    GenTriangles(v1, v2, v3, splits);
                }
            }
        }

        AssetDatabase.DeleteAsset(spherePath);
        LinkMeshs.linkMeshs();
    }


    private static void GenTriangles(Vector3 v1, Vector3 v2, Vector3 v3, int rec)
    {
        if (rec == 0)
        {
            //Debug.Log("generating innermost voxel");
            VoxelGen v = new VoxelGen(v1, v2, v3, 0, columnID);
            columnID++;
        }
        else
        {
            int longest;
            double[] dists = new double[3];
            dists[0] = Vector3.Distance(v1, v2);
            dists[1] = Vector3.Distance(v2, v3);
            dists[2] = Vector3.Distance(v3, v1);
            Vector3 midPoint;
            if (dists[0] < dists[1])
            {
                //0 out
                if (dists[2] < dists[1])
                {
                    //2 out
                    longest = 1;
                }
                else
                {
                    //1 out
                    longest = 2;
                }
            }
            else
            {
                //1 out
                if (dists[2] < dists[0])
                {
                    //2 out
                    longest = 0;
                }
                else
                {
                    //0 out
                    longest = 2;
                }
            }

            if (longest == 0)
            {
                midPoint = (v1 + v2) / 2;
                GenTriangles(midPoint, v2, v3, rec - 1);
                GenTriangles(midPoint, v3, v1, rec - 1);
            }

            if (longest == 1)
            {
                midPoint = (v2 + v3) / 2;
                GenTriangles(midPoint, v1, v2, rec - 1);
                GenTriangles(midPoint, v3, v1, rec - 1);
            }

            if (longest == 2)
            {
                midPoint = (v3 + v1) / 2;
                GenTriangles(midPoint, v2, v3, rec - 1);
                GenTriangles(midPoint, v1, v2, rec - 1);
            }
        }
    }

    private static void copyMesh(GameObject go)
    {
        MeshFilter mf = go.GetComponent<MeshFilter>();
        Mesh meshCopy = Mesh.Instantiate(mf.sharedMesh) as Mesh; //make a deep copy
        mesh = mf.mesh = meshCopy;
    }

    private static void createSpherePrimitive(string spherePath)
    {
        GameObject sphereGoPrimitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Object pref = PrefabUtility.CreateEmptyPrefab(spherePath);
        PrefabUtility.ReplacePrefab(sphereGoPrimitive, pref, ReplacePrefabOptions.ConnectToPrefab);
    }
}