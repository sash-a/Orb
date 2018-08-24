using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterGravity : MonoBehaviour
{

    private Rigidbody rb;
    private static float gravcceleration = 35000f;
    private static float anchorAcceleration = 1000f;


    public Voxel attachedVoxel;
    CritterActions actions;

    HashSet<Vector3> voxelSideNormals;
    Vector3[] mainNormals;


    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainNormals = new Vector3[2];
        voxelSideNormals = new HashSet<Vector3>();
        actions = GetComponent<CritterActions>();
        actions.autoOrientate = false;

        //transform.GetChild(0).GetChild(0).gameObject.GetComponent<CritterAnchor>().gravity = this;
    }

    // Update is called once per frame
    void Update()
    {

        if (attachedVoxel == null)
        {//if not anchored - fall down as per normal gravity
            //transform.rotation = Quaternion.LookRotation(getFoward(), -transform.position.normalized);
        }
        else
        {//is anchored to a voxel - fall into it - using the normal to the voxel which best fits the position of the critter
         // transform.rotation = Quaternion.LookRotation(getFoward(), norm);


        }

    }

    int gravityCounter = 0;
    int gravityIgnoreTime = 150;//amount of time critter waits to attach to new voxel before falling


    Vector3 lastNorm;
    private void FixedUpdate()
    {
        wallCounter--;
        if (!getFoward().Equals(Vector3.zero) && !transform.position.Equals(Vector3.zero))
        {
            if (attachedVoxel == null && gravityCounter <= 0)
            {//if not anchored - fall down as per normal gravity

                //transform.rotation = Quaternion.LookRotation(forward, -transform.position.normalized);
                rb.AddForce(transform.position.normalized * gravcceleration * Time.deltaTime);

                //Debug.Log("using gravity");
            }
            else
            {//is anchored to a voxel - fall into it - using the normal to the voxel which best fits the position of the critter
                Vector3 norm;
                if (attachedVoxel == null)
                {
                    gravityCounter--;
                    norm = lastNorm;
                    rb.AddForce(-norm * gravcceleration * Time.deltaTime);//falls in the direction you were last in - at gravity speeds
                    //Debug.Log("no attached voxel but still on previous voxels grav time - falling using: " + -norm * gravityCounter);
                }
                else
                {
                    norm = getBestNorm();
                    //Debug.DrawRay(attachedVoxel.worldCentreOfObject, norm * 5f, Color.red, 1f);
                    gravityCounter = gravityIgnoreTime;
                    lastNorm = norm;
                    rb.AddForce(-norm * anchorAcceleration * Time.deltaTime);

                }
                //Debug.Log("using vo norm to orientate. norm = " + norm + " positional up = " + -transform.position.normalized);
                //transform.rotation = Quaternion.LookRotation(forward, norm);
                //rb.MoveRotation(Quaternion.LookRotation(forward, norm));

            }
        }

        rb.MoveRotation(Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(getFoward(), -getFallDir()), 0.1f));

        actions.doMovement();
        actions.doRotations();

        //Debug.Log("forward after rotations " + getFoward());
    }

    public Vector3 getFallDir()
    {
        if (attachedVoxel == null)
        {
            if (gravityCounter > 0)
            {
                try
                {
                    return -(lastNorm.Equals(Vector3.zero) ? getBestNorm() : lastNorm);
                }
                catch
                {
                    return transform.position.normalized;
                }
            }
            else
            {
                return transform.position.normalized;
            }
        }
        else
        {
            if (lastNorm != null)
            {
                return -(lastNorm.Equals(Vector3.zero) ? getBestNorm() : lastNorm);
            }
            else
            {
                return -getBestNorm();
            }
        }
    }

    public Vector3 getFoward()
    {
        var up = -getFallDir();
        var foward = Vector3.Cross(up, transform.right);

        if (Vector3.Dot(foward, transform.forward) < 0)
        {
            foward *= -1;
        }

        return foward;
    }

    private Vector3 getBestNorm()
    {
        float bestFit = float.MinValue;
        Vector3 normal = Vector3.zero;

        bool flatNorm = checkMainNorm();

        if (flatNorm)//only check the main norms if the critter is definitely above/below the voxel
        {
            Debug.Log("hit flat side");

            for (int i = 0; i < 2; i++)
            {
                float fit = Vector3.Dot(-(attachedVoxel.worldCentreOfObject - transform.position).normalized, mainNormals[i]);
                if (fit > bestFit)
                {
                    normal =mainNormals[i];
                    bestFit = fit;
                }
            }
        }
        else
        {
            Debug.Log("hit wall");
            foreach (Vector3 n in voxelSideNormals)
            {
                //the fit of a normal is how much in the direction from the critter to voxel the normal is - with a bias towards the larger planes
                float fit = Vector3.Dot(n, -(attachedVoxel.worldCentreOfObject - transform.position).normalized);
                if (fit > bestFit)
                {
                    bestFit = fit;
                    normal = n;
                    //Debug.Log("found better norm: " + normal);
                }
            }
        }

        //Debug.Log("best norm is: " + normal);
        return normal;
    }

    /*
     *checks if the critter is on (top of or bellow - true) or on the sides of a voxel 
     */
    private bool checkMainNorm()
    {
        Mesh mesh = attachedVoxel.GetComponent<MeshFilter>().mesh;
        bool hitMainNorm = false;
        float scale = (float)attachedVoxel.scale * MapManager.mapSize;

        /*
        for (int i = 0; i < 4; i += 3)
        {
            Plane p = new Plane(mesh.vertices[i] * scale, mesh.vertices[i + 1] * scale, mesh.vertices[i + 2] * scale);//plane goiing through main surface
            float enter = 0.0f;

            if (p.Raycast(new Ray(transform.position, (attachedVoxel.worldCentreOfObject - transform.position)), out enter))
            {
                //Get the point that is clicked
                //Vector3 hitPoint = ray.GetPoint(enter);

                if (enter <= (attachedVoxel.worldCentreOfObject - transform.position).magnitude * 1.05f)
                {
                    hitMainNorm = true;
                }
                else
                {
                    //Debug.Log("intercepted plane - not close enough. dist = " + enter + " needed " + (attachedVoxel.worldCentreOfObject - transform.position).magnitude * 1.2f);
                }
            }
        }
        */
        for (int side = -1; side < 2; side += 2)
        {
            Vector3[] mainFaceVerts;
            mainFaceVerts = attachedVoxel.getMainFaceAtLayer(side);
            Plane p = new Plane(mainFaceVerts[0]* scale, mainFaceVerts[1] * scale, mainFaceVerts[2] * scale);//plane goiing through main surface
            float enter = 0.0f;

            if (p.Raycast(new Ray(transform.position, (attachedVoxel.worldCentreOfObject - transform.position)), out enter))
            {
                //Get the point that is clicked
                //Vector3 hitPoint = ray.GetPoint(enter);

                if (enter <= (attachedVoxel.worldCentreOfObject - transform.position).magnitude * 1.05f)
                {
                    hitMainNorm = true;
                }
                else
                {
                    //Debug.Log("intercepted plane - not close enough. dist = " + enter + " needed " + (attachedVoxel.worldCentreOfObject - transform.position).magnitude * 1.2f);
                }
            }

        }

        return hitMainNorm;
    }

    public int wallCounter = 0;
    int wallTime = 15;

    public void replaceVoxel(Voxel v)
    {

        HashSet<Vector3> oldSideNormals = voxelSideNormals;
        Vector3[] oldMainNormals = mainNormals;

        voxelSideNormals = new HashSet<Vector3>();
        mainNormals = new Vector3[2];
        int mainCount = 0;

        Mesh mesh = v.GetComponent<MeshFilter>().mesh;
        /*
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3 norm = Vector3.Cross((mesh.vertices[mesh.triangles[i]] - mesh.vertices[mesh.triangles[i + 1]]), (mesh.vertices[mesh.triangles[i + 1]] - mesh.vertices[mesh.triangles[i + 2]])).normalized;

            if (isFlatSide(mesh.triangles[i], mesh.triangles[i + 1], mesh.triangles[i + 2]))
            {
                mainNormals[mainCount] = norm;
                mainCount++;
                Debug.DrawRay(v.worldCentreOfObject, norm * 3f, Color.blue, 20f);
            }
            else
            {
                voxelSideNormals.Add(norm);
                Debug.DrawRay(v.worldCentreOfObject, norm * 2f, Color.green, 20f);
            }            

        }
        */

        Vector3[] mainFaceVerts;
        for (int side = -1; side < 2; side += 2)
        {
            mainFaceVerts = v.getMainFaceAtLayer(side);
            Vector3 norm = Vector3.Cross((mainFaceVerts[0] - mainFaceVerts[1]), (mainFaceVerts[1] - mainFaceVerts[2])).normalized;
            mainNormals[mainCount] =(mainCount==0?1:-1)* norm;
            mainCount++;
            Debug.DrawRay(v.worldCentreOfObject, norm * 3f, Color.blue, 20f);

        }

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3 norm = Vector3.Cross((mesh.vertices[mesh.triangles[i]] - mesh.vertices[mesh.triangles[i + 1]]), (mesh.vertices[mesh.triangles[i + 1]] - mesh.vertices[mesh.triangles[i + 2]])).normalized;

            if (!mainNormals[0].Equals(norm) && !mainNormals[1].Equals(norm))
            {
                voxelSideNormals.Add(norm);
                Debug.DrawRay(v.worldCentreOfObject, norm * 2f, Color.green, 20f);
            }
        }
        if (mainCount != 2)
        {
            Debug.LogError("did not find 2 main normals for voxel");
        }

        if (attachedVoxel == null)
        {
            attachedVoxel = v;
        }

        if (voxelSideNormals.Contains(getBestNorm()))
        {
            //the newly hit voxel has been hit side on
            Debug.Log("hit wall of new voxel");
            wallCounter = wallTime;
            attachedVoxel = v;
        }
        else
        {//not wall
            if (wallCounter > 0 && false)
            {//should still be on the wall
                voxelSideNormals = oldSideNormals;
                mainNormals = oldMainNormals;
                Debug.Log("went from wall to not wall too quickly - reverting to old wall");
            }
            else
            {
                attachedVoxel = v;
            }
        }

    }


    private bool isFlatSide(int v1, int v2, int v3)
    {
        for (int i = 0; i < 2; i++)//for each flat side(base and top)
        {
            if ((v1 == i * 3 || v2 == i * 3 || v3 == i * 3) && (v1 == i * 3 + 1 || v2 == i * 3 + 1 || v3 == i * 3 + 1) && (v1 == i * 3 + 2 || v2 == i * 3 + 2 || v3 == i * 3 + 2))
            {//these 3 points are either 0,1,2 or 3,4,5
                return true;
            }
        }
        return false;
    }
}
