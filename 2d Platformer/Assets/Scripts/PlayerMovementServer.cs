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
        public ClientUDP client;
        public ServerUDP server;
        public bool TCPConnection;
        private Vector2 futurePosition;
        private Vector2 futurePositionCheck;
        public GameObject parent;
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            coll = GetComponent<Collider2D>();
            animator = GetComponent<Animator>();

            initialScale = transform.localScale;

            futurePosition = new Vector2(0.0f, 0.0f);
            futurePositionCheck  = new Vector2(0.0f, 0.0f);
            //Call server to connect
            FindTCP();
        }

        // Update is called once per frame
        void Update()
        {
            if (futurePosition != futurePositionCheck)
            {
                Debug.Log("Changing player position");
                rb.MovePosition(futurePosition);
                futurePositionCheck = futurePosition;
            }



            if (objectTCP == null)
            {
                FindTCP();
            }
        }

        public void SetPosition(Vector2 position)
        {
            futurePosition = position;
        }

        public void FindTCP()
        {
            if (objectTCP == null) objectTCP = GameObject.Find("ClientManager");
            if (objectTCP == null) objectTCP = GameObject.Find("ServerManager");
            if (server == null)
            {
                server = objectTCP.GetComponent<ServerUDP>();

                if (server != null)
                {
                    Debug.Log("AAAA");
                    server.ConnectToPlayer(parent);
                    TCPConnection = true;
                }
            }
            if (client == null)
            {
                client = objectTCP.GetComponent<ClientUDP>();

                if (client != null)
                {
                    client.ConnectToPlayer(parent);
                    TCPConnection = false;
                }
            }
        }
    }
}

