using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BulletScript : MonoBehaviour
{
    public float speed = 70f;
    public int lifeTime = 24 * 1000; // 24 frames * 15 seconds
    Rigidbody2D rb;
    Collider2D coll;
    void Start()
    {
        // Add a Rigidbody2D component and set collision detection to Continuous
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.velocity = transform.right * speed;
        coll = GetComponent<Collider2D>();
    }

    void Update()
    {
        lifeTime--;
        if (lifeTime < 0)
        {
            Debug.Log("Deleting bullet because of time");
            //Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("this triggered");
        // Destroy the bullet if it hits an enemy or goes out of bounds
        if (other.CompareTag("Wall") || other.CompareTag("Player1") || other.CompareTag("Enemy")) // Change this thing later
        {
            //Destroy(gameObject);
        }
    }
}

