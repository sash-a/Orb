using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CaveManager : NetworkBehaviour
{

    static HashSet<Digger> diggers;
    static int caveNo = 1;

    static UnityEngine.Object diggerPrefab;

    static System.Random rand;

    // Use this for initialization
    void Start()
    {
        if (!isServer) { return; }
        rand = new System.Random();
        diggers = new HashSet<Digger>();
        diggerPrefab = Resources.Load("Prefabs/Digger");
        //Debug.Log("found digger : " + diggerPrefab);
    }

    public static void digCaves()
    {
        for (int i = 0; i < caveNo; i++)
        {
            GameObject digObj = (GameObject)Instantiate(diggerPrefab, new Vector3(0,0,0), Quaternion.LookRotation(new Vector3(0,0,1)));
            digObj.transform.localScale = new Vector3(1, 1, 1);
            Digger digger = digObj.GetComponent<Digger>();
            digger.init();
            digger.createEntranceAt(rand.Next(0,MapManager.neighboursMap.Count-1));
            diggers.Add(digger);
        }

    }

    static IEnumerator SmoothVoxels() {
        yield return new WaitForSeconds(1f);

        Voxel.useSmoothing = true;
        for (int i = 0; i < MapManager.mapLayers; i++)
        {
            ArrayList keys = new ArrayList();
            foreach (int n in MapManager.voxels[i].Keys) {
                keys.Add(n);
            }
            for (int j = 0; j < keys.Count; j++)
            {
                MapManager.voxels[i][(int)keys[j]].smoothBlockInPlace();
            }

        }

        for (int i = 0; i < MapManager.mapLayers; i++)
        {
            ArrayList keys = new ArrayList();
            foreach (int n in MapManager.voxels[i].Keys)
            {
                keys.Add(n);
            }
            for (int j = 0; j < keys.Count; j++)
            {
                MapManager.voxels[i][(int)keys[j]].smoothBlockInPlace();
            }

        }
    }

    public static void removeDigger(Digger d)
    {
        diggers.Remove(d);
        Destroy(d.gameObject);
        if (diggers.Count <= 0)
        {
            MapManager.manager.StartCoroutine(SmoothVoxels());
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
