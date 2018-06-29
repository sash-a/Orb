using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Digger : NetworkBehaviour
{
    static int speed = 150;//150

    public int gradient;//number of neighbours it will bouonce to before increasing the layer
    public int neighbourCount;//the count for number of runs without rise
    public static int maxSize = 20;
    public static int minSize = 3;


    public static Vector3 stdscale = new Vector3(5000f, 1f, 1f).normalized;//(3.5f, 0.5f, 0.5f)

    public Vector3 nextDest;
    public int layer;
    public int colID;
    public Vector3 travelDir;
    public Vector3 right;//right remains constant along a path - is used to rotate around as digger moves through sphere

    System.Random rand;
    Rigidbody body;
    public CaveComponent master;


    internal void init(CaveComponent m)
    {
        rand = new System.Random();
        body = GetComponent<Rigidbody>();
        gameObject.SetActive(false);
        master = m;
    }

    // Update is called once per frame
    void Update()
    {
        //if (!isServer) { return; }
        //Debug.DrawRay(transform.position, travelDir*50, Color.red);
        if (Vector3.Distance(transform.position, nextDest) < 4f)
        {
            transform.position = nextDest;
            /**
            if (layer == depth && state.Equals(State.Entrance)) {
                gradient = length + 2;
                neighbourCount = 0;
                travelDir = travelDir.normalized + right *(float) (rand.NextDouble() * 1.5f - 0.75f);
                right = Vector3.Cross(travelDir, -transform.position);
                state = State.Cave;
                transform.localScale = Vector3.one * size;
                layer-=2;
                //Debug.Log("finished digging entrance");
            }
            if (state.Equals(State.Cave) && neighbourCount >= length) {//has finished digging cave
                Debug.Log("finished digging cave - size : " + transform.localScale.magnitude);
                state = State.Exit;
                gradient=0;
                transform.localScale = stdscale* size;
            }
            if (state.Equals(State.Exit)&& layer<=0) {
                Debug.Log("finished cave");
                CaveManager.removeDigger(this);
            }
            */

            master.informArrived();
            nextDest = getNextVox();
            travelDir = Vector3.Cross(-transform.position, right);
        }
        travelToNext();

    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("digger hit " + other.gameObject.name);
        if (other.gameObject.name.Contains("voxel") || other.gameObject.name.Contains("Voxel"))
        {
            CaveEntrance test = master as CaveEntrance;
            //if ( other.gameObject.GetComponent<Voxel>().layer>0)
            if ((test != null) || other.gameObject.GetComponent<Voxel>().layer > 1)
            {//only cave entrances can destroy surface level voxels
                var hit = other.gameObject;
                hitObject(hit);
            }
        }
    }


    private void hitObject(GameObject hit)
    {
        var health = hit.GetComponent<NetHealth>();
        if (health != null && hit.tag != "Player")
        {
            //health.takeDamage(1000);
            health.RpcDamage(1000);
        }
        //Destroy(gameObject);
    }



    Vector3 lastDir = Vector3.zero;
    public void travelToNext()
    {
        //Debug.Log(" aiming digger at  " + nextDest + "at col: " + colID + " layer " + layer);
        //Debug.Log("making vel " + (nextDest - transform.position).normalized * 100f + " aiming at col: " + colID);
        if (body == null)
        {
            body = GetComponent<Rigidbody>();
        }
        body.velocity = (nextDest - transform.position).normalized * speed; //100f;
        body.MoveRotation(Quaternion.LookRotation(travelDir.normalized, -transform.position.normalized));
        if (Vector3.Dot((nextDest - transform.position).normalized, lastDir) < 0)
        {//has passed the required dest in the last step
            //Debug.Log("digger passed dest");
        }
        lastDir = (nextDest - transform.position).normalized;
        //Debug.Log("made " + body.velocity);
    }

    private Vector3 getNextVox()
    {

        int bestID = -1;
        double bestComp = double.MinValue;

        foreach (int n in MapManager.manager.neighboursMap[colID])
        {//finds neighbour in dir closest to desired dir
            double comp = Vector3.Dot((MapManager.manager.getPositionOf(layer, n) - transform.position).normalized, travelDir);
            if (comp > bestComp)
            {
                bestComp = comp;
                bestID = n;
            }
        }
        //Debug.Log("found next best vox : " + bestID + " with comp in travelDir = " + bestComp);
        neighbourCount++;
        if (gradient > 0)
        {
            if (neighbourCount >= gradient)
            {
                layer++;
                neighbourCount = 0;
            }
        }
        else if (gradient < 0)
        {
            if (neighbourCount >= -gradient)
            {
                layer--;
                neighbourCount = 0;
            }
        }

        colID = bestID;
        return MapManager.manager.getPositionOf(layer, bestID);


    }

    public void setScale(float s)
    {
        transform.localScale = stdscale * s;
    }
}
