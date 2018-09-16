using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class VoxelFragmentForce : MonoBehaviour
{
    public int minForce;
    public int maxForce;
    public float livingTime;

    // Use this for initialization
    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.AddForce
        (
            -Random.Range(minForce, maxForce) * (transform.position.normalized + Random.onUnitSphere).normalized,
            ForceMode.Impulse
        );
        
        Destroy(gameObject, livingTime);
    }
}