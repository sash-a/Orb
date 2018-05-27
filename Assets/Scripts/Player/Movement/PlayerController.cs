using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerActions))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float lookSens = 8f;
    private PlayerActions actions;

    void Start()
    {
        actions = GetComponent<PlayerActions>();
        //StartCoroutine(hackPos());
    }

    void Update()
    {
        // movement
        var xMov = Input.GetAxis("Horizontal") * transform.right;
        var yMov = Input.GetAxis("Vertical") * transform.forward;
        var velocity = (xMov + yMov).normalized * speed;

        actions.move(Input.GetKey(KeyCode.LeftShift) ? velocity * 3f : velocity);

        // Rotation
        var yRot = new Vector3(0, Input.GetAxis("Mouse X"), 0) * lookSens;
        float xRot = Input.GetAxis("Mouse Y") * lookSens;

        actions.rotate(yRot, xRot);

        if (Input.GetButtonDown("Jump"))
            actions.jump();
    }

    IEnumerator hackPos()
    {
        yield return new WaitForSeconds(5);
        transform.position = new Vector3(1, 0, 0);
    }
}