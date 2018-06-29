using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Used to orientate a gameObject with a rigidBody correctly in the circle

public class MapAsset : MonoBehaviour
{

    Rigidbody rb;
    GameObject asset;
    Voxel voxel;

    static float heightVariation = 0.7f;
    static float widthVariation = 1f;
    static float rotateVariation = 5f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Vector3 forward = getFoward();
        rb.MoveRotation(Quaternion.LookRotation(forward, -transform.position.normalized));
        

        StartCoroutine(SetParent());
    }

    public static MapAsset createAsset(Voxel vox)
    {
        GameObject ass = null;
        if (vox.layer == 0)
        {
            ass = Instantiate(Resources.Load<GameObject>("Prefabs/Map/MapAssets/Palm_Tree"), vox.worldCentreOfObject, Quaternion.identity);

        }
        ass.GetComponent<MapAsset>().voxel = vox;
        return ass.GetComponent<MapAsset>();
    }

    IEnumerator SetParent()
    {
        yield return new WaitForSeconds(0.7f);
        MapManager man = MapManager.manager;
        GameObject map = man.Map;
        transform.parent = map.transform.GetChild(2);

        Vector3 forward = getFoward();
        Vector3 right = Vector3.Cross(forward, -transform.position).normalized;

        transform.position += forward * (float)(voxel.rand.NextDouble() - 0.5f) + right * (float)(voxel.rand.NextDouble() - 0.5f);
        float width = (float)(voxel.rand.NextDouble()* widthVariation + widthVariation*0.5f);
        transform.localScale = new Vector3(transform.localScale.x * width, transform.localScale.y * (float)(voxel.rand.NextDouble() * heightVariation + widthVariation * 0.5f), transform.localScale.z * width);
        transform.Rotate(new Vector3((float)(voxel.rand.NextDouble() * rotateVariation + rotateVariation * 0.5f), (float)(voxel.rand.NextDouble() * rotateVariation + rotateVariation * 0.5f), (float)(voxel.rand.NextDouble() * rotateVariation + rotateVariation * 0.5f)));
    }

    public Vector3 getFoward()
    {
        var up = -transform.position.normalized;
        var foward = Vector3.Cross(up, transform.right);

        if (Vector3.Dot(foward, transform.forward) < 0)
        {
            foward *= -1;
        }

        return foward.normalized;
    }

}