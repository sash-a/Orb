using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class VoxelDestructionEffect : NetworkBehaviour
{

    public GameObject voxelFragment;

    public void Start()
    {
        Destroy(gameObject, 3);
    }

    public void spawnVoxelFragment(Vector3 position, Material m)
    {

        GameObject VF = Instantiate(voxelFragment, position, transform.rotation);
        VF.GetComponent<Renderer>().material = m;
        GameObject VF1 = Instantiate(voxelFragment, position, transform.rotation);
        VF1.GetComponent<Renderer>().material = m;
        GameObject VF2 = Instantiate(voxelFragment, position, transform.rotation);
        VF2.GetComponent<Renderer>().material = m;
        GameObject VF3 = Instantiate(voxelFragment, position, transform.rotation);
        VF3.GetComponent<Renderer>().material = m;
        GameObject VF4 = Instantiate(voxelFragment, position, transform.rotation);
        VF4.GetComponent<Renderer>().material = m;
        GameObject VF5 = Instantiate(voxelFragment, position, transform.rotation);
        VF5.GetComponent<Renderer>().material = m;
        NetworkServer.Spawn(VF);
        NetworkServer.Spawn(VF1);
        NetworkServer.Spawn(VF2);
        NetworkServer.Spawn(VF3);
        NetworkServer.Spawn(VF4);
        NetworkServer.Spawn(VF5);
    }


}
