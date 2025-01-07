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
        bool goToScene1 = false;
        static MemoryStream stream;
        public GameObject objectPlayer;
        public PlayerMovementCopy playerScript;
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

            if (goToScene1)
            {
                sceneLoader.LoadScene01Client();
                goToScene1 = false;
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

            ActionType actionType;

            BinaryReader reader = new BinaryReader(stream);
            stream.Seek(0, SeekOrigin.Begin);

            string json01 = reader.ReadString();
            string json02 = reader.ReadString();

            actionType = JsonUtility.FromJson<ActionType>(json01);

            SendAction(actionType, json02);
            return 1;
        }

        void SetPosition(int netId, Vector2 pos)
        {
            if (netManager != null)
            {
                netIdScript.SetPosition(netId, pos);
            }
        }

        public void SendAction(ActionType actionType, string str)
        {
            if (actionType == ActionType.Position)
            {
                MovementPacket packet = JsonUtility.FromJson<MovementPacket>(str);

                // Setting position by netId
                SetPosition(packet.netId, packet.position);
            }
            else if (actionType == ActionType.Create)
            {
                CreatePacket packet = JsonUtility.FromJson<CreatePacket>(str);
                netIdScript.StackObject(packet.netId, packet.objType, packet.position, packet.direction);
            }
            else if (actionType == ActionType.Damage)
            {
                IntPacket packet = JsonUtility.FromJson<IntPacket>(str);
                netIdScript.GiveDamage(packet.netId, packet.a);
            }
            else if (actionType == ActionType.StartGame)
            {
                goToScene1 = true;
            }
        }

        public void SendPosition(int netId, Vector2 position)
        {
            MovementPacket packet = new MovementPacket(netId, position);
            string json01 = JsonUtility.ToJson(packet);
            Debug.Log("sending player pos");
            SendString(json01, ActionType.Position);
        }

        public void SendCreateObject(int netId, GameObjectType type, Vector2 pos, Vector2 direction)
        {
            CreatePacket packet = new CreatePacket(netId, pos, direction, type);

            string json01 = JsonUtility.ToJson(packet);
            SendString(json01, ActionType.Create);
        }

        public void SendDamage(int netId, int health)
        {
            IntPacket packet = new IntPacket(netId, health);

            string json01 = JsonUtility.ToJson(packet);
            SendString(json01, ActionType.Damage);
        }

        public void SendString(string str, ActionType type)
        {
            string json02 = JsonUtility.ToJson(type);
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(json02);
            writer.Write(str);

            byte[] data = new byte[2048];
            data = stream.ToArray();

            server.SendTo(data, SocketFlags.None, ipep);
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
                    playerScript = objectPlayer.GetComponent<PlayerMovementCopy>();

                    if (playerScript != null)
                    {

                    }
                }
            }
        }
    }
}