using UnityEngine;

namespace Scripts
{
    public class PlayerMovement : MonoBehaviour
    {
        public int netId;
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

        // server objects
        public GameObject objectUDP;
        public ServerUDP server;
        public ClientUDP client;
        public bool UDPConnection = false;
        public GameObject parent;
        public GameObject camera;
        public GameObject netIdManager;
        public NetIdManager netIdScript;
        public GameObject bulletPrefab;
        public Transform firePoint;
        public int sendInformation;
        public int movementDirection = 1;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            coll = GetComponent<Collider2D>();
            animator = GetComponent<Animator>();

            initialScale = transform.localScale;

            float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
            jumpVelocity = Mathf.Sqrt(2 * gravity * maxJumpHeight);

            //Get client component
            FindNetIdManager();

            FixCamera();

            sendInformation = 5;
        }

        void Update()
        {
            if (camera == null)
            {
                FixCamera();
            }
            // Get horizontal input for movement
            movement.x = Input.GetAxisRaw("Horizontal");
            if (movement.x > 0)
            {
                movementDirection = 1;
            }
            else if (movement.x < 0)
            {
                movementDirection = -1;
            }
            // Update animator parameter for walking
            animator.SetBool("isWalking", movement.x != 0);

            // Check for jump input and grounded state
            if (Input.GetButtonDown("Jump") && IsGrounded())
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
                //animator.SetBool("isJumping", true);
            }

            if (Input.GetButtonDown("ResetPos"))
            {
                rb.MovePosition(new Vector2(0.0f, 0.0f));
            }
            // Set isJumping to false when grounded
            if (IsGrounded())
            {
                //animator.SetBool("isJumping", false);
            }

            if (Input.GetButtonDown("j"))
            {
                FireBullet();
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

            if (objectUDP == null)
            {
                if (UDPConnection == true)
                {
                    if (server == null) FindNetIdManager();
                }
                else if (client == null) FindNetIdManager();

            }

            sendInformation--;
            if (sendInformation == 0)
            {
                sendInformation = 5;
                SendPlayerPosition(); //Send the position to the server every fram for the moment
            }

        }

        void FireBullet()
        {
            // Instantiate a bullet at the fire point
            Vector3 position_ = rb.transform.position + (new Vector3(0.6f, 0.0f, 0.0f) * movementDirection);
            GameObject bullet = Instantiate(bulletPrefab, position_, rb.transform.rotation);
            BulletScript bulletScript = bullet.GetComponent<BulletScript>();
            bulletScript.Start_();
            bulletScript.isHost = true;
            bulletScript.rb.velocity = 10f * movementDirection * transform.right;
            //bulletScript.SetDirection(movementDirection);

            //Sending server creation of bullet
            netIdScript.CreateBullet(bullet as GameObject, rb.transform.position, movementDirection);
        }

        void FixedUpdate()
        {
            //Apply horizontal movement
            rb.velocity = new Vector2(movement.x * moveSpeed, rb.velocity.y);
        }

        //Ground check using collision layers
        bool IsGrounded()
        {
            return Physics2D.IsTouchingLayers(coll, groundLayer);
        }

        void SendPlayerPosition()
        {
            netIdScript.SendPosition(parent, rb.position);
        }

        //This function is called at Start, the objective is to find the gameobject of the client or server
        public void FindNetIdManager()
        {
            netIdManager = GameObject.Find("NetIdManager");
            if (netIdManager != null) netIdScript = netIdManager.GetComponent<NetIdManager>();
        }

        public void FixCamera()
        {
            camera = GameObject.Find("Main Camera");
            if (camera == null)
            {
                Debug.Log("CAMERA NOT FOUND!!");
            }

            camera.GetComponent<CameraFollow>().ChangeTarget(parent.transform);
        }
    }
    
}