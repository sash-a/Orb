using System;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

public class RegisterPrefabs : MonoBehaviour
{
    void Start()
    {
       registerVoxelPrefabs(MapManager.splits);
    }

    public static void registerVoxelPrefabs(int splits)
    {
        Object[] voxels = Resources.LoadAll("Voxels/Prefabs/Split" + splits, typeof(GameObject));

        foreach (var voxel in voxels)
        {
            int colID = Int32.Parse(voxel.name.Substring(5));
            var voxelGameObj = (GameObject)voxel;
            voxelGameObj.GetComponent<Voxel>().columnID = colID;
            ClientScene.RegisterPrefab(voxelGameObj);
        }
        if (voxels.Length > 0) Debug.Log("registering prefabs");
    }
}