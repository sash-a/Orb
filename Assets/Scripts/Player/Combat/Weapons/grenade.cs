using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(ResourceManager))]
public class grenade : AAttackBehaviour {

    public float delay = 3f;
    public float blastRadius = 5f;
    public float damage = 10f;

    float countdown;

    bool hasExploded = false;

    public GameObject explosionEffect;
    public GameObject hitGround;

    // Use this for initialization
    void Start ()
    {
        countdown = delay;
	}
	
	// Update is called once per frame
	void Update ()
    {
        countdown -= Time.deltaTime;
        if(countdown <= 0f && !hasExploded)
        {
            attack();
            hasExploded = true;
        }
	}

    //explodes
    [Client]
    public override void attack()
    {
        if (isServer)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            NetworkServer.Spawn(explosion);
        }

        //all items in blast radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, blastRadius);

        foreach (Collider nearbyObject in colliders)
        {
            if (nearbyObject.tag == PLAYER_TAG)
                CmdPlayerAttacked(nearbyObject.name, damage);

            // Only add this if we are sure that voxels are getting damaged by guns otherwise check gun type before damaging
            if (nearbyObject.tag == VOXEL_TAG)
            {
                CmdVoxelDamaged(nearbyObject.gameObject, damage); // weapontype.envDamage?

                if (nearbyObject.GetComponent<NetHealth>().getHealth() <= 0)
                {
                    if (isServer)
                    {
                        GameObject hitParticle = Instantiate(hitGround, nearbyObject.transform.position, nearbyObject.transform.rotation);
                        NetworkServer.Spawn(hitParticle);
                    }
                }
            }
        }

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
