using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AreaOfEffectDamage : AAttackBehaviour
{
    public float damage;

    public float radius;
    public float duration;

    public bool damageGunners = false;
    public bool damageMagicians = false;
    public bool damageVoxels = true;
    bool showVisualisation = false;

    public float damagePeriod = 0.2f;//period of time between damaging elements
    float timeSinceDamage = 0;
    float timeSinceStart = 0;

    GameObject visualRepresentation;

    int voxelIterations = 5;
    int iterationsLeft;

    // Use this for initialization
    void Start()
    {
        if (showVisualisation)
        {
            visualRepresentation = (GameObject)Instantiate<UnityEngine.Object>(Resources.Load<UnityEngine.Object>("Prefabs/AOEIndicator"));
            visualRepresentation.transform.position = transform.position;
            visualRepresentation.transform.localScale = new Vector3(2 * radius, 2 * radius, 2 * radius);
            visualRepresentation.transform.parent = transform;
        }

        if (damageVoxels)
        {
            iterationsLeft = voxelIterations;
            StartCoroutine(damageVoxelsInArea());
        }
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceDamage += Time.deltaTime;
        timeSinceStart += Time.deltaTime;

        if (timeSinceStart >= duration)
        {
            if (visualRepresentation != null) {
                Destroy(visualRepresentation);
            }
            if (iterationsLeft <= 0)
            {
                Destroy(gameObject);
            }//else - wait for the voxel iterations to finish
        }
        else {
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

                damagePeriod += 0.002f;
                timeSinceDamage = 0;
            }
        }

        
    }

    int batchCount = 0;
    int batchSize = 4;

    private IEnumerator damageVoxelsInArea()
    {
        float time = Time.time;
        //Debug.Log("starting new damage voxels iter");

        RaycastHit[] colliders = Physics.SphereCastAll(transform.position, radius/2, transform.position, 0, mask, QueryTriggerInteraction.UseGlobal);

        //loop through every item in blast radius
        List<Voxel> damagedVoxels = new List<Voxel>(); ;

        foreach (RaycastHit nearbyObject in colliders)
        {

            if (nearbyObject.collider.tag == "TriVoxel")
            {
                //Debug.Log("found vox in AOE : " + nearbyObject.collider.gameObject + " damaging by: " + damage*2);
                Voxel v = nearbyObject.collider.gameObject.GetComponent<Voxel>();
                damagedVoxels.Add(v);
                v.shatterCap = 1;
            }
        }

        int length = damagedVoxels.Count;
        for (int i = 0; i < length; i++)
        {
            batchCount++;
            //CmdVoxelDamaged(nearbyObject.collider.gameObject, damage * 2);
            if (damagedVoxels[i] != null && damagedVoxels[i].gameObject != null)
            {
                damagedVoxels[i].gameObject.GetComponent<NetHealth>().RpcDamage(damage * 1000);
            }
            if (batchCount >= batchSize)
            {
                batchCount = 0;
                yield return new WaitForEndOfFrame();
            }
        }

        //Debug.Log("finished one aoe vox iter ("+(voxelIterations - iterationsLeft)+") in " + (Time.time - time) + " seconds");

        if (iterationsLeft > 0)
        {
            iterationsLeft--;
            yield return new WaitForEndOfFrame();
            StartCoroutine(damageVoxelsInArea());
        }
        else {
            if (timeSinceStart >= duration)
            {
                if (iterationsLeft <= 0)
                {
                    Destroy(gameObject);
                }//else - wait for the voxel iterations to finish
            }
        }
    }


    /// <summary>
    /// Notifies the server that a voxel has been hit
    /// </summary>
    /// <param name="go">GameObject that was hit</param>
    /// <param name="damage">Damage to be done to the voxel</param>
    [Command]
    public override void CmdVoxelDamaged(GameObject go, float damage)
    {
        Debug.Log("cmd vox damaged called");
        if (go == null)
        {
            Debug.LogError("trying to damage null voxel");
            return;
        }

        var health = go.GetComponent<NetHealth>();
        if (health == null)
        {
            Debug.LogError("Voxel did not have health component");
            return;
        }
        Debug.Log("damaging voxel health");
        health.RpcDamage(damage);
    }

    void damageTeam(Team team)
    {
        foreach (PlayerController player in team.players)
        {
            double dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist <= radius)
            {
                Debug.Log("found player in area of effect");
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
