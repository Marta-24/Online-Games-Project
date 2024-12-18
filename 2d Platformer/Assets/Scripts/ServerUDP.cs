using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;


namespace Scripts
{
    public enum UdpActions_
    {
        Hello = 0,
        Position = 1,
        Create = 2,
        StartGame
    }


    public class Command
    {
        public int netID;
        public UdpActions_ action;
        public Command()
        {

        }
        public Command(int netID, UdpActions_ action)
        {
            this.netID = netID;
            this.action = action;
        }
    }


    public class UserUDP
    {
        public EndPoint endPoint;
        public String name;
        public UserUDP(EndPoint endPoint, String name)
        {
            this.endPoint = endPoint;
            this.name = name;
        }
    }

    public class StringJson
    {
        public string value;
        public StringJson(string value)
        {
            this.value = value;
        }
    }
    public class ServerUDP : MonoBehaviour
    {

        private AutoResetEvent _waitHandle = new AutoResetEvent(false);
        Thread mainThread = null;
        private List<UserUDP> _clientEndPoints = new List<UserUDP>();
        private List<Thread> _clientEndPointsThread = new List<Thread>();
        public GameObject objectPlayer;
        public PlayerMovementServer playerScript;
        Socket socket;
        UserUDP user;
        public GameObject enemyPrefab;
        public GameObject netManager;
        public NetIdManager netIdScript;

        // Function called with a button to start the udp server
        public void StartServer()
        {
            Debug.Log("Initializing udp server");

            IPEndPoint localEp = new IPEndPoint(IPAddress.Any, 9090);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.Bind(localEp);

            Thread newConnection = new Thread(Receive);
            newConnection.Start();
        }

         void Update()
        {
            if (netManager == null) // Even though this is called at start, we had some problems and for now this is a way to make sure we find the server or client
            {
                FindNetIdManager();
            }
        }

        void Receive()
        {
            int recv;
            byte[] _data = new byte[2048];

            while (true)
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);


                recv = socket.ReceiveFrom(_data, ref Remote);



                //Creating user
                UserUDP user_ = new UserUDP(Remote, Encoding.ASCII.GetString(_data, 0, recv));
                user = user_;
                SendHello(Remote);

                Thread clientThread = new Thread(() => ReceiveJob(user_));
                clientThread.Start();
                _clientEndPointsThread.Add(clientThread);
            }
        }

        public void StartGame()
        {
            Command com = new Command(0, UdpActions_.StartGame);
            string json01 = JsonUtility.ToJson(com);

            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(json01);
            byte[] data = new byte[2048];
            data = stream.ToArray();

            socket.SendTo(data, SocketFlags.None, user.endPoint);
        }

        void SendHello(EndPoint Remote)
        {
            string str = "serverName";

            Command com = new Command(0, UdpActions_.Hello);


            string json01 = JsonUtility.ToJson(com);
            string json02 = JsonUtility.ToJson(str);

            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(json01);
            writer.Write(json02);


            byte[] data = new byte[2048];
            data = stream.ToArray();




            socket.SendTo(data, data.Length, SocketFlags.None, Remote);
        }

        void ReceiveJob(UserUDP user)
        {
            while (true)
            {
                int rec = ReceiveUDP(user);

                if (rec == 0)
                {
                    _clientEndPoints.Remove(user);
                    break;
                }
            }
        }

        int ReceiveUDP(UserUDP user)
        {
            byte[] data = new byte[2048];

            int rec = socket.ReceiveFrom(data, ref user.endPoint); // This works because there is only one connection
            Debug.Log("Received message!!!" + rec);

            if (rec == 0)
            {
                return 0;
            }

            int com = DeserializeJson(data);

            return rec;
        }

        //Right now this function only decodes the position of a player, in a future this function will either redistribute the calls from the client or be one little part of various functions to deserialize
        public int DeserializeJson(byte[] data_)
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
            else if (com.action == UdpActions_.Create)
            {
                Debug.Log("creating something");
                json02 = reader.ReadString();
                gameObjectType type = JsonUtility.FromJson<gameObjectType>(json02);
                json02 = reader.ReadString();
                Vector2 vec = JsonUtility.FromJson<Vector2>(json02);
                netIdScript.StackObject(com.netID, type, vec);
            }
            

            return 1;
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

            socket.SendTo(data, SocketFlags.None, user.endPoint);
        }

        public void SendCreateObject(int netId, gameObjectType type, Vector2 pos)
        {
            Command com = new Command(netId, UdpActions_.Create);

            string json01 = JsonUtility.ToJson(com);
            string json02 = JsonUtility.ToJson(type);
            string json03 = JsonUtility.ToJson(pos);

            Debug.Log("sending comand create type " + ((int)type));

            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(json01);
            writer.Write(json02);
            writer.Write(json03);

            byte[] data = new byte[2048];
            data = stream.ToArray();

            socket.SendTo(data, SocketFlags.None, user.endPoint);
        }

        public void SetPosition(int netId, Vector2 pos)
        {
            if (netManager != null) netIdScript.SetPosition(netId, pos);
        }

        //This function is called from player movement, it is used so "playerScript" finds the object component, in the future we will want to call the netId manager to move orders, right now we are hardcoding it
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
                        Debug.Log("Player Script found!!!");
                    }
                }
            }
        }

        public void FindNetIdManager()
        {
            netManager = GameObject.Find("NetIdManager");
            if (netManager != null) netIdScript = netManager.GetComponent<NetIdManager>();
        }


        public void SpawnEnemy()
        {


            // Create an enemy object in the server scene
            GameObject enemy = Instantiate(enemyPrefab, new Vector3(5, 5, 0), Quaternion.identity); // Example position

            // Send data to the client to replicate the enemy
            SendEnemyDataToClient(enemy.transform.position);
        }

        public void SendEnemyDataToClient(Vector3 position)
        {
            //var command = new Command(0, (int)UdpActions_.Create); // Create action
            //var fieldPosition = new FieldDoubleInt((int)position.x, (int)position.y); // Simplified serialization for position
            //command.fieldList.Add(fieldPosition);
            //
            //byte[] data = command.Serialize();
            //Debug.Log("Sending enemy data to client.");
            //socket.SendTo(data, SocketFlags.None, user.endPoint);
        }


    }
}