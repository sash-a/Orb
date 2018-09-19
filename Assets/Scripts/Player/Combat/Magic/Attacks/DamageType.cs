using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class DamageType : SpellType
{
    public static readonly string HEAD = "Head";

    public Transform rightHand;

    public List<string> damageNames;
    public List<float> damageValues;

    public GameObject damageText;


    public override void attack()
    {
        if (!magic.isClient) return;

        RaycastHit hitFromCam;
        if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out hitFromCam, 1000, mask)) return;


        // Checking range, need high distance in raycast to orient effect
        if (Mathf.Abs(Vector3.Distance(hitFromCam.point, player.position)) > range) return;

        // Shoot ray from hand to hit position
        RaycastHit hitFromHand;
        if (!Physics.Linecast(rightHand.position, hitFromCam.point + 2 * cam.transform.forward, out hitFromHand, mask))
            return; // This should never return

        var rootTransform = hitFromHand.collider.transform.root;

        if (hitFromHand.collider.CompareTag(SHIELD_TAG))
        {
            createDamageText(hitFromHand.transform, getDamageValue(SHIELD_TAG), true, false, true);
            magic.CmdShieldHit(hitFromHand.collider.gameObject, getDamageValue(SHIELD_TAG));
        }
        else if (rootTransform.CompareTag(GUNNER_TAG))
        {
            Debug.Log("Hit head: " + (hitFromHand.collider.name == HEAD));
            float damage = hitFromHand.collider.name == HEAD
                ? getDamageValue(GUNNER_TAG) * getDamageValue(HEAD)
                : getDamageValue(GUNNER_TAG);

            createDamageText(rootTransform, damage, false, hitFromHand.collider.name == HEAD);
            magic.CmdPlayerAttacked(rootTransform.GetComponent<Identifier>().id, damage);
        }
        else if (rootTransform.CompareTag(MAGICIAN_TAG))
        {
            createDamageText(rootTransform, getDamageValue(MAGICIAN_TAG), true);
            magic.CmdPlayerAttacked(rootTransform.GetComponent<Identifier>().id, getDamageValue(MAGICIAN_TAG));
        }
        else if (hitFromHand.collider.CompareTag(VOXEL_TAG))
        {
            magic.CmdVoxelDamaged(hitFromHand.collider.gameObject, getDamageValue(VOXEL_TAG));
        }
    }

    public override void startAttack()
    {
        if (!magic.isClient) return;

        isActive = true;
        fx.Play(); // This needs to be an rpc
    }

    public override void endAttack()
    {
        if (!magic.isClient) return;

        isActive = false;
        fx.Stop();
    }


    private void createDamageText(Transform hit, float damage, bool isHealing = false, bool isHeadshot = false,
        bool isShield = false)
    {
        float posUp = isShield ? 10 : 15;
        Object.Instantiate
        (
            damageText,
            hit.position + hit.up * posUp,
            hit.rotation
        ).GetComponent<TextDamageIndicator>().setUp((int) damage, isHealing, isHeadshot);
    }

    private float getDamageValue(string name)
    {
        int index = damageNames.FindIndex(x => x == name);
        return damageValues[index];
    }
}