using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(ResourceManager))]
public class WeaponAttack : AAttackBehaviour
{
    [SerializeField] private WeaponType WeaponType;
    [SerializeField] private GameObject gunModel;

    private ResourceManager resourceManager;

    void Start()
    {
        // gunModel = WeaponType.gunModel??
        resourceManager = GetComponent<ResourceManager>();
    }

    [Client]
    public override void attack()
    {
        // Using ammo
        resourceManager.usePrimary(1);
        // Add in relevent effects and shit here
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, WeaponType.range, mask))
        {
            if (hit.collider.tag == PLAYER_TAG)
                CmdPlayerAttacked(hit.collider.name, WeaponType.damage);
            // Only add this if we are sure that voxels are getting damaged by guns otherwise check gun type before damaging
            if (hit.collider.tag == VOXEL_TAG)
                CmdVoxelDamaged(hit.collider.gameObject, WeaponType.damage); // weapontype.envDamage?
        }

        // For explosives someother kind of range check will be required and a grenade/explosive gameObject instead of raycasting
    }
}