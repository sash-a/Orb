using UnityEngine;

public class AttackComponentsContainer : MonoBehaviour
{
    // AAtack behaiviour/mutual attributes
    public Camera cam;
    public LayerMask mask;
    
    // All weapon attack specific attributes
    public ParticleSystem PistolMuzzleFlash;
    public ParticleSystem AssaultMuzzleFlash;
    public ParticleSystem ShotgunMuzzleFlash;
    public ParticleSystem SniperMuzzleFlash;

    public GameObject hitEffect;
    public GameObject VoxelDestroyEffect;
    public GameObject explosionEffect;

    public GameObject grenadeSpawn;
    public GameObject grenadePrefab;
    public float throwForce = 40;

    // All magic specific attributes
    public GameObject shield;
    public GameObject telekenObjectPos;
}