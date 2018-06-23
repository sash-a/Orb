using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Digger : NetworkBehaviour
{
    Rigidbody body;
    int gradient = 2;//number of neighbours it will bouonce to before increasing the layer
    int neighbourCount;
    int depth = 7;
    int length = 8;
    int speed = 150;
    int size = 4;
    int maxSize = 20;

    //Vector3 entranceScale
    Vector3 stdscale = new Vector3(5000f, 1f,1f).normalized;//(3.5f, 0.5f, 0.5f)

    Vector3 nextDest;
    int layer;
    int colID;
    Vector3 travelDir;
    Vector3 right;

    System.Random rand;

    public enum State {
        Entrance, Cave, Exit
    };
    State state = State.Entrance;


    // Use this for initialization
    void Start()
    {
        //rand = new System.Random();
        //if (!isServer) { return; }
        //MapManager.manager.digger = this;
        //body = GetComponent<Rigidbody>();
        //gameObject.SetActive(false);
        //body.AddForce(Vector3.forward*80f, ForceMode.VelocityChange);
    }

    internal void init()
    {
        rand = new System.Random();
        body = GetComponent<Rigidbody>();
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //if (!isServer) { return; }
        //Debug.DrawRay(transform.position, travelDir*50, Color.red);
        if (Vector3.Distance(transform.position, nextDest) < 4f) {
            transform.position = nextDest;
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

            //Debug.Log("arived at dest " + transform.position);
            nextDest = getNextVox();
            travelDir = Vector3.Cross(-transform.position, right);
        }
        travelToNext();

    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("digger hit " + other.gameObject.name);
        var hit = other.gameObject;
        hitObject(hit);

        if (state.Equals(State.Entrance))
        {
            transform.localScale *= 0.999f;
        }
        if (state.Equals(State.Cave)){
            if (neighbourCount > 6)
            {
                transform.localScale += stdscale * 1.01f;
                if (transform.localScale.magnitude > maxSize)
                {
                    transform.localScale = transform.localScale.normalized * maxSize;
                }
                Debug.Log("increasing cave scale to mag:" + transform.localScale.magnitude);
            }
            else {

                transform.localScale += stdscale * 1.005f;
                if (transform.localScale.magnitude > maxSize)
                {
                    transform.localScale = transform.localScale.normalized * maxSize;
                }
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

    public void createEntranceAt(int colID) {
        this.colID = colID;
        //Debug.Log("rand: " + rand);
        int n = rand.Next(0, 2);
        int count = 0;
        layer = 0;
        neighbourCount = 0;
        transform.position = Vector3.zero;
        transform.localScale = stdscale * size;


        Vector3 dir = Vector3.zero;
        int neighbour = -1; ;
        foreach (int nei in MapManager.manager.neighboursMap[colID]) {
            if (count == n) {
                dir = MapManager.manager.getPositionOf(0, nei)- MapManager.manager.getPositionOf(0, colID)  ;
                neighbour = nei;
                break;
            }
            count++;
        }
        right = Vector3.Cross(dir, -MapManager.manager.getPositionOf(0, neighbour));
        travelDir = dir.normalized;
        nextDest = MapManager.manager.getPositionOf(0, colID);

        gameObject.SetActive(true);

        travelToNext();
    }


    private void travelToNext()
    {
        //Debug.Log(" aiming digger at  " + nextDest + "at col: " + colID + " layer " + layer);
        //Debug.Log("making vel " + (nextDest - transform.position).normalized * 100f + " aiming at col: " + colID);
        if (body == null)
        {
            body = GetComponent<Rigidbody>();
        }
        body.velocity = (nextDest - transform.position).normalized * speed; //100f;
        body.MoveRotation(Quaternion.LookRotation(travelDir.normalized, -transform.position.normalized));
        //Debug.Log("made " + body.velocity);
    }

    private Vector3 getNextVox()
    {
        if (gradient > 0)
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
            if (neighbourCount >= gradient)
            {
                layer++;
                neighbourCount = 0;
            }
            colID = bestID;
            return MapManager.manager.getPositionOf(layer, bestID);
        }
        else
        {
            neighbourCount++;
            layer--;
            //Debug.Log("sending digger to voxel above - layer " +( layer));
            return MapManager.manager.getPositionOf(layer, colID);
        }
    }

}
