using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ForcePush : NetworkBehaviour
{
    [SerializeField] private Vector3 casterPos;
    [SerializeField] private float force;
    private Collider coll;

    private void Start()
    {
        coll = GetComponent<Collider>();
    }

    public void setUp(Vector3 pos, float force)
    {
        coll.enabled = true;
        casterPos = pos;
        this.force = force;
    }

    public void tearDown()
    {
        coll.enabled = false;
        force = 0;
    }

    [Client]
    void OnCollisionEnter(Collision other)
    {
        Debug.Log("Collision Enter");
        if (other.collider.CompareTag(AAttackBehaviour.PLAYER_TAG))
        {
            CmdPush(other.gameObject.GetComponent<Identifier>().id);
            Debug.Log("Force hit " + other.gameObject.GetComponent<Identifier>().id);
        }
    }

    [Client]
    void OnCollisionStay(Collision other)
    {
        Debug.Log("Collision stay");
        if (other.collider.CompareTag(AAttackBehaviour.PLAYER_TAG))
        {
            CmdPush(other.gameObject.GetComponent<Identifier>().id);
            Debug.Log("Force hit " + other.gameObject.GetComponent<Identifier>().id);
        }
    }

    [Client]
    void OnCollisionExit(Collision other)
    {
        Debug.Log("Collision stay");
        if (other.collider.CompareTag(AAttackBehaviour.PLAYER_TAG))
        {
            CmdPush(other.gameObject.GetComponent<Identifier>().id);
            Debug.Log("Force hit " + other.gameObject.GetComponent<Identifier>().id);
        }
    }

    [Command]
    void CmdPush(string id)
    {
        RpcPush(id);
    }

    [ClientRpc]
    void RpcPush(string id)
    {
        var direction = GameManager.getObject(id).transform.position - casterPos;
        GameManager.getObject(id).gameObject.GetComponent<Rigidbody>()
            .AddForce(direction.normalized * force * (1 / direction.sqrMagnitude), ForceMode.Impulse);
    }
}