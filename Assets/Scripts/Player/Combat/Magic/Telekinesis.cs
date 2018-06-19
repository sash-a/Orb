using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Telekenises : MonoBehaviour
{
    private Vector3 direction;
    private float distance;
    private Transform player;
    private bool stuck = false;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void setUp(Vector3 dir, float dist, Transform stuckTo)
    {
        direction = dir;
        distance = dist;
        player = stuckTo;
    }

    void OnCollisionEnter(Collision collision)
    {
//        player = collision.collider.transform; //what did we hit?
//        impactPosOffset = transform.position - player.position; //where were we relative to it?
//        impactRotOffset = transform.eulerAngles - player.eulerAngles; //how were we rotated relative to it?
//        stuck = true; //yeah, we hit something

        // TODO don't allow player to rotate
    }

    private void OnCollisionExit(Collision other)
    {
        stuck = false;
    }

    void Update()
    {
        
    }
}