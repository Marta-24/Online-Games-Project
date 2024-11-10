using UnityEngine;

namespace Scripts
{
    public class PlayerMovement : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float maxJumpHeight = 2f;

        private float jumpVelocity;
        private Rigidbody2D rb;
        private Vector2 movement;

        public LayerMask groundLayer;
        private bool isGrounded;
        private Collider2D coll;

        // Animator for handling animations
        public Animator animator;

        // Store the initial scale to preserve size
        private Vector3 initialScale;

        // Client objects
        public GameObject objectClient;
        public ClientTCP client;
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            coll = GetComponent<Collider2D>();
            animator = GetComponent<Animator>();

            initialScale = transform.localScale;

            float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
            jumpVelocity = Mathf.Sqrt(2 * gravity * maxJumpHeight);

            //Get client component
            objectClient = GameObject.Find("ClientManager");
            if (objectClient != null)
            {
                client = objectClient.GetComponent<ClientTCP>();

                if (client != null)
                {
                    Debug.Log("Client found!!!");
                }
            }

        }


        void Update()
        {
            // Get horizontal input for movement
            movement.x = Input.GetAxisRaw("Horizontal");

            // Update animator parameter for walking
            animator.SetBool("isWalking", movement.x != 0);

            // Check for jump input and grounded state
            if (Input.GetButtonDown("Jump") && IsGrounded())
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
                animator.SetBool("isJumping", true);
            }

            // Set isJumping to false when grounded
            if (IsGrounded())
            {
                animator.SetBool("isJumping", false);
            }

            // Flip the character's sprite based on movement direction, preserving the initial scale
            if (movement.x < 0)
            {
                transform.localScale = new Vector3(-Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
            }
            else if (movement.x > 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(initialScale.x), initialScale.y, initialScale.z);
            }


            SendPosition(); //Send the position to the server every fram for the moment
        }

        void FixedUpdate()
        {
            // Apply horizontal movement
            rb.velocity = new Vector2(movement.x * moveSpeed, rb.velocity.y);
        }

        // Ground check using collision layers
        bool IsGrounded()
        {
            return Physics2D.IsTouchingLayers(coll, groundLayer);
        }

        void SendPosition()
        {
        
			Debug.Log(rb.position);

            client.SendPosition(rb.position);
        }
    }
}