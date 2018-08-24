using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    void OnCollisionEnter(Collision collider)
    {
        // Debug.Log("Hit");
        //remove force when colliding
        var rb = transform.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        GetComponent<Gravity>().enabled = false;
        //transform.GetComponent<Rigidbody>().mass = 0;
        //Vector3 offset = new Vector3(0, 0, 1); // replace with your desired offset vector

        //while(transform.gameObject != null)
        //{
        //    transform.position = collider.transform.position + offset;
        //}



    }
}


