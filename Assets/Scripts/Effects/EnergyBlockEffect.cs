using UnityEngine;
using UnityEngine.Networking;

public class EnergyBlockEffect : MonoBehaviour
{
    public Transform target;
    public float speed;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Debug.Log("Spawning energy voxel");
    }

    void Update()
    {
        rb.MovePosition(transform.position + (target.position - transform.position) * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == target.name)
        {
            // Get energy
            Debug.Log("Get energy");
            if (other.gameObject.GetComponent<Identifier>().typePrefix == Identifier.magicianType)
            {
                other.gameObject.GetComponent<MagicAttack>().getResourceManager().gainEnery(1); 
            }
            else
            {
                Debug.Log("Gunner");
                other.gameObject.GetComponent<WeaponAttack>().getResourceManager().gainEnery(1);
            }
            
            NetworkServer.Destroy(gameObject);
        }
    }
}