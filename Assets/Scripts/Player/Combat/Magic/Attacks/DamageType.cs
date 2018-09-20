using System.Collections.Generic;
using UnityEngine;

namespace Player.Combat.Magic.Attacks
{
    [System.Serializable]
    public class DamageType : SpellType
    {
        public static readonly string HEAD = "Head";

        public List<string> damageNames;
        public List<float> damageValues;

        public GameObject damageText;

        [SerializeField] private DamageNetHelper netHelper;
        
        //FX
        [SerializeField] private NetworkFXPlayer fxPlayer;
        [SerializeField] private EnergyBlockEffectSpawner energyBlockEffectSpawner;
        [SerializeField] private DestructionEffectSpawner destructionEffectSpawner;

        public override void attack()
        {
            if (!magic.isClient) return;

            RaycastHit hitFromCam;
            if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out hitFromCam, 1000, mask)) return;


            // Checking range, need high distance in raycast to orient effect
            if (Mathf.Abs(Vector3.Distance(hitFromCam.point, player.position)) > range) return;

            // Shoot ray from hand to hit position
            RaycastHit hitFromHand;
            if (!Physics.Linecast(magic.rightHand.position, hitFromCam.point + 2 * cam.transform.forward, out hitFromHand,
                mask))
                return; // This should never return

            var rootTransform = hitFromHand.collider.transform.root;
            var hitTransform = hitFromHand.transform;

            if (hitTransform.name.ToLower().Contains(SHIELD))
            {
                createDamageText(hitTransform, getDamageValue(SHIELD), true, false, true);
                magic.CmdShieldHit(hitTransform.gameObject, getDamageValue(SHIELD));
            }
            else if (rootTransform.name.ToLower().Contains(GUNNER))
            {
                float damage = hitTransform.name == HEAD
                    ? getDamageValue(GUNNER) * getDamageValue(HEAD)
                    : getDamageValue(GUNNER);

                createDamageText(rootTransform, damage, false, hitTransform.name == HEAD);
                magic.CmdPlayerAttacked(rootTransform.GetComponent<Identifier>().id, damage);
            }
            else if (rootTransform.name.ToLower().Contains(MAGICIAN))
            {
                createDamageText(rootTransform, getDamageValue(MAGICIAN), true);
                magic.CmdPlayerAttacked(rootTransform.GetComponent<Identifier>().id, getDamageValue(MAGICIAN));
            }
            else if (hitTransform.name.ToLower().Contains(VOXEL))
            {
                var voxel = hitTransform.GetComponent<Voxel>();
                if (voxel.hasEnergy && name == DIGGER_TYPE)
                    energyBlockEffectSpawner.spawnBlock();

                if (!voxel.hasEnergy)
                    destructionEffectSpawner.play(hitFromHand.point, voxel);


                magic.CmdVoxelDamaged(hitTransform.gameObject, getDamageValue(VOXEL));
            }
        }

        public override void startAttack()
        {
            if (!magic.isClient) return;

            isActive = true;
            netHelper.CmdSetSpellActive(true, equippedIndex);
            fxPlayer.play(true, equippedIndex);
        }

        public override void endAttack()
        {
            if (!magic.isClient) return;

            isActive = false;
            netHelper.CmdSetSpellActive(false, equippedIndex);
            fxPlayer.play(false, equippedIndex);
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
}