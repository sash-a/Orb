using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelFragmentForce : MonoBehaviour
{

    public int forceStrength = 40;
    public int eRadius = 10;

    // Use this for initialization
    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        //rb.AddForce(-forceStrength* transform.position.normalized, ForceMode.Acceleration)
        rb.AddExplosionForce(-forceStrength, transform.position.normalized, eRadius);

        Destroy(gameObject, 3);
    }


}
