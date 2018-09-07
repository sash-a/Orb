using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    public static int numArtifacts = 3;

    public enum Class
    {
        GUNNER,
        MAGICIAN
    };

    public Class itemClass;

    public enum ItemType//list all weapon types after the magic types
    {
        NONE,
        DAMAGE_ARTIFACT,
        HEALER_ARTIFACT,
        TELEPATH_ARTIFACT,
        EXPLOSIVE_CROSSBOW
    };

    public ItemType itemType;


    public void pickedUp()
    {
        //play some effect or something
        MapManager.manager.collectables.Remove(this);
        Destroy(gameObject);
    }
}