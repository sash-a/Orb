[System.Serializable] // Allows unity to show in inspector
public class WeaponType : Item
{
    // Default values
    public string name = "Generic gun name";
    public float damage = 10f;
    // public float envDamage = 20f; ?? 
    public float range = 100f;

    public int weaponLevel = 1;

    // isExplosive = false; float blast radius; or some implementation like this

    // These will likely be stored as a list in player
    // Store effects in here too like bullet impact gun smoke and that kinda shit
    // Impact effect and impact force and muzzle flash etc?
    // Great tutorial for effects and shit: https://www.youtube.com/watch?v=THnivyG0Mvo
}