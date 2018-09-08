using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaOfEffectDamage : AAttackBehaviour
{
    public float damage;

    public float radius;
    public float duration;

    public bool damageGunners=false;
    public bool damageMagicians=false;
    public bool damageVoxels=true;

    public float damagePeriod = 0.2f;//period of time between damaging elements
    float timeSinceDamage = 0;
    float timeSinceStart = 0;

    GameObject visualRepresentation;

    // Use this for initialization
    void Start()
    {
        visualRepresentation = (GameObject)Instantiate<UnityEngine.Object>(Resources.Load<UnityEngine.Object>("Prefabs/AOEIndicator"));
        visualRepresentation.transform.position = transform.position;
        visualRepresentation.transform.localScale = new Vector3(2 * radius, 2 * radius, 2 * radius);
        visualRepresentation.transform.parent = transform;
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceDamage += Time.deltaTime;
        timeSinceStart += Time.deltaTime;

        if (timeSinceStart >= duration) {
            Destroy(gameObject);
        }

        if (timeSinceDamage >= damagePeriod)
        {
            if (damageGunners)
            {
                damageTeam(TeamManager.singleton.gunners);
            }
            if (damageMagicians)
            {
                damageTeam(TeamManager.singleton.magicians);
            }
            if (damageVoxels) {
                damageVoxelsInArea();
            }
            damagePeriod += 0.02f;
        }
    }

    private void damageVoxelsInArea()
    {
        RaycastHit[] colliders = Physics.SphereCastAll(transform.position, radius, transform.position, 0, mask, QueryTriggerInteraction.UseGlobal);

        //loop through every item in blast radius
        foreach (RaycastHit nearbyObject in colliders)
        {

            if (nearbyObject.collider.tag == "TriVoxel")
            {
                //adjust damage so that the further away something is from grenade the less damage it does
                if (nearbyObject.distance >= 0 && nearbyObject.distance < 1)
                {
                    CmdVoxelDamaged(nearbyObject.collider.gameObject, damage*2); // weapontype.envDamage?
                }
                else
                {
                    CmdVoxelDamaged(nearbyObject.collider.gameObject, damage * 2 / nearbyObject.distance); // weapontype.envDamage?
                }


                if (nearbyObject.collider.GetComponent<NetHealth>().getHealth() <= damage)
                {
                    //commenting out for now...spawning it on nearbyObject spawns it in the sky for some reason
                    //CmdVoxelDestructionEffect(nearbyObject.point, nearbyObject.normal);
                }
            }
        }
    }

    void damageTeam(Team team)
    {
        foreach (PlayerController player in team.players)
        {
            double dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist <= radius)
            {
                player.GetComponent<NetHealth>().RpcDamage(damage);
            }
        }
    }

    public override void attack()
    {
        throw new NotImplementedException();
    }

    public override void endAttack()
    {
        throw new NotImplementedException();
    }

    public override void secondaryAttack()
    {
        throw new NotImplementedException();
    }

    public override void endSecondaryAttack()
    {
        throw new NotImplementedException();
    }
}
