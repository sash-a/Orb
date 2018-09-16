using System;
using System.Collections;
using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    public static int numArtifacts = 3;

    public enum Class
    {
        GUNNER,
        MAGICIAN,
        BOTH
    };

    public Class itemClass;

    public enum ItemType//list all weapon types after the magic types
    {
        NONE,
        DAMAGE_ARTIFACT,
        HEALER_ARTIFACT,
        TELEPATH_ARTIFACT,
        LESSER_ARTIFACT,
        EXPLOSIVE_CROSSBOW
    };

    public ItemType itemType;


    public void pickedUp()
    {
        //play some effect or something
        MapManager.manager.collectables.Remove(this);
        Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(setItem());
    }

    private IEnumerator setItem()
    {
        yield return new WaitForSecondsRealtime(2);
        if (!MapManager.manager.collectables.Contains(this)) {
            MapManager.manager.collectables.Add(this);
        }
    }
}