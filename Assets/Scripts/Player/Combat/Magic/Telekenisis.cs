using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Voxel))]
public class Telekenisis : MonoBehaviour
{
    [SerializeField] private string casterID;
    [SerializeField] private Transform requiredPos;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Voxel vox;

    private const float voxelSpeed = 5;
    private const float gunnerSpeed = 2;
    private const float beastSpeed = 0;
    private float speed;

    public static int VOXEL = 0;
    public static int GUNNER = 1;
    public static int BEAST = 2;

    private bool hasReleased;
    private float throwForce;

    [SerializeField] public ParticleSystem teleEffect;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        vox = GetComponent<Voxel>();

        var shape = teleEffect.GetComponent<ParticleSystem>().shape;
        shape.mesh = vox.filter.mesh;        
        
        hasReleased = false;
        throwForce = 150;
    }

    public void setUp(Transform stuckTo, int typeHit, string casterID)
    {        
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
        
        teleEffect.gameObject.SetActive(true);
        teleEffect.Play();
    }

    void Update()
    {
        if (hasReleased || rb == null) return; // Timer + explode

        var oldPos = new Vector3(rb.position.x, rb.position.y, rb.position.z);
        rb.MovePosition(rb.position +
                        (requiredPos.position - vox.worldCentreOfObject).normalized * Time.deltaTime * speed);
        
        var diff = rb.position - oldPos;
        vox.worldCentreOfObject += diff; // Updating the voxels position in world co-ords
        
        teleEffect.transform.position = vox.worldCentreOfObject;
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

        // TODO tweak numbers
        if (hit.collider.CompareTag(AAttackBehaviour.PLAYER_TAG))
            casterAttack.CmdPlayerAttacked(hit.collider.name, 20);
        else if (hit.collider.CompareTag(AAttackBehaviour.VOXEL_TAG))
            casterAttack.CmdVoxelDamaged(hit.collider.gameObject, 2);
        else if (hit.collider.CompareTag("Shield")) // TODO constant
            casterAttack.CmdShieldHit(hit.collider.gameObject, 75);

        // TODO FX
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