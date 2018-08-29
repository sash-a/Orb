using UnityEngine;
using UnityEngine.Networking;

public class DestructionEffectSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject destructionBlock;
    [SerializeField] private Material grass;
    [SerializeField] private Material ground;

    /// <summary>
    /// Spawns a random number of effect blocks with the texture of the given voxel
    /// </summary>
    /// <param name="pos">Position where the voxels will spawn</param>
    /// <param name="voxel">The voxel that was hit</param>
    /// <param name="minVoxels">Minimum number of voxels to spawn in the effect</param>
    /// <param name="maxVoxels">Minimum number of voxels to spawn in the effect</param>
    public void play(Vector3 pos, Voxel voxel, int minVoxels = 5, int maxVoxels = 12)
    {
        CmdSpawnVoxels(pos, minVoxels, maxVoxels, voxel.layer);
    }


    [Command]
    private void CmdSpawnVoxels(Vector3 pos, int minVoxels, int maxVoxels, int voxelLayer)
    {
        RpcSpawnVoxels(pos, minVoxels, maxVoxels, voxelLayer);
    }

    [ClientRpc]
    private void RpcSpawnVoxels(Vector3 pos, int minVoxels, int maxVoxels, int voxelLayer)
    {
        int numVoxels = Random.Range(minVoxels, maxVoxels);
        
        // Setting the material to the same as the destroyed voxels
        destructionBlock.GetComponent<MeshRenderer>().material = voxelLayer >= 1 ? ground : grass;

        for (int i = 0; i < numVoxels; i++)
        {
            Instantiate(destructionBlock, pos, Quaternion.identity);
        }
    }
}