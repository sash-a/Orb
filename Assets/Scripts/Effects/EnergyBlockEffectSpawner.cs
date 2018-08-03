using UnityEngine;
using UnityEngine.Networking;

// This should live on each voxel
public class EnergyBlockEffectSpawner : NetworkBehaviour
{
    public GameObject energyBlock;
    private GameObject voxel;
    private Vector3 pos;

    public void setVoxel(GameObject voxel)
    {
        var subVox = voxel.GetComponent<SubVoxel>();
        if (subVox != null)
        {
            pos = subVox.voxelPosition.position;
        }
        else
        {
            pos = voxel.GetComponent<Voxel>().worldCentreOfObject;
        }
    }

    public void spawnBlock()
    {
        int rand = Random.Range(0, 6);
        // Spawns a block 1 in 6 times
        if (rand == 0)
            CmdSpawnBlock();
    }

    [Command]
    void CmdSpawnBlock()
    {
        // Need to check if sub voxel to get propper position
        var blockInst = Instantiate
        (
            energyBlock,
            pos, // get pos if sub vox?
            Quaternion.identity
        );
        
        blockInst.GetComponent<EnergyBlockEffect>().target = transform;
        NetworkServer.Spawn(blockInst);
    }
}