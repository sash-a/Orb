using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HumanTeleken : NetworkBehaviour
{
    private Rigidbody rb;
    private Transform requiredPos;
    public float speed;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        GetComponent<Gravity>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer)
        {
            CmdMove();
        }
    }

    public void setUp(Transform requiredPos)
    {
        this.requiredPos = requiredPos;
    }

    [Command]
    void CmdMove()
    {
        rb.MovePosition(rb.position + (requiredPos.position - rb.position).normalized * Time.deltaTime * speed);
    }
}