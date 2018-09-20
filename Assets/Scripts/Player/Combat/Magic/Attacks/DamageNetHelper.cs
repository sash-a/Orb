using UnityEngine.Networking;

namespace Player.Combat.Magic.Attacks
{
    public class DamageNetHelper : NetworkBehaviour
    {
        private MagicAttack magic;
        
        private void Start()
        {
            magic = GetComponent<MagicAttack>();
        }
        
        // Sets a spell active on the server and on all clients
        [Command]
        public void CmdSetSpellActive(bool active, int spell)
        {
            setSpellActive(active, spell);
            RpcSetSpellActive(active, spell);
        }

        [ClientRpc]
        void RpcSetSpellActive(bool active, int spell)
        {
            setSpellActive(active, spell);
        }

        private void setSpellActive(bool active, int spell)
        {
            magic.spells[spell].isActive = active;
        }
    }
}