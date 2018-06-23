﻿using System;
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

    //NB: the old voxel object should be deleted once this voxel container has been created
    public void start(Voxel majorVoxel)
    {//creates the voxel container from the old voxel scripts data - then replaces it in the mapManager
        //Debug.Log("creating voxel container");

        layer = majorVoxel.layer;
        columnID = majorVoxel.columnID;
        subVoxelID = majorVoxel.subVoxelID;
        subVoxels = new ArrayList();
        rand = new System.Random();
   
        shatterLevel = majorVoxel.shatterLevel;

        createVoxelContainer(majorVoxel);
      
 
        lastHitRay = majorVoxel.lastHitRay;
        lastHitPosition = majorVoxel.lastHitPosition;
        if (shatterLevel < MapManager.shatters)
        {
            //damageContainer();
        }
        //Debug.Log("created container: " + this);
       
    }

    

    private void Start()
    {
        //Debug.Log("started container");
        if (shatterLevel == 0)
        {
            //Debug.Log("replacing triVoxel with voxel container at: " + layer + " ," + columnID);
            //container = this;
            //MapManager.manager.voxels[layer][columnID] = this;

            //var test = MapManager.manager.voxels[layer][columnID];
            //Debug.Log("replaced tlv with: " + test + " type: " + test.GetType());
        }
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
                if (shatterLevel < MapManager.shatters - 1)
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
                if (shatterLevel < MapManager.shatters - 1)
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
        if (majorVoxel.deletedPoints.Count == 0)
        {//is a full voxel
            //need to construct 6 submeshes using the major mesh data
            Vector3[] centerPoints = getCenterPoints(majorMesh);//0 bottom; 1 middle; 2 top
            for (int i = 0; i < (shatterLevel == 0 ? 6 : 8); i++)
            {
                //sub mesh i is the triangle that goes from vert i -> vert [(i+1)%3+ (i > 2 ? 3 : 0)] -> center - 0,1,2 is bottom 3    4,5,6 is top 3
                GameObject subVoxelObject = genNewSubVoxel();
                Voxel subVoxelScript = subVoxelObject.GetComponent<Voxel>();

                MeshFilter subMesh = subVoxelObject.GetComponent<MeshFilter>();
                Vector3[] verts = getSubMesh(majorMesh, centerPoints, i);
                subMesh.mesh.vertices = verts;
                subMesh.mesh.triangles = getTriangles(i, majorVoxel.isBottom);
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
                subVoxelScript.subVoxelID = majorVoxel.subVoxelID + "," + i;
                subVoxelScript.info += "id: " + (subVoxelScript.subVoxelID);

                subVoxelScript.filter = subMesh;
                subVoxelScript.deletedPoints = new HashSet<int>();



                //Instantiate(subVoxelObject);
                double scale = Math.Pow(scaleRatio, Math.Abs(layer));
                subVoxelScript.worldCentreOfObject = subVoxelScript.centreOfObject * (float)scale * MapManager.mapSize;
                //Debug.Log("scaling up subVoxel from " + subVoxelObject.transform.localScale + " to " + (Vector3.one * (float)scale));
                subVoxelObject.transform.localScale = Vector3.one * (float)scale * MapManager.mapSize;
                subVoxelObject.transform.parent = gameObject.transform;
                subVoxels.Add(subVoxelScript);
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


    private Vector3[] getSubMesh(Mesh majorMesh, Vector3[] centerPoints, int i)
    {
        int subsequent = (i + 1) % 3 + (i > 2 ? 3 : 0);
        Vector3[] verts = new Vector3[6];

        if (shatterLevel == 0)
        {
            //Debug.Log("i= " + i + " subsequent = " + subsequent);

            verts[0] = majorMesh.vertices[i];
            verts[1] = majorMesh.vertices[subsequent];//the successive point on the same side as point i
            verts[2] = centerPoints[i < 3 ? 0 : 2];//the correct center point to the level of the submesh
                                                   //Debug.Log("outter center point = " + verts[2]);

            verts[3] = getMidPoint(majorMesh, i);//the midpoint abov or below point i depending on whether i is top or bottom
            verts[4] = getMidPoint(majorMesh, subsequent);
            verts[5] = centerPoints[1];
            //Debug.Log("inner center point = " + verts[5]);
        }
        else
        {
            if (i < 6)
            {
                verts[0] = majorMesh.vertices[i];
                verts[1] = (majorMesh.vertices[i] + majorMesh.vertices[subsequent]) / 2.0f;
                verts[2] = (majorMesh.vertices[i] + majorMesh.vertices[findRemainingOnSide(new int[] { i, subsequent })[0]]) / 2.0f;

                verts[3] = getMidPoint(majorMesh, i);
                verts[4] = (getMidPoint(majorMesh, i) + getMidPoint(majorMesh, subsequent)) / 2.0f;
                verts[5] = (getMidPoint(majorMesh, i) + getMidPoint(majorMesh, findRemainingOnSide(new int[] { i, subsequent })[0])) / 2.0f;
            }
            else
            {
                int a = (i == 6 ? 0 : 3);
                for (int k = 0; k < 3; k++)
                {
                    verts[k] = (majorMesh.vertices[k + a] + majorMesh.vertices[(k + 1) % 3 + a]) / 2.0f;
                    verts[k + 3] = (getMidPoint(majorMesh, k + a) + getMidPoint(majorMesh, (k + 1) % 3 + a)) / 2.0f;
                }

            }
        }
        return verts;

    }

    private Vector3 getMidPoint(Mesh majorMesh, int i)
    {
        i = i % 3;
        return (majorMesh.vertices[i] + majorMesh.vertices[i + 3]) / 2.0f;
    }

    private Vector3[] getCenterPoints(Mesh mesh)
    {
        Vector3[] res = new Vector3[] { Vector3.zero, Vector3.zero, Vector3.zero };
        //Debug.Log("getting centers - orig mesh has " + mesh.vertices.Length + " verts");
        res[0] = (mesh.vertices[0] + mesh.vertices[1] + mesh.vertices[2]) / 3.0f;
        res[2] = (mesh.vertices[3] + mesh.vertices[4] + mesh.vertices[5]) / 3.0f;
        res[1] = (res[0] + res[2]) / 2.0f;

        float len = (res[2] - res[0]).magnitude;//to make deviation relative to vox size
        float variationFac = 0.8f;

        /*res[1] += new Vector3((float)((Math.Sqrt(rand.NextDouble()) * 2 - 1) * len * variationFac),
            (float)((Math.Sqrt(rand.NextDouble()) * 2 - 1) * len * variationFac),
            (float)((Math.Sqrt(rand.NextDouble()) * 2 - 1) * len * variationFac));
            */

        //Debug.Log("center points: " + res[0] + " ; " + res[1] + " ; " + res[2]);

        return res;
    }

    GameObject genNewSubVoxel()
    {
        GameObject subVoxelObject = new GameObject();
        subVoxelObject.AddComponent<MeshFilter>();
        subVoxelObject.AddComponent<Voxel>();
        subVoxelObject.AddComponent<MeshCollider>();
        subVoxelObject.AddComponent<MeshRenderer>();
        //subVoxelObject.AddComponent<NetworkIdentity>();
        subVoxelObject.AddComponent<Health>().fullHealth = 10;
        subVoxelObject.tag = "TriVoxel";

        return subVoxelObject;
    }

}