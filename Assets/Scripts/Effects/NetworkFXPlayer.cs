using UnityEngine;
using UnityEngine.Networking;


public class NetworkFXPlayer : NetworkBehaviour
{
    // Can be generalized to use AAtackBehaviour
    private MagicAttack magic;

    private void Start()
    {
        magic = GetComponent<MagicAttack>();
    }

    /*
     * Playing partilce effects on all clients
     */
    public void play(bool isPlaying, int spell)
    {
        if (isLocalPlayer)
        {
            CmdPlayEffect(isPlaying, spell);
        }

        if (isPlaying)
        {
            magic.spells[spell].fx.Play();
            return;
        }

        magic.spells[spell].fx.Stop();
    }

    [Command]
    void CmdPlayEffect(bool isPlaying, int spell)
    {
        RpcPlayEffect(isPlaying, spell);
    }

    [ClientRpc]
    void RpcPlayEffect(bool isPlaying, int spell)
    {
        if (isLocalPlayer) return;

        play(isPlaying, spell);
    }
}