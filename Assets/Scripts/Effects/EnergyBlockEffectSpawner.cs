using UnityEngine;
using UnityEngine.Networking;

// This should live on each voxel
public class EnergyBlockEffectSpawner : NetworkBehaviour
{
    public GameObject energyBlock;
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
        int rand = Random.Range(0, 7);
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
            pos + (Random.insideUnitSphere * 6),
            Quaternion.identity
        );

        blockInst.GetComponent<EnergyBlockEffect>().target = transform;
        NetworkServer.Spawn(blockInst);
    }
}