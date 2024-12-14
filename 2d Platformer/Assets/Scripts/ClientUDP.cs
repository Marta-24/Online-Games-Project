using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
//using UnityEngine.tvOS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using UnityEngine.SceneManagement;
namespace Scripts
{
    public class ClientUDP : MonoBehaviour
    {
        Socket server;
        bool goToSampleScene = false;
        static MemoryStream stream;
        public GameObject objectPlayer;
        public PlayerMovementServer playerScript;
        IPEndPoint ipep;
        public GameObject textPanel;
        string text;
        public GameObject enemyPrefab;
        public GameObject netManager;
        public NetIdManager netIdScript;
        public GameObject sceneManager;
        public SceneLoader sceneLoader;
        void Start()
        {
            sceneLoader = sceneManager.GetComponent<SceneLoader>();
        }

        void Update()
        {
            if (netManager == null) // Even though this is called at start, we had some problems and for now this is a way to make sure we find the server or client
            {
                FindNetIdManager();
            }
        }

        public void StartClient()
        {
            //127.0.0.1
            text = textPanel.GetComponent<TMP_InputField>().text;
            Thread connect = new Thread(Connect);
            connect.Start();

        }

        void Connect()
        {


            ipep = new IPEndPoint(IPAddress.Parse(text), 9090);

            server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);


            Thread sendThread = new Thread(Send);
            sendThread.Start();
            //SceneManager.LoadScene("WaitingRoom");

        }

        private void ReceiveJob()
        {
            while (true)
            {
                int rec = ReceiveUDP();

                if (rec == 0)
                {
                    break;
                }

            }
        }

        private int ReceiveUDP()
        {
            byte[] data = new byte[2048];
  

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint _remote = (EndPoint)(sender);

            int rec = server.ReceiveFrom(data, ref _remote);

            if (rec == 0)
            {
                Debug.Log("breaking receive");
                return 0;
            }

            // receive the orders here
            int com = deserializeJson(data);
            return rec;
        }
        void Send()
        {
            byte[] data = new byte[2048];
     
            data = Encoding.ASCII.GetBytes("userName");
            server.SendTo(data, 0, SocketFlags.None, ipep);
            Receive();
        }

        void Receive()
        {
            byte[] data = new byte[2048];
            int recv = 0;

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);

            recv = server.ReceiveFrom(data, data.Length, SocketFlags.None, ref Remote);

            deserializeJson(data);
            Thread receiveThread = new Thread(ReceiveJob);
            receiveThread.Start();
        }

        int deserializeJson(byte[] data_)
        {
           
            MemoryStream stream = new MemoryStream();
            stream.Write(data_, 0, data_.Length);

            //var command = new ReplicationMessage();
            var com = new Command();

            
            BinaryReader reader = new BinaryReader(stream);
            stream.Seek(0, SeekOrigin.Begin);

            string json01 = reader.ReadString();
            string json02;

            com = JsonUtility.FromJson<Command>(json01);

            Debug.Log("this is it" + com.action);
            
            // DO Diferent actions with the data received
            if (com.action == UdpActions_.Position)
            {
                json02 = reader.ReadString();
                Debug.Log("changing player position");
                Vector2 vec = JsonUtility.FromJson<Vector2>(json02);
                //changing player position
                SetPosition(com.netID, vec);
            }
            else if(com.action == UdpActions_.Create)
            {
                Debug.Log("creating something");
                json02 = reader.ReadString();
                gameObjectType type = JsonUtility.FromJson<gameObjectType>(json02);
                json02 = reader.ReadString();
                Vector2 vec = JsonUtility.FromJson<Vector2>(json02);
                netIdScript.StackObject(com.netID, type, vec);
            }
            else if(com.action == UdpActions_.StartGame)
            {
       
                sceneLoader.LoadScene01Client_();
            }

            //if (com.action == (int)UdpActions_.Create)
            //{
            //    // Assume position fields are sent as integers
            //    var field = com.fieldList[0] as FieldDoubleInt;
            //    Vector3 position = new Vector3(field.a, field.b, 0);
//
            //    Debug.Log($"Spawning enemy at position {position} on client.");
            //    Instantiate(enemyPrefab, position, Quaternion.identity);
            //}


            //if (command.action == 1)
            //{
            //    Debug.Log("Servername received");
            //}
            //else if (command.action == 2)
            //{
            //    t = JsonUtility.FromJson<testClass>(json02);
            //
            //    // trying to set position
            //    SetPlayerPosition(t);
            //}
            return 1;
        }

        void SetPosition(int netId, Vector2 pos)
        {
            if(netManager != null) netIdScript.SetPosition(netId, pos);
            //if (playerScript == null)
            //{
//
            //}
            //else if (playerScript != null)
            //{
            //    playerScript.SetPosition(pos);
            //}
        }

        public void SendPosition(int netId, Vector2 position)
        {
            Command com = new Command(netId, UdpActions_.Position);

            string json01 = JsonUtility.ToJson(com);
            string json02 = JsonUtility.ToJson(position);

            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(json01);
            writer.Write(json02);

            byte[] data = new byte[2048];
            data = stream.ToArray();

            server.SendTo(data, SocketFlags.None, ipep); //this should work;
        }

        public void FindNetIdManager()
        {
            netManager = GameObject.Find("NetIdManager");
            if (netManager != null) netIdScript = netManager.GetComponent<NetIdManager>();
        }

        public void ConnectToPlayer(GameObject gameObject)
        {
            if (playerScript == null)
            {
                objectPlayer = gameObject;
                if (objectPlayer != null)
                {
                    playerScript = objectPlayer.GetComponent<PlayerMovementServer>();

                    if (playerScript != null)
                    {
     
                    }
                }
            }
        }
    }
}