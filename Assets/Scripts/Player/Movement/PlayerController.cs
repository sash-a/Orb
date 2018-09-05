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
    private float xMoveOld = 0;
    private float yMoveOld = 0;
    private float xMove = 0;
    private float yMove = 0;
    private float interpSpeed = 0.25f;
    
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

        // Movement
        var xMov = Input.GetAxis("Horizontal") * transform.right;
        var yMov = Input.GetAxis("Vertical") * transform.forward;
        var velocity = (xMov + yMov).normalized * speed;

        if (!animator.GetBool("isReloading"))
        {
            actions.move(Input.GetKey(KeyCode.LeftShift) ? velocity * runMultiplier : velocity);
        }
        else
        {
            actions.move(Input.GetKey(KeyCode.LeftShift) ? velocity * 1 : velocity);
        }
            
        
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
        yMove = Input.GetAxis("Vertical");
        xMove = Input.GetAxis("Horizontal");

        if (!animator.GetBool("isReloading"))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                yMove *= 2;
                xMove *= 2;
            }
        }
        
        yMoveOld = yMoveOld + (yMove - yMoveOld) * (interpSpeed /( Mathf.Round(yMove) ==0 ? 1 :(float)Mathf.Abs(Mathf.Round(yMove))));
        xMoveOld = xMoveOld + (xMove - xMoveOld) * (interpSpeed / (Mathf.Round(xMove) == 0 ? 1 : (float)Mathf.Abs(Mathf.Round(xMove))));

        if (Input.GetButtonDown("Jump"))
        {
            animator.SetTrigger("jump");
        }

        animator.SetBool("isMoving", isMoving);
        animator.SetFloat("xMove", xMoveOld);
        animator.SetFloat("yMove", yMoveOld);
    }

    IEnumerator hackPos()
    {
        yield return new WaitForSeconds(2);
        transform.position = new Vector3(0, 0, 0);
        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

}