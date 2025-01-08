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

            //Get client component
            FindNetIdManager();
            parent = gameObject;
        }

        // Update is called once per frame
        void Update()
        {
            CheckPosition();

            if (followPlayer == true)
            {
                MoveTowardsPlayer(player);
            }
        }

        void MoveTowardsPlayer(Vector2 pos)
        {
            Vector2 vector = new Vector2(-rb.position.x + pos.x, -rb.position.y + pos.y);
            float divident = Mathf.Sqrt((vector.x * vector.x) + (vector.y * vector.y));
            float x = (vector.x / divident) * speed;
            float y = (vector.y / divident) * speed;
            rb.MovePosition(new Vector2(transform.position.x + x, transform.position.y + y));
        }

        void CheckPosition()
        {

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

        void OnTriggerStay2D(Collider2D other)
        {
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