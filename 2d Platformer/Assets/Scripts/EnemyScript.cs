using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{

    public class EnemyScript : MonoBehaviour
    {
        float laserLength = 1f;
        public float speed;
        public int movementDirection = 1;
        private Rigidbody2D rb;
        public GameObject netIdManager;
        public NetIdManager netIdScript;
        GameObject parent;
        public bool isHost = false;
        public int sendInformation = 3;
        private Vector2 futurePosition;
        private Vector2 futurePositionCheck;
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
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
            if (isHost)
            {
                CheckPosition();
                MoveEnemy();
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

        void MoveEnemy()
        {
            float movement = movementDirection * speed;

            rb.MovePosition(new Vector2(transform.position.x + movement, transform.position.y));
        }

        void CheckPosition()
        {
            Vector3 vec = transform.position + (new Vector3(0.55f, 0, 0) * movementDirection);

            //Checking ground
            RaycastHit2D[] hit = Physics2D.RaycastAll(vec, Vector2.down, laserLength);
            Vector3 vec3 = new Vector3(0, -1 * laserLength, 0);

            Debug.DrawLine(vec, vec + vec3);
            Debug.DrawLine(transform.position, new Vector3(0, 0, 0));

            bool hittingGround = false;
            foreach (RaycastHit2D hit2d in hit)
            {
                if (hit2d.collider.tag == "Wall") hittingGround = true;
                //Hit something, print the tag of the object

            }

            //Checking front
            vec = transform.position;
            RaycastHit2D[] hit_ = Physics2D.RaycastAll(vec, Vector2.right, laserLength - 0.45f);

            vec3 = new Vector3(1 * laserLength * movementDirection, 0, 0);

            Debug.DrawLine(vec, vec + vec3);

            Debug.DrawLine(transform.position, new Vector3(0, 0, 0));
            foreach (RaycastHit2D hit2d in hit_)
            {
                if (hit2d.collider.tag == "Wall") hittingGround = false;
                //Hit something, print the tag of the object
            }

            if (hittingGround == false)
            {
                FlipDirection();
            }
        }


        void FlipDirection()
        {
            movementDirection *= -1;
        }

        void SendEnemyPosition()
        {
            netIdScript.SendPosition(parent, rb.position);
        }

        public void SetPosition(Vector2 position)
        {
            futurePosition = position;
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
    }
}