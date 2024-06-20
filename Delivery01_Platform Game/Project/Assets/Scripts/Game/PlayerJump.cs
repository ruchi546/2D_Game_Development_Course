using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    [SerializeField] private float jumpForce = 7f;
    public bool doubleJump = false;
    private const int maxJumps = 2;
    private int jumpsPerformed = 0;
    [SerializeField] private float jumpCancelFactor = 0.5f;

    private bool isGrounded;
    [SerializeField] private Transform groundCheck;
    private float radius = 0.3f;
    private LayerMask groundLayer;
 
    private Rigidbody2D rigidBody;

    public Animator playerAnim;


    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        groundLayer = LayerMask.GetMask("Ground");
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, radius, groundLayer);

        //reset jumps if grounded
        if (isGrounded)
        {
            jumpsPerformed = 0;
        }

    }

    private void Update()
    {
        if (!isGrounded)
        {
            playerAnim.SetBool("IsJumping", true);
        }
        else
        {
            playerAnim.SetBool("IsJumping", false);
        }

    }

    public void Jump(InputAction.CallbackContext context)
        {
   
        //if jump button is pressed and player is grounded and double jump is enabled or player has not performed max jumps
        //then jump
        if (context.performed && (isGrounded || (doubleJump && jumpsPerformed < maxJumps))) // && rigidBody.velocity.y ><= 0 to only do the double jump when falling or rising
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce);
            jumpsPerformed++;
            
        }

        //if jump button is released and player is rising then cancel jump
        if (context.canceled && rigidBody.velocity.y > 0)
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, rigidBody.velocity.y * jumpCancelFactor);
            jumpsPerformed++;

        }
    }
}
