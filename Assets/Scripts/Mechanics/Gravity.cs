using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
public class Gravity : MonoBehaviour
{
    private Rigidbody rb;

    private static float acceleration = 4000f;//4000
    public bool inSphere = true;//if false - uses traditional down vector as grav dir

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
            if (inSphere)
            {
                rb.AddForce(getDownDir() * acceleration * Time.fixedDeltaTime, ForceMode.Acceleration);
            }
            else
            {
                rb.AddForce(getDownDir() * acceleration * Time.fixedDeltaTime, ForceMode.Acceleration);
            }
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
                if(gameObject.GetComponent<NetworkIdentity>() != null)
                {
                    NetworkServer.Destroy(gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }

        oldPos = transform.position;
    }

    public Vector3 getDownDir()
    {
        if (inSphere)
        {
            return transform.position.normalized;
        }
        else
        {
            return Vector3.down;
        }
    }
}
