using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb2d;
    private Animator anim;
    private PlayerControls playerControls;

    [Header("Movement Variables")]
    Vector2 movementInput;

    public bool isGrounded;
    public bool jump;
    public bool facingRight;
    private bool playerFalling;

    public float runSpeed = 3f;
    public float horizontalMove = 0f;
    public float jumpForce = 3f;

    public float playerFallingGravity = 1;
    public float playerDownfall = 1;
    public float downfallLimit = 1;
    public float hangTime = 0.2f;
    private float hangCounter;
    private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    [Header("Raycast Variables")]
    public LayerMask groundLayerMask;
    public float groundRayCastLength;
    public Vector3 groundRaycastOffset;
    public Transform leftFoot;
    public Transform rightFoot;

    //enable player input systems
    //needs to be public to be referenced in other scripts. Allows the player movement to be locked and unlocked 
    public void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();

            //set input to be the vector 2 movementInput
            playerControls.PlayerMovement.Move.performed += i => movementInput = i.ReadValue<Vector2>();
            playerControls.PlayerMovement.Move.canceled += i => movementInput = i.ReadValue<Vector2>();

            //set input to be the jump function
            playerControls.PlayerMovement.Jump.performed += ctx => Jump();
            playerControls.PlayerMovement.Jump.canceled += i => Jump();

        }

        playerControls.Enable();
    }

    //disable player input systems
    //needs to be public to be refered to in other scripts. Allows the player movement to be locked and unlocked 
    public void OnDisable()
    {
        playerControls.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    //movement checks in here
    private void FixedUpdate()
    {
        if (playerFalling)
        {
            if ((rb2d.velocity.y < 0) && (rb2d.velocity.y > -downfallLimit))
            {
                rb2d.AddForce(new Vector2(0, -playerDownfall), ForceMode2D.Impulse);
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        movement();
        groundCheck();

        Vector2 current_velocity = rb2d.velocity;

        //checking if the player is moving left/right
        if (horizontalMove != 0)
        {
            current_velocity.x = horizontalMove * runSpeed;

            if (current_velocity.x < 0)
            {
                transform.eulerAngles = new Vector3(0, -180, 0);
                //this is used for the boost paint powerup, inside launchPadScript
                facingRight = false;

            }
            else if (current_velocity.x > 0)
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
                //this is used for the boost paint powerup, inside launchPadScript
                facingRight = true;


            }
            horizontalMove += 0.01f;
        }
        else
        {
            horizontalMove = 0f;
            current_velocity.x = 0;
        }

        //set the rb velocity to be whatever the player's velocity is now after moving
        rb2d.velocity = new Vector2(current_velocity.x, current_velocity.y);

        //jump check
        if ((jumpBufferCounter > 0f && hangCounter > 0f))
        {
            //Jump();
            jumpBufferCounter = 0f;
        }
    }

    private void jumpBuffer()
    {
        if (rb2d.velocity.y < 0)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }
    private void movement()
    {
        horizontalMove = movementInput.x;
    }

    private void Jump()
    {
        if (isGrounded)
        {
            if (anim != null)
            {
                //playerJump animation
                anim.SetTrigger("isJumping");
                anim.SetBool("isFalling", false);
                rb2d.velocity = new Vector2(rb2d.velocity.x, jumpForce);
                isGrounded = false;
                hangCounter = 0;
            }
        }
    }

    //Function for checking collisions using raycasts on each of the player's feet
    private void groundCheck()
    {
        RaycastHit2D leftFootHit = Physics2D.Raycast(leftFoot.position, Vector2.down, groundRayCastLength, groundLayerMask);
        RaycastHit2D rightFootHit = Physics2D.Raycast(rightFoot.position, Vector2.down, groundRayCastLength, groundLayerMask);
        if (leftFootHit.collider != null || rightFootHit.collider != null)
        {
            Debug.Log("ground found by raycasts");
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    //function to draw Gizmos 
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(leftFoot.position, new Vector2(leftFoot.position.x, leftFoot.position.y - groundRayCastLength));
        Gizmos.DrawLine(rightFoot.position, new Vector2(rightFoot.position.x, rightFoot.position.y - groundRayCastLength));
       
    }

}
