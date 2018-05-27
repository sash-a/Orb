using UnityEngine;


public class Gravity : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] private float acceleration;

    // Update is called once per frame
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        acceleration = 30;
    }

    Vector3 oldPos;
    void Update()
    {
        if (!(gameObject.name == "TriVoxel"))
        {
            rb.AddForce(transform.position.normalized * acceleration);
        }
        else {
            rb.AddForce(gameObject.GetComponent<Voxel>().centreOfObject.normalized * acceleration);
            //if (transform.position == oldPos)
            if(Vector3.Distance(transform.position, oldPos)< 0.1f)
            {
                Destroy(gameObject);
            }
        }
        
        oldPos = transform.position;
    }
}
