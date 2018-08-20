using UnityEngine;
using UnityEngine.Networking;

//[RequireComponent(typeof(ResourceManager))] //not sure if this is neccesary
public class grenade : AAttackBehaviour
{
    public float delay = 3f;
    public float blastRadius = 5f;
    public float damage = 10f;
    public float envDamage = 10f;

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

    //[Command]
    //private void CmdVoxelDestructionEffect(Vector3 position, Vector3 normal)
    //{
    //    GameObject VoxelParticle = Instantiate(VoxelDestroyEffect, position,
    //                            Quaternion.LookRotation(normal));
    //    NetworkServer.Spawn(VoxelParticle);
    //}

    //explodes
    [Client]
    public override void attack()
    {

        //Explosion effect
        CmdExplosionFX();

        //all items in blast radius
        RaycastHit[] colliders = Physics.SphereCastAll(transform.position, blastRadius, transform.position, 0, mask, QueryTriggerInteraction.UseGlobal);

        //loop through every item in blast radius
        foreach (RaycastHit nearbyObject in colliders)
        {

            if (nearbyObject.collider.tag == PLAYER_TAG)
            {
                if (nearbyObject.distance >= 0 && nearbyObject.distance < 1)
                {
                    CmdPlayerAttacked(nearbyObject.collider.name, damage);
                }
                else
                {
                    CmdPlayerAttacked(nearbyObject.collider.name, damage / nearbyObject.distance);
                }
            }


            if (nearbyObject.collider.tag == VOXEL_TAG)
            {
                //adjust damage so that the further away something is from grenade the less damage it does
                if (nearbyObject.distance >= 0 && nearbyObject.distance < 1)
                {
                    CmdVoxelDamaged(nearbyObject.collider.gameObject, envDamage); // weapontype.envDamage?
                }
                else
                {
                    CmdVoxelDamaged(nearbyObject.collider.gameObject, envDamage / nearbyObject.distance); // weapontype.envDamage?
                }


                if (nearbyObject.collider.GetComponent<NetHealth>().getHealth() <= damage)
                {
                    //commenting out for now...spawning it on nearbyObject spawns it in the sky for some reason
                    //CmdVoxelDestructionEffect(nearbyObject.point, nearbyObject.normal);
                }
            }
        }

        //destroy grenade when finished exploding
        Destroy(gameObject);

        //------------------------------------ OLD CODE (JUST IN CASE)
        //all items in blast radius
        //Collider[] colliders = Physics.OverlapSphere(transform.position, blastRadius);

        //foreach (Collider nearbyObject in colliders)
        //{
        //    float distance = Vector3.Distance(transform.position, nearbyObject.transform.position);

        //    if (nearbyObject.tag == PLAYER_TAG)
        //        CmdPlayerAttacked(nearbyObject.name, damage);

        //    if (nearbyObject.tag == VOXEL_TAG)
        //    {
        //        //adjust damage so that the further away something is from grenade the less damage it does
        //        if (distance >= 0 && distance < 1)
        //        {
        //            CmdVoxelDamaged(nearbyObject.gameObject, damage); // weapontype.envDamage?
        //        }
        //        else
        //        {
        //            CmdVoxelDamaged(nearbyObject.gameObject, damage/distance); // weapontype.envDamage?
        //        }


        //        if (nearbyObject.GetComponent<NetHealth>().getHealth() <= damage)
        //        {
        //            //spawning on grenade for now...spawning it on nearbyObject spawns it in the sky for some reason
        //            CmdVoxelDestructionEffect(transform.position, transform.position.normalized);
        //        }
        //    }
        //}
        //-------------------------------------------
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