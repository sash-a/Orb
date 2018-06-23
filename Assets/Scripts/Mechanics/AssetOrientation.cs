using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Used to orientate a gameObject with a rigidBody correctly in the circle

public class AssetOrientation : MonoBehaviour
{

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.MoveRotation(Quaternion.LookRotation(getFoward(), -transform.position.normalized));
        StartCoroutine(SetParent());
    }

    IEnumerator SetParent() {
        yield return new WaitForSeconds(0.3f);
        transform.parent = MapManager.manager.Map.transform.GetChild(2);
    }

    public Vector3 getFoward()
    {
        var up = -transform.position.normalized;
        var foward = Vector3.Cross(up, transform.right);

        if (Vector3.Dot(foward, transform.forward) < 0)
        {
            foward *= -1;
        }

        return foward;
    }

}