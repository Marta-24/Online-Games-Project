using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scripts
{

    public class BulletScript : MonoBehaviour
    {
        public float speed = 70f;
        public int lifeTime = 24 * 1000; // 24 frames * 15 seconds
        public Rigidbody2D rb;
        Collider2D coll;
        public bool isHost = false;
        public void Start_()
        {
            // Add a Rigidbody2D component and set collision detection to Continuous
            rb = GetComponent<Rigidbody2D>();
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            //rb.velocity = transform.right * speed;
            coll = GetComponent<Collider2D>();
        }

        void Update()
        {
            lifeTime--;
            if (lifeTime < 0)
            {

                //Destroy(gameObject);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            // Debug.Log("this triggered" + " " + other.gameObject.tag);
            // Destroy the bullet if it hits an enemy or goes out of bounds
            if (other.CompareTag("Wall") || other.CompareTag("Enemy")) // Change this thing later
            {
                
                if (other.CompareTag("Enemy") && isHost)
                {
                    Debug.Log("this worke!!!!!!!!!!!!!!!!!!!!!!!d");
                    GameObject obj = other.gameObject;
                    obj.GetComponent<LifeSystem>().TakeDamage(25);
                }
                //Debug.Log("destroying");
                Destroy(gameObject);
            }
        }

        public void SetDirection(int direction) // only multiply by 1 or -1!!!
        {

            rb.velocity *= transform.right * speed * direction;
        }
    }

}