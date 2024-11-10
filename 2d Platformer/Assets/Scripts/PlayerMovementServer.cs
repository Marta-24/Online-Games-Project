using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    public class PlayerMovementServer : MonoBehaviour
    {
        private Rigidbody2D rb;
        private Collider2D coll;
        public LayerMask groundLayer;
        public Animator animator;
        private Vector3 initialScale;
        public GameObject objectServer;
        public ServerTCP server;
        private Vector2 futurePosition;
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            coll = GetComponent<Collider2D>();
            animator = GetComponent<Animator>();

            initialScale = transform.localScale;

            //Call server to connect
            objectServer = GameObject.Find("ServerManager");
            if (objectServer != null)
            {
                server = objectServer.GetComponent<ServerTCP>();

                if (server != null)
                {
                    Debug.Log("Server found!!!, pinging him");
                    server.ConnectToPlayer();
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            rb.MovePosition(futurePosition);
        }

        public void SetPosition(Vector2 position)
        {
            Debug.Log("trying to change position!");
            futurePosition = position;
        }
    }
}