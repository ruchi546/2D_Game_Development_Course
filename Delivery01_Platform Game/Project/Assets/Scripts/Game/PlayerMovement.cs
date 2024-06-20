using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody2D rigidBody;
    private float horizontalInput;
    private SpriteRenderer playerSprite;

    public Animator playerAnim;


    private Vector2 inputForce;
    private float minInputForce = 0.1f;

    private bool isContact;
    private LayerMask wallLayer;
    private LayerMask groundLayer;
    private float wallInputForce = 0.6f; 
    private bool isLeft;
    private bool isRight;
    private float radius = 0.3f;

    void Start()
    {
        playerSprite = transform.Find("PlayerSprite").GetComponent<SpriteRenderer>();
        rigidBody = GetComponent<Rigidbody2D>();

        wallLayer = LayerMask.GetMask("Wall");
        groundLayer = LayerMask.GetMask("Ground");

    }

    private void FixedUpdate()
    {
        isLeft = Physics2D.Raycast(transform.position, Vector2.left, radius, wallLayer | groundLayer);
        isRight = Physics2D.Raycast(transform.position, Vector2.right, radius, wallLayer | groundLayer);
        
        isContact = (isLeft && inputForce.x <= -wallInputForce) || (isRight && inputForce.x >= wallInputForce);

        if (!isContact) //check if player is not in contact with wall
        {
            rigidBody.velocity = new Vector2(horizontalInput * speed, rigidBody.velocity.y);
        }

    }

    private void Update()
    {
        if (inputForce.x > 0.1 || inputForce.x < -0.1)
        {

            playerAnim.SetBool("IsWalking", true);


        }
        else {
            playerAnim.SetBool("IsWalking", false);
        }
    }

    //new input system unity event
    public void Move(InputAction.CallbackContext context)
    {
        inputForce = context.ReadValue<Vector2>();

        
        //flip sprite
        if (inputForce.x > minInputForce)
        {
            playerSprite.flipX = false;
        }
        else if (inputForce.x < -minInputForce)
        {
            playerSprite.flipX = true;    
        }

        horizontalInput = inputForce.x;
    }
}
