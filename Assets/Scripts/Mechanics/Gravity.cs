using UnityEngine;
using UnityEngine.Networking;

public class Gravity : MonoBehaviour
{
    private Rigidbody rb;

    private static float acceleration = 2000f;

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
        if (rb == null)
        {
            //Destroy(this);
        }
        if (!(gameObject.name.Contains("oxel")))
        {
            rb.AddForce(transform.position.normalized * acceleration * Time.deltaTime);
        }
        else
        {
            //rb.AddForce(gameObject.GetComponent<Voxel>().centreOfObject.normalized * acceleration * Time.deltaTime);

            try
            {
                rb.AddForce(gameObject.GetComponent<Voxel>().centreOfObject.normalized * acceleration * Time.deltaTime);
            }
            catch
            {
                Destroy(this);
            }

        }

        if (gameObject.name.Contains("oxel") || gameObject.tag == "MapAsset")
        {
            if (Vector3.Distance(transform.position, oldPos) < 0.1f && age > 20 && rb.velocity.magnitude < 0.1f)
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
