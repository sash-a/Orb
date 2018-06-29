using UnityEngine;


public class Gravity : MonoBehaviour
{
    private Rigidbody rb;

    private static float acceleration = 900f;

    // Update is called once per frame
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    Vector3 oldPos;
    void Update()
    {
        if (!gameObject.name.Contains("oxel"))
        {
            rb.AddForce(transform.position.normalized * acceleration * Time.deltaTime);
        }
        else {
            rb.AddForce(gameObject.GetComponent<Voxel>().centreOfObject.normalized * acceleration*Time.deltaTime);
            //if (transform.position == oldPos)
            if(Vector3.Distance(transform.position, oldPos)< 0.1f)
            {
                Destroy(gameObject);
            }
        }
        
        oldPos = transform.position;
    }
}
