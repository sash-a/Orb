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

        int count = 0;
        foreach (var voxel in voxels)
        {   
            var voxelGameObj = (GameObject)voxel;
            voxelGameObj.GetComponent<Voxel>().columnID = count;
            ClientScene.RegisterPrefab(voxelGameObj);
            count++;

        }
        //if (voxels.Length > 0) Debug.Log("registering prefabs");
    }
}