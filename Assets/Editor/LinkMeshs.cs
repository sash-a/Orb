using System;
using System.Collections.Generic;
using UnityEngine;

public class LinkMeshs
{
    static Dictionary<int, Mesh> meshDict = new Dictionary<int, Mesh>();

    [UnityEditor.MenuItem("Voxel/Link mesh's/Split 0")]
    public static void splitZero()
    {
        MapGen.splits = 0;
        linkMeshs();
    }
    
    [UnityEditor.MenuItem("Voxel/Link mesh's/Split 1")]
    public static void splitOne()
    {
        MapGen.splits = 1;
        linkMeshs();
    }
    
    [UnityEditor.MenuItem("Voxel/Link mesh's/Split 2")]
    public static void splitTwo()
    {
        MapGen.splits = 2;
        linkMeshs();
    }
    
    [UnityEditor.MenuItem("Voxel/Link mesh's/Split 3")]
    public static void splitThree()
    {
        MapGen.splits = 3;
        linkMeshs();
    }
    
    [UnityEditor.MenuItem("Voxel/Link mesh's/Split 4")]
    public static void splitFour()
    {
        MapGen.splits = 4;
        linkMeshs();
    }

    public static void linkMeshs()
    {
        UnityEngine.Object[] voxels = Resources.LoadAll("Voxels/Prefabs/Split" + MapGen.splits, typeof(GameObject));
        UnityEngine.Object[] meshs = Resources.LoadAll("Voxels/Meshs/Split" + MapGen.splits, typeof(Mesh));

        if (voxels.Length == 0)
        {
            Debug.LogError("failed to load voxels");
        }

        foreach (var mesh in meshs)
        {
            meshDict.Add(Int32.Parse(mesh.name.Substring(4)), (Mesh) mesh);
        }

        foreach (var voxel in voxels)
        {
            int colID = Int32.Parse(voxel.name.Substring(5));
            var voxelGameObj = ((GameObject) voxel);
            voxelGameObj.GetComponent<Voxel>().columnID = colID;

            // Add mesh 
            voxelGameObj.GetComponent<MeshFilter>().mesh = meshDict[colID];
            voxelGameObj.GetComponent<MeshCollider>().sharedMesh = meshDict[colID];
        }
    }
}