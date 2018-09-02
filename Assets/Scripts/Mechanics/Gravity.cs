using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
public class Gravity : MonoBehaviour
{
    private Rigidbody rb;

    private static float acceleration = 4000f;//4000

    void Start()
    {
        if (gameObject.GetComponent<Rigidbody>() == null)
        {
            gameObject.AddComponent<Rigidbody>();
        }
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    Vector3 oldPos;
    int age = 0;
    void FixedUpdate()
    {
        age++;
        if (!gameObject.CompareTag("TriVoxel"))
        {
            rb.AddForce(transform.position.normalized * acceleration * Time.fixedDeltaTime, ForceMode.Acceleration);
        }
        else
        {
            try
            {
                rb.AddForce(gameObject.GetComponent<Voxel>().centreOfObject.normalized * acceleration * Time.fixedDeltaTime);
            }
            catch
            {
                Destroy(this);
            }

        }

        if (!gameObject.tag.Equals("Player"))
        {
            if (Vector3.Distance(transform.position, oldPos) < 0.25f && age > 20 && rb.velocity.magnitude < 0.18f)
            {
                try
                {
                    NetworkServer.Destroy(gameObject);
                }
                catch
                {
                    Destroy(gameObject);
                }
            }
        }

        oldPos = transform.position;
    }
}
