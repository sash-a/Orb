using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
public class Gravity : MonoBehaviour
{
    private Rigidbody rb;

    private static float acceleration = 4000f;//4000

    // Update is called once per frame
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
    void Update()
    {
        age++;
        if (!(gameObject.name.Contains("oxel")))
        {
            rb.AddForce(transform.position.normalized * acceleration * Time.deltaTime);
        }
        else
        {
            try
            {
                rb.AddForce(gameObject.GetComponent<Voxel>().centreOfObject.normalized * acceleration * Time.deltaTime);
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
