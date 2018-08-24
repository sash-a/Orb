using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CritterActions))]
public class CritterController : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float lookSens;
    [SerializeField] private float jumpForce;
    [SerializeField] private float runMultiplier;
    private CritterActions actions;

   static float size = 1f;

    void Start()
    {
        actions = GetComponent<CritterActions>();
        transform.localScale *= size;
    }

    void Update()
    {
        if (PlayerUI.isPaused)
        {
            actions.move(Vector3.zero);
            actions.rotate(Vector3.zero, 0);
            return;
        }

        // movement
        var xMov = Input.GetAxis("Horizontal") * transform.right;
        var yMov = Input.GetAxis("Vertical") * transform.forward;
        var velocity = (xMov + yMov).normalized * speed;

        actions.move(Input.GetKey(KeyCode.LeftShift) ? velocity * runMultiplier : velocity);

        // Rotation
        var yRot = new Vector3(0, Input.GetAxis("Mouse X"), 0) * lookSens;
        float xRot = Input.GetAxis("Mouse Y") * lookSens;

        actions.rotate(yRot, xRot);

        if (Input.GetButtonDown("Jump"))
            actions.jump(jumpForce);
    }

    IEnumerator hackPos()
    {
        yield return new WaitForSeconds(2);
        transform.position = new Vector3(0, 0, 0);
        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }
}