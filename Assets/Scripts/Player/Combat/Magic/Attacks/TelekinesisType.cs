using UnityEngine;
using UnityEngine.Networking;

namespace Player.Combat.Magic.Attacks
{
    [System.Serializable]
    public class TelekinesisType : SpellType
    {
        [HideInInspector] public GameObject currentVoxel;
        [SerializeField] private TelekinesisNetHelper netHelper;

        public Transform targetPos;

        public float colisionDamage;


        public override void attack()
        {
        }

        public override void startAttack()
        {
            // Shoot ray from the camera to center of screen
            RaycastHit hitFromCam;
            if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out hitFromCam, range, mask))
                return;


            if (!hitFromCam.collider.CompareTag(VOXEL_TAG) &&
                !hitFromCam.collider.transform.root.CompareTag(PLAYER_TAG)) return;


            if (hitFromCam.collider.CompareTag(VOXEL_TAG))
            {
                Voxel voxel = hitFromCam.collider.gameObject.GetComponent<Voxel>();
                if (voxel == null)
                {
                    return;
                }

                isActive = true;

                if (voxel.shatterLevel >= 1)
                    netHelper.CmdVoxelTeleken(voxel.columnID, voxel.layer, voxel.subVoxelID);
                else
                    netHelper.CmdVoxelTeleken(voxel.columnID, voxel.layer, "NOTSUB");
            }
        }

        public override void endAttack()
        {
            isActive = false;
            netHelper.CmdEndTeleken();
        }

        public override void upgrade(PickUpItem.ItemType artifactType)
        {
            if (artifactType != PickUpItem.ItemType.LESSER_ARTIFACT ||
                artifactType != PickUpItem.ItemType.TELEPATH_ARTIFACT)
                return;

            hasArtifact = artifactType == PickUpItem.ItemType.TELEPATH_ARTIFACT;
            colisionDamage *= 1.5f;
        }

        public override void downgrade()
        {
            if (!hasArtifact)
                return;

            hasArtifact = false;
            colisionDamage /= 1.5f;
        }

        /// <summary>
        /// Adds a rigidbody and enables the network transform of a given voxel.
        /// Creates the necessary voxels below the current telekenetic one.
        /// </summary>
        public void prepVoxel(int col, int layer, string subID, string playerID)
        {
            if (subID == "NOTSUB")
            {
                currentVoxel = MapManager.manager.voxels[layer][col].gameObject;
            }
            else
            {
                currentVoxel = MapManager.manager.getSubVoxelAt(layer, col, subID).gameObject;
            }

            var voxel = currentVoxel.GetComponent<Voxel>();
            // Creating the voxels below
            //Debug.Log("Show neighbours " + (subID == "NOTSUB"));
            voxel.showNeighbours(subID == "NOTSUB");

            // Setting up rigidbody
            // Needs to be true to work with a rigid body
            voxel.gameObject.GetComponent<MeshCollider>().convex = true; // Still throws error sometimes

            if (voxel.gameObject.GetComponent<Rigidbody>() == null)
            {
                var rb = voxel.gameObject.AddComponent<Rigidbody>();
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.useGravity = false;
            }

            // Enabling network transform
            currentVoxel.GetComponent<NetworkTransform>().enabled = true;

            voxel.transform.parent = MapManager.manager.Map.transform;
            voxel.gameObject.name = playerID + "_teleken_voxel";

            var tele = currentVoxel.GetComponent<Telekinesis>();
            tele.enabled = true;
            tele.setUp(targetPos, Telekinesis.VOXEL, magic.GetComponent<Identifier>().id, magic.isServer);
        }
    }
}