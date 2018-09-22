using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Voxel))]
public class Telekinesis : MonoBehaviour
{
    [SerializeField] private string casterID;
    [SerializeField] private Transform requiredPos;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Voxel vox;
    private Transform voxPos;

    [SerializeField] private float damage;

    private const float voxelSpeed = 10;
    private const float gunnerSpeed = 2;
    private const float beastSpeed = 0;
    private float speed;

    public static int VOXEL = 0;
    public static int GUNNER = 1;

    private bool hasReleased;
    private float throwForce;

    [SerializeField] public ParticleSystem teleEffect;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        vox = GetComponent<Voxel>();
        voxPos = vox.voxelPositon.transform;

        //        var shape = teleEffect.GetComponent<ParticleSystem>().shape;
        //        shape.mesh = vox.filter.mesh;

        hasReleased = false;
        throwForce = 150;
        damage = 90;
    }

    public void setUp(Transform stuckTo, int typeHit, string casterID, bool isServer)
    {
        vox = GetComponent<Voxel>();

        requiredPos = stuckTo;
        this.casterID = casterID;

        if (typeHit == VOXEL) speed = voxelSpeed;
        else if (typeHit == GUNNER) speed = gunnerSpeed;
        else speed = beastSpeed;

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("Waiting for rigidbody to spawn");
            StartCoroutine(getRB());
        }

        //        teleEffect.gameObject.SetActive(true);
        //        teleEffect.Play();
    }

    void Update()
    {
        if (hasReleased || rb == null) return; // Timer + explode

        // Stops the voxel from "bouncing" when player not looking around
        if ((voxPos.position - requiredPos.position).magnitude < 1) return;

        rb.MovePosition(rb.position + (requiredPos.position - voxPos.position).normalized * Time.deltaTime * speed);

        // teleEffect.transform.position = voxPos.position;
    }

    public void throwObject(Vector3 direction)
    {
        hasReleased = true;
        gameObject.AddComponent<Gravity>();
        rb.AddForce(direction * throwForce, ForceMode.Impulse);
    }


    private void OnCollisionEnter(Collision hit)
    {
        if (!hasReleased) return;

        var casterAttack = GameManager.getObject(casterID).GetComponent<MagicAttack>();

        if (hit.collider.CompareTag(SpellType.PLAYER_TAG))
            casterAttack.CmdPlayerAttacked(hit.collider.name, damage);
        else if (hit.collider.CompareTag(SpellType.VOXEL_TAG))
            casterAttack.CmdVoxelDamaged(hit.collider.gameObject, damage);
        else if (hit.collider.CompareTag(SpellType.SHIELD))
            casterAttack.CmdShieldHit(hit.collider.gameObject, 0);

        NetworkServer.Destroy(gameObject);
    }

    IEnumerator getRB()
    {
        yield return new WaitForSeconds(0.5f);
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Could not find rigidbody on client");
        }
    }
}