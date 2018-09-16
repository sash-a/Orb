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
        if (rb == null || target == null) return;

        rb.MovePosition(transform.position +
                        (target.position + target.up * 4 - transform.position) * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        var rootTransform = other.transform.root;
        if (rootTransform.name == target.name)
        {
            // Get energy
            Debug.Log("Get energy");
            if (rootTransform.GetComponent<Identifier>().typePrefix == Identifier.magicianType)
            {
                rootTransform.GetComponent<MagicAttack>().getResourceManager().gainEnery(1);
            }
            else
            {
                rootTransform.GetComponent<WeaponAttack>().getResourceManager().gainEnery(1);
            }

            NetworkServer.Destroy(gameObject);
        }
    }
}