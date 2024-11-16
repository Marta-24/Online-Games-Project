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
        public GameObject objectTCP;
        public ClientTCP client;
        public ServerTCP server;
        public bool TCPConnection;
        private Vector2 futurePosition;
        public GameObject parent;
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            coll = GetComponent<Collider2D>();
            animator = GetComponent<Animator>();

            initialScale = transform.localScale;

            //Call server to connect
            FindTCP();
        }

        // Update is called once per frame
        void Update()
        {
            rb.MovePosition(futurePosition);


            if (objectTCP == null)
            {
                FindTCP();
            }
        }

        public void SetPosition(Vector2 position)
        {
            Debug.Log("trying to change position!");
            futurePosition = position;
        }

        public void FindTCP()
        {
            if (objectTCP == null) objectTCP = GameObject.Find("ClientManager");
            if (objectTCP == null) objectTCP = GameObject.Find("ServerManager");
            if (server == null)
            {
                server = objectTCP.GetComponent<ServerTCP>();

                if (server != null)
                {
                    Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                    server.ConnectToPlayer(parent);
                    TCPConnection = true;
                }
            }
            if (client == null)
            {
                client = objectTCP.GetComponent<ClientTCP>();

                if (client != null)
                {
                    client.ConnectToPlayer();
                    TCPConnection = false;
                }
            }
        }
    }
}

