using UnityEngine;

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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();

        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        jumpVelocity = Mathf.Sqrt(2 * gravity * maxJumpHeight);
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
        }
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(movement.x * moveSpeed, rb.velocity.y);
    }

    bool IsGrounded()
    {
        return Physics2D.IsTouchingLayers(coll, groundLayer);
    }
}
