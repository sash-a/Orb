using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MagicEffects : NetworkBehaviour
{
    // Digger tool
//    [SyncVar(hook = "onDig")] private bool isDigging;
//
//    // Damage
//    [SyncVar(hook = "onDamage")] private bool isDamaging;

    [SerializeField] private ParticleSystem attackEffect;
    [SerializeField] private ParticleSystem diggerEffect;

    void Start()
    {
    }

    void Update()
    {
//        if (isDigging)
//        {
//            RaycastHit hit;
//            if (!Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, attackStats.telekenRange,
//                mask))
//                return;
//        }
    }
}