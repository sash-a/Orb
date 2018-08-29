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
    //NB variables for walking/running Blend animations
    private float xMove;
    private float yMove;
    

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

        //Animation
        MovementAnimation(velocity);

    }

    void MovementAnimation(Vector3 velocity)
    {
        //Animation:
        xMove = Input.GetAxis("Horizontal");
        yMove = Input.GetAxis("Vertical");

        if (velocity != Vector3.zero)
        {
            isMoving = true;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                //When running xMove a yMove are equal to 2 or -2 (equal to 1 or -1 when walking)
                if (Input.GetAxis("Horizontal") > 0) //RIGHT
                {
                    xMove += 1.0f;
                }
                else if (Input.GetAxis("Horizontal") < 0) //LEFT
                {
                    xMove -= 1.0f;
                }

                if (Input.GetAxis("Vertical") > 0) //FORWARD
                {
                    yMove += 1.0f;
                }
                else
                {
                    yMove -= 1.0f;
                }
                
            }
        }
        else
        {
            isMoving = false;
        }

        //Jumping: set MoveY = 3?

        animator.SetBool("isMoving", isMoving);
        animator.SetFloat("xMove", xMove);
        animator.SetFloat("yMove", yMove);
    }

    IEnumerator hackPos()
    {
        yield return new WaitForSeconds(2);
        transform.position = new Vector3(0, 0, 0);
        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

}