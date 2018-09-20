using UnityEngine.Networking;

namespace Player.Combat.Magic.Attacks
{
    public class TelekinesisNetHelper : NetworkBehaviour
    {
        private MagicAttack magic;
        private TelekinesisType telekinesis;

        private void Start()
        {
            magic = GetComponent<MagicAttack>();
            telekinesis = magic.telekin;
        }

        /// <summary>
        /// Allows the player to control a voxel
        /// </summary>
        [Command]
        public void CmdVoxelTeleken(int col, int layer, string subID)
        {
            if (subID == "NOTSUB")
            {
                telekinesis.currentVoxel = MapManager.manager.voxels[layer][col].gameObject;
            }
            else
            {
                telekinesis.currentVoxel = MapManager.manager.getSubVoxelAt(layer, col, subID).gameObject;
            }

            RpcPrepVoxel(col, layer, subID, GetComponent<Identifier>().id);
        }


        /// <summary>
        /// Adds and enables the necessarry components to the voxels on every client
        /// </summary>
        /// <param name="col"></param>
        /// <param name="layer"></param>
        /// <param name="subID"></param>
        /// <param name="playerID"></param>
        [ClientRpc]
        private void RpcPrepVoxel(int col, int layer, string subID, string playerID)
        {
            telekinesis.prepVoxel(col, layer, subID, playerID);
        }

        [Command]
        public void CmdEndTeleken()
        {
            if (telekinesis.currentVoxel != null)
            {
                telekinesis.currentVoxel.GetComponent<Telekinesis>().throwObject(telekinesis.cam.transform.forward);
            }
        }
    }
}