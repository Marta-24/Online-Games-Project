using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Scripts
{

    public class EnemyFlyScript : MonoBehaviour
    {
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
        bool isHost = false;
        public int sendInformation = 3;
        private Vector2 futurePosition;
        private Vector2 futurePositionCheck;
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            coll = GetComponent<Collider2D>();
            speed = 0.05f;

            //Get client component
            FindNetIdManager();
            parent = gameObject;
            futurePosition = new Vector2(0.0f, 0.0f);
            futurePositionCheck  = new Vector2(0.0f, 0.0f);
        }

        // Update is called once per frame
        void Update()
        {
            if (isHost == true)
            {
                CheckPosition();

                if (followPlayer == true)
                {
                    MoveTowardsPlayer(player);
                }
            }
            else if (isHost == false)
            {
                if (futurePosition != futurePositionCheck)
                {
                    rb.MovePosition(futurePosition);
                    futurePositionCheck = futurePosition;
                }
            }

            sendInformation--;
            if (sendInformation == 0)
            {
                sendInformation = 5;
                SendEnemyPosition(); //Send the position to the server every fram for the moment
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
            if (netIdManager != null)
            {
                netIdScript = netIdManager.GetComponent<NetIdManager>();
                isHost = netIdScript.CheckConnection();
            }
        }

        void SendEnemyPosition()
        {
            netIdScript.SendPosition(parent, rb.position);
        }

        public void SetPosition(Vector2 position)
        {
            futurePosition = position;
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