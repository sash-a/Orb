using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{

    Quaternion boltOrientation;

    private void Update()
    {
        //boltOrientation = transform.rotation;
    }

    void OnCollisionEnter(Collision collider)
    {
        // Debug.Log("Hit");
        //remove force when colliding
        var rb = transform.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        GetComponent<Gravity>().enabled = false;
        //transform.rotation = boltOrientation;
        transform.Translate(0, 0, 1);
 
        //get direction in update and set direction to that when colliding

    }
}


