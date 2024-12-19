using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

namespace Scripts
{
    
    public class EnemyScript : MonoBehaviour
    {
        float laserLength = 1f;
        public float speed;
        public int movementDirection = 1;
        private int health;
        private Rigidbody2D rb;
        public GameObject netIdManager;
        public NetIdManager netIdScript;
        GameObject parent;
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
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
            MoveEnemy();

            if (health <= 0)
            {
                Destroy(gameObject);
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
    }
}