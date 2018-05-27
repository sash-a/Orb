using UnityEngine;

[RequireComponent(typeof(NetPlayerActions))]
public class NetPlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float lookSens = 2f;
    private NetPlayerActions actions;

    void Start()
    {
        actions = GetComponent<NetPlayerActions>();
    }

    void Update()
    {
        // movement
        var xMov = Input.GetAxis("Horizontal") * transform.right;
        var yMov = Input.GetAxis("Vertical") * transform.forward;
        var velocity = (xMov + yMov).normalized * speed;

        actions.move(velocity);

        // Rotation
        var yRot = new Vector3(0, Input.GetAxis("Mouse X"), 0) * lookSens;
        float xRot = Input.GetAxis("Mouse Y") * lookSens;

        actions.rotate(yRot, xRot);
    }
}