using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

namespace Scripts
{

    public class EnemyFlyScript : MonoBehaviour
    {
        float laserLength = 1f;
        public float speed;
        public int movementDirection = 1;
        private int health;
        private Rigidbody2D rb;
        public GameObject netIdManager;
        public NetIdManager netIdScript;
        GameObject parent;
        private Vector2 playerPos;
        Collider2D coll;
        Vector2 player;
        bool followPlayer = false;
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            coll = GetComponent<Collider2D>();
            speed = 0.05f;
            health = 100;

            //Get client component
            FindNetIdManager();
            parent = gameObject;
        }

        // Update is called once per frame
        void Update()
        {
            CheckPosition();

            if (player != null)
            {
                MoveTowardsPlayer(player);
            }

            if (health <= 0)
            {
                Destroy(gameObject);
            }
        }

        void MoveTowardsPlayer(Vector2 pos)
        {
            Debug.Log("moving");
            Debug.Log(pos.x + ", " + pos.y);
            float divident = Mathf.Sqrt((pos.x * pos.x) + (pos.y * pos.y));
            float x = (pos.x / divident) * speed;
            float y = (pos.y / divident) * speed;
            rb.MovePosition(new Vector2(transform.position.x + x, transform.position.y + y));
        }

        void CheckPosition()
        {

        }

        public void TakeDamage(int dmg)
        {
            Debug.Log("taking damage");
            health -= dmg;

            netIdScript.TakeDamage(parent, dmg);
        }

        public void ReceiveDamage(int dmg)
        {
            health -= dmg;
        }
        void FlipDirection()
        {
            movementDirection *= -1;
        }

        public void FindNetIdManager()
        {
            netIdManager = GameObject.Find("NetIdManager");
            if (netIdManager != null) netIdScript = netIdManager.GetComponent<NetIdManager>();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log("trigger works");
            if (other.CompareTag("Player1"))
            {
                player = other.transform.position;
                followPlayer = true;
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player1"))
            {
                followPlayer = false;
            }
        }
    }
}