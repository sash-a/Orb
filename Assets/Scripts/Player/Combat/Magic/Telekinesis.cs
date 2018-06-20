using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Voxel))]
public class Telekenises : MonoBehaviour
{
    [SerializeField] private string casterID;
    [SerializeField] private Transform requiredPos;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Voxel vox;

    private const float voxelSpeed = 3;
    private const float gunnerSpeed = 1;
    private const float beastSpeed = 0.5f;
    private float speed;

    public static int VOXEL = 0;
    public static int GUNNER = 1;
    public static int BEAST = 2;

    private bool hasReleased;
    private float throwForce;


    private GameObject test;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        vox = GetComponent<Voxel>();

        // Testing
        test = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube), vox.worldCentreOfObject,
            Quaternion.identity);
        test.AddComponent<Rigidbody>().useGravity = false;

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
    }

    void Update()
    {
        if (hasReleased) return;

        // TODO need some way to track voxel world pos and update it (transform pos is 0,0,0)
//        rb.MovePosition(vox.worldCentreOfObject +
//                        (requiredPos.position - vox.worldCentreOfObject) * Time.deltaTime * speed);


        test.GetComponent<Rigidbody>().MovePosition(test.transform.position +
                                                    (requiredPos.position - test.transform.position) *
                                                    Time.deltaTime * speed);
    }

    public void throwObject(Vector3 direction)
    {
        hasReleased = true;
//        rb.AddForce(direction * 200f, ForceMode.Impulse);
        test.GetComponent<Rigidbody>().AddForce(direction * 200f, ForceMode.Impulse);
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

        vox.GetComponent<NetHealth>().RpcDamage(10); // kill the voxel
        // TODO FX
    }
}