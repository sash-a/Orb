using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerActions))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float lookSens;
    [SerializeField] private float jumpForce;
    [SerializeField] private float runMultiplier;
    private PlayerActions actions;

    //Animation
    public Animator animator;
    private bool isMoving = false;
    private bool isWalking = false;
    private bool isRunning = false;
    

    void Start()
    {
        actions = GetComponent<PlayerActions>();
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

        //Animation:
        if (velocity != Vector3.zero)
        {
            isMoving = true;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                isRunning = true;
                isWalking = false;
            }
            else
            {
                isRunning = false;
                isWalking = true;
            }
        }
        else
        {
            isMoving = false;
            isWalking = false;
        }
        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isRunning);
        animator.SetFloat("xMove", Input.GetAxis("Horizontal"));
        animator.SetFloat("yMove", Input.GetAxis("Vertical"));

        //Debug.Log("xMov: " + Input.GetAxis("Horizontal") + " yMov: " + Input.GetAxis("Vertical"));

    }

    IEnumerator hackPos()
    {
        yield return new WaitForSeconds(2);
        transform.position = new Vector3(0, 0, 0);
        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

}