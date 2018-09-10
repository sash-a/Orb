using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class VoxelContainer : Voxel
{

    /*
     * A voxel container is a voxel which has been broken into peices 
     * while the entire thing behaves as a single voxel - this is only for storage purposes, in fact it is a container for further subvoxels
     * each of which occupies the same layer,column tuple position - but have different meshes
     */

    public ArrayList subVoxels;
    bool shatterSmoothedVoxels = false;


    //NB: the old voxel object should be deleted once this voxel container has been created
    public void start(Voxel majorVoxel)
    {//creates the voxel container from the old voxel scripts data - then replaces it in the mapManager
        //Debug.Log("creating voxel container");
        gameObject.name = "Container";

        layer = majorVoxel.layer;
        columnID = majorVoxel.columnID;
        subVoxelID = majorVoxel.subVoxelID;
        subVoxels = new ArrayList();
        rand = new System.Random(columnID * layer + columnID);

        shatterLevel = majorVoxel.shatterLevel;

        createVoxelContainer(majorVoxel);


        lastHitRay = majorVoxel.lastHitRay;
        lastHitPosition = majorVoxel.lastHitPosition;
        if (shatterLevel < MapManager.manager.shatters)
        {
            //damageContainer();
        }
        //Debug.Log("created container: " + this);

    }

    private void Start()
    {
        //gameObject.name = "Container";
    }


    private void damageContainer()
    {
        //if(v.gameObject.GetComponent<MeshCollider>().)
        //Debug.Log("casting ray ");
        RaycastHit hit;
        bool found = false;
        if (Physics.Raycast(lastHitRay, out hit))
        {
            //Debug.Log("rehit voxel with shatter level: " + hit.collider.gameObject.GetComponent<Voxel>().shatterLevel);
            Voxel hitVox = hit.collider.gameObject.GetComponent<Voxel>();
            if (subVoxels.Contains(hitVox))
            {
                lastHitPosition = hit.point;
                hitVox.lastHitPosition = hit.point;
                //Debug.Log("rehit subvoxel in same voxel");
                if (shatterLevel < MapManager.manager.shatters - 1)
                {
                    //VoxelContainer vc = new VoxelContainer(hitVox);
                }
                Destroy(hitVox.gameObject);
                found = true;
            }

        }
        if (!found)
        {
            double d = Double.MaxValue;
            Voxel closest = null;
            foreach (Voxel v in subVoxels)
            {
                if (Vector3.Distance(v.worldCentreOfObject, lastHitPosition) < d)
                {
                    d = Vector3.Distance(v.worldCentreOfObject, lastHitPosition);
                    closest = v;
                }
            }
            if (closest != null)
            {
                //Debug.Log("settling for a closest solution");
                if (shatterLevel < MapManager.manager.shatters - 1)
                {
                    //VoxelContainer vc = new VoxelContainer(closest);
                }
                Destroy(closest.gameObject);
            }
            else
            {
                //Debug.Log("couldnt find a closest");
            }
        }

    }

    public void createVoxelContainer(Voxel majorVoxel)
    {

        Mesh majorMesh = majorVoxel.filter.mesh;
        if (majorVoxel.deletedPoints.Count == 0 || shatterSmoothedVoxels)
        {//is a full voxel
            //need to construct 6 submeshes using the major mesh data
            Vector3[] centerPoints = getCenterPoints(majorMesh, majorVoxel);//0 bottom; 1 middle; 2 top

            randDev = new float[] { (float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble() };
            
            //for (int i = 0; i < (shatterLevel == 0 ? 6 : 8); i++)//for the 6 split first then the 8 split
            for (int i = 0; i < (majorVoxel.shatterLevel==0?8:4); i++)// for the 8 split both times
                //for (int i = 0; i < (shatterLevel == 0 ? 8 : 4); i++)//for the 8 split then the 4 split(8split without cutting the voxel depth in two)
                {
                //sub mesh i is the triangle that goes from vert i -> vert [(i+1)%3+ (i > 2 ? 3 : 0)] -> center - 0,1,2 is bottom 3    4,5,6 is top 3
                GameObject subVoxelObject = genNewSubVoxel();
                Voxel subVoxelScript = subVoxelObject.GetComponent<Voxel>();

                MeshFilter subMesh = subVoxelObject.GetComponent<MeshFilter>();
                Vector3[] verts = getSubMesh(majorMesh, centerPoints, i, shatterLevel,majorVoxel);
                subMesh.mesh.vertices = verts;
                if (majorVoxel.shatterLevel > 0 && i == 3)
                {
                    subMesh.mesh.triangles = getTriangles(i, !majorVoxel.isBottom);
                }
                else {
                    subMesh.mesh.triangles = getTriangles(i, majorVoxel.isBottom);
                }
                //Debug.Log("binding i=" + i + " triangles : " + subMesh.mesh.triangles[0] + " ; " + subMesh.mesh.triangles[1] + " ; " + subMesh.mesh.triangles[2] + " ; " + subMesh.mesh.triangles[3] + " ; " + subMesh.mesh.triangles[4] + " ; " + subMesh.mesh.triangles[5] + " ; ");
                subMesh.name = "subVoxelShape";
                subMesh.mesh.uv = new[]{
                    new Vector2(0.3f, 0.4f), new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.6f),
                    new Vector2(0.1f, 0.6f),
                    new Vector2(0.1f, 0.9f), new Vector2(0.4f, 0.9f)};

                subMesh.mesh.RecalculateNormals();
                subVoxelObject.GetComponent<MeshCollider>().sharedMesh = subMesh.mesh;


                subVoxelScript.layer = majorVoxel.layer;
                subVoxelScript.columnID = majorVoxel.columnID;
                subVoxelScript.centreOfObject = getCentrePoint(verts);
                //subVoxelScript.info += "sub i=" + i;
                subVoxelScript.shatterLevel = majorVoxel.shatterLevel + 1;
                subVoxelScript.isBottom = i > 2 && i <= 5 || (i == 7 && !majorVoxel.isBottom) || (i == 6 && majorVoxel.isBottom);
                //subVoxelScript.isBottom = subVoxelScript.isBottom;
                subVoxelScript.subVoxelID = majorVoxel.subVoxelID + "," + i;
                subVoxelScript.info += "id: " + (subVoxelScript.subVoxelID);
                subVoxelScript.hasEnergy = majorVoxel.hasEnergy;
                subVoxelScript.isCaveFloor = majorVoxel.isCaveFloor;
                subVoxelScript.isCaveBorder = majorVoxel.isCaveBorder;
                subVoxelScript.isCaveCeiling = majorVoxel.isCaveCeiling;
                subVoxelScript.shatterCap = majorVoxel.shatterCap;

                subVoxelScript.filter = subMesh;
                subVoxelScript.deletedPoints = new HashSet<int>();


                //Instantiate(subVoxelObject);
                double scale = Math.Pow(scaleRatio, Math.Abs(layer));
                subVoxelScript.worldCentreOfObject = subVoxelScript.centreOfObject * (float)scale * MapManager.mapSize;
                //Debug.Log("scaling up subVoxel from " + subVoxelObject.transform.localScale + " to " + (Vector3.one * (float)scale));
                subVoxelObject.transform.localScale = Vector3.one * (float)scale * MapManager.mapSize;
                //subVoxelObject.transform.parent = gameObject.transform;
                subVoxels.Add(subVoxelScript);

                subVoxelScript.checkTriangleNorms(5);


                if (isServer)
                {
                    NetworkServer.Spawn(subVoxelObject);
                }
            }
        }

        layer = majorVoxel.layer;
        columnID = majorVoxel.columnID;
    }



    private int[] getTriangles(int i, bool parentIsBottom)
    {
        int[] tris = genVolumeTriangles();
        if (((i > 2 && i <= 5) && !parentIsBottom) || (i < 3 && parentIsBottom) || (i == 7 && !parentIsBottom) || (i == 6 && parentIsBottom))
        {
            //Debug.Log("shifting verts in sub i = " + i);
            for (int j = 0; j < 24; j += 3)
            {
                int temp = tris[j];
                tris[j] = tris[j + 2];
                tris[j + 2] = temp;
            }
        }
        return tris;
    }

    private Vector3 getCentrePoint(Vector3[] vertices)
    {
        Vector3 av = new Vector3(0, 0, 0);
        for (int i = 0; i < vertices.Length; i++)
        {
            av += vertices[i];
        }

        return av / vertices.Length;
    }


    float[] randDev;
    public Vector3[] getSubMesh(Mesh majorMesh, Vector3[] centerPoints, int i, int shatterLevel, Voxel majorVoxel)
    {
        int subsequent = (i + 1) % 3 + (i > 2 ? 3 : 0);
        Vector3[] verts = new Vector3[6];

        float variation = 0.5f;

        Vector3[] majorVerts = new Vector3[6];
        int count = 0;
        for (int j = 0; j < 6; j++)
        {
            int ind;
            if (majorVoxel.deletedPoints.Contains(i))
            {
                //ind = (j + 3 - majorVoxel.deletedPoints.Count) % (6 - majorVoxel.deletedPoints.Count);
                ind = (j + 3) % 6;
            }
            else {
                //ind = count;
                ind = j;
                count++;
            }
            try
            {
                majorVerts[j] = majorMesh.vertices[ind];
            }
            catch {
                Debug.LogError("trying to index vert list of len " + majorMesh.vertices.Length + " with ind = " + ind + " j = " + j + " count = " + count + " num deleted = " + majorVoxel.deletedPoints.Count);
            }
        }

        if (shatterLevel == 0 && !false)
        {
            if (i < 6)
            {//the first 6 are outter(bottom and top) 
                int remaining = findRemainingOnSide(new int[] { i, subsequent })[0];
                verts[0] = majorVerts[i];
                verts[1] = majorVerts[i] + (majorVerts[subsequent] - majorVerts[i]) * (0.5f - variation / 2.0f + randDev[i % 3] * variation);
                verts[2] = majorVerts[i] + (majorVerts[remaining] - majorVerts[i]) * (0.5f + variation / 2.0f - randDev[remaining % 3] * variation);

                verts[3] = getMidPoint(majorVerts, i);
                verts[4] = getMidPoint(majorVerts, i) + (getMidPoint(majorVerts, subsequent) - getMidPoint(majorVerts, i)) * (0.5f - variation / 2.0f + randDev[i % 3] * variation);
                verts[5] = getMidPoint(majorVerts, i) + (getMidPoint(majorVerts, remaining) - getMidPoint(majorVerts, i)) * (0.5f + variation / 2.0f - randDev[remaining % 3] * variation);

            }
            else
            {//the inner triangles- one bottom one top
                int a = (i == 6 ? 0 : 3);
                for (int k = 0; k < 3; k++)
                {
                    verts[k] = majorVerts[k + a] + (majorVerts[(k + 1) % 3 + a] - majorVerts[k + a]) * (0.5f - variation / 2.0f + randDev[k % 3] * variation);

                    verts[k + 3] = getMidPoint(majorVerts, k + a) + (getMidPoint(majorVerts, (k + 1) % 3 + a) - getMidPoint(majorVerts, k + a)) * (0.5f - variation / 2.0f + randDev[k % 3] * variation);
                }

            }
        }
        else
        {
            if (i < 3)
            {//the first 6 are outter(bottom and top) 
                int remaining = findRemainingOnSide(new int[] { i, subsequent })[0];
                verts[0] = majorVerts[i];
                verts[1] = majorVerts[i] + (majorVerts[subsequent] - majorVerts[i]) * (0.5f - variation / 2.0f + randDev[i % 3] * variation);
                verts[2] = majorVerts[i] + (majorVerts[remaining] - majorVerts[i]) * (0.5f + variation / 2.0f - randDev[remaining % 3] * variation);

                verts[3] = majorVerts[i+3];
                verts[4] = majorVerts[i+3] + (majorVerts[subsequent+3] - majorVerts[i+3]) * (0.5f - variation / 2.0f + randDev[i % 3] * variation);
                verts[5] = majorVerts[i+3] + (majorVerts[remaining+3] - majorVerts[i+3]) * (0.5f + variation / 2.0f - randDev[remaining % 3] * variation);

            }
            else
            {//the inner triangles- one bottom one top
                for (int k = 0; k < 3; k++)
                {
                    verts[k] = majorVerts[k + 0] + (majorVerts[(k + 1) % 3 + 0] - majorVerts[k + 0]) * (0.5f - variation / 2.0f + randDev[k % 3] * variation);

                    verts[k + 3] = majorVerts[k + 3] + (majorVerts[(k + 1) % 3 + 3] - majorVerts[k + 3] )* (0.5f - variation / 2.0f + randDev[k % 3] * variation);
                }

            }
        }
        return verts;

    }

    private static Vector3 getMidPoint(Vector3[] majorVerts, int i)
    {
        i = i % 3;
        return (majorVerts[i] + majorVerts[i + 3]) / 2.0f;
    }

    private Vector3[] getCenterPoints(Mesh mesh, Voxel majorVox)
    {
        float heightVar = 0.4f;//proportion of the height the middle cnter can move up and down along 
        Vector3[] res = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero };
        //Debug.Log("getting centers - orig mesh has " + mesh.vertices.Length + " verts");
        res[0] = (mesh.vertices[(majorVox.deletedPoints.Contains(0)?3:0)] + mesh.vertices[(majorVox.deletedPoints.Contains(1) ? 4 : 1)] + mesh.vertices[(majorVox.deletedPoints.Contains(2) ? 5 : 2)]) / 3.0f;
        res[2] = (mesh.vertices[(majorVox.deletedPoints.Contains(3) ? 0 : 3)] + mesh.vertices[(majorVox.deletedPoints.Contains(4) ? 1 : 4)] + mesh.vertices[(majorVox.deletedPoints.Contains(5) ? 2 : 5)]) / 3.0f;
        res[1] = res[0] + (res[2] - res[0]) * (0.5f - heightVar / 2.0f + (float)(rand.NextDouble() * heightVar));

        float len = (res[2] - res[0]).magnitude;//to make deviation relative to vox size
        float variationFac = 0.5f;

        Vector3 variationDir = ((mesh.vertices[(majorVox.deletedPoints.Contains(0) ? 3 : 0)] - mesh.vertices[(majorVox.deletedPoints.Contains(1) ? 4 : 1)]) * (float)rand.NextDouble() + (mesh.vertices[(majorVox.deletedPoints.Contains(1) ? 4 : 1)] - mesh.vertices[(majorVox.deletedPoints.Contains(2) ? 5 : 2)]) * (float)rand.NextDouble() + (mesh.vertices[(majorVox.deletedPoints.Contains(0) ? 5 : 2)] - mesh.vertices[(majorVox.deletedPoints.Contains(0) ? 3 : 0)]) * (float)rand.NextDouble()).normalized;

        for (int i = 0; i < 3; i++)
        {
            res[i] += variationDir * len * variationFac;
        }



        return res;
    }

    GameObject genNewSubVoxel()
    {
        GameObject subVoxelObject = (GameObject)Instantiate(Resources.Load("Prefabs/Map/SubVoxel"));
        subVoxelObject.transform.position = Vector3.zero;

        subVoxelObject.tag = "TriVoxel";
        subVoxelObject.name = "SubVoxel";

        return subVoxelObject;
    }

}
