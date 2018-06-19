using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(ResourceManager))]
public class grenade : AAttackBehaviour
{
    public float delay = 3f;
    public float blastRadius = 5f;
    public float damage = 10f;

    float countdown;

    bool hasExploded = false;

    public GameObject explosionEffect;
    public GameObject VoxelDestroyEffect;

    // Use this for initialization
    void Start()
    {
        countdown = delay;
    }

    // Update is called once per frame
    void Update()
    {
        countdown -= Time.deltaTime;
        if (countdown <= 0f && !hasExploded)
        {
            Debug.Log("Boom!");
            attack();
            hasExploded = true;
        }
    }

    [Command]
    private void CmdExplosionFX()
    {
        GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
        NetworkServer.Spawn(explosion);
    }

    [Command]
    private void CmdVoxelDestructionEffect(Vector3 position, Vector3 normal)
    {
        GameObject VoxelParticle = Instantiate(VoxelDestroyEffect, position,
                                Quaternion.LookRotation(normal));
        NetworkServer.Spawn(VoxelParticle);
    }

    //explodes
    [Client]
    public override void attack() //need to adjust damage so that the further away something is from grenade the less damage it does
    {
        //Explosion effect
        CmdExplosionFX();
        //all items in blast radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, blastRadius);

        foreach (Collider nearbyObject in colliders)
        {
            if (nearbyObject.tag == PLAYER_TAG)
                CmdPlayerAttacked(nearbyObject.name, damage);

            if (nearbyObject.tag == VOXEL_TAG)
            {
                CmdVoxelDamaged(nearbyObject.gameObject, damage); // weapontype.envDamage?
                
                if (nearbyObject.GetComponent<NetHealth>().getHealth() <= damage)
                {
                    //spawning on grenade for now...spawning it on nearbyObject spawns it in the sky for some reason
                    CmdVoxelDestructionEffect(transform.position, transform.position.normalized);
                }
            }
        }
        //destroy grenade when finished exploding
        Destroy(gameObject);
    }

    public override void endAttack()
    {
        throw new System.NotImplementedException();
    }

    public override void secondaryAttack()
    {
        throw new System.NotImplementedException();
    }

    public override void endSecondaryAttack()
    {
        throw new System.NotImplementedException();
    }
}