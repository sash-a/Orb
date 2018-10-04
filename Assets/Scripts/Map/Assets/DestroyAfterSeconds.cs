using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DestroyAfterSeconds : MonoBehaviour
{
    public float time;
    
    void Start()
    {
        StartCoroutine(destroy());
    }

    IEnumerator destroy()
    {
        yield return new WaitForSeconds(time);
        NetworkServer.Destroy(gameObject);
        Destroy(gameObject);
    }
}