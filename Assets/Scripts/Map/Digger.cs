using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Digger : NetworkBehaviour
{
    Rigidbody body;
    int gradient = 10;//number of neighbours it will bouonce to before increasing the layer
    int neighbourCount;


    Vector3 nextDest;
    int layer;
    int colID;
    Vector3 travelDir;



    // Use this for initialization
    void Start()
    {
        if (!isServer) { return; }
        body = GetComponent<Rigidbody>();
        //body.AddForce(Vector3.forward*80f, ForceMode.VelocityChange);
    }

    // Update is called once per frame
    bool updated = false;
    void Update()
    {
        updated = true;
        //Debug.Log("-------------UPDATE------------------");
        //Debug.Log("vel = " + body.velocity);

    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("bullet hit thing");
        var hit = other.gameObject;
        hitObject(hit);
        if (hit.name == "TriVoxel")
        {
            if (layer < MapManager.mapLayers - 1)
            {
                if (Vector3.Distance(transform.position, nextDest) > 0.2f)
                {//if i have arrived
                    Debug.Log("arived at dest " + transform.position);
                    nextDest = getNextVox();
                    travelToNext();
                }
                else
                {
                    travelToNext();
                }
            }
        }
    }


    private void hitObject(GameObject hit)
    {
        //Debug.Log("digger hit " + hit.name);
        var health = hit.GetComponent<Health>();
        if (health != null && hit.tag != "Player")
        {
            health.takeDamage(1000);
        }
        //Destroy(gameObject);
    }

    public void createEntranceAt(int colID, Vector3 dir)
    {
        nextDest = MapManager.voxels[0][colID].worldCentreOfObject;
        layer = 0;
        neighbourCount = 0;
        transform.position = Vector3.zero;
        travelToNext();
    }

    private void travelToNext()
    {
        Debug.Log(" aiming digger at  " + nextDest + "at col: " + colID + " layer " + layer);
        Debug.Log("making vel " + (nextDest - transform.position).normalized * 100f + " aiming at col: " + colID);
        if (body == null)
        {
            body = GetComponent<Rigidbody>();
        }
        body.velocity = (nextDest - transform.position).normalized * 10f; //100f;
        //Debug.Log("made " + body.velocity);
    }

    private Vector3 getNextVox()
    {
        int bestID = -1;
        double bestComp = double.MinValue;

        foreach (int n in MapManager.neighboursMap[colID])
        {//finds neighbour in dir closest to desired dir
            double comp = Vector3.Dot(MapManager.voxelPositions[layer][n] - transform.position, travelDir);
            if (comp > bestComp)
            {
                bestComp = comp;
                bestID = n;
            }
        }
        neighbourCount++;
        if (neighbourCount >= gradient)
        {
            layer++;
            neighbourCount = 0;
        }
        colID = bestID;
        return MapManager.voxelPositions[layer][bestID];
    }
}
