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

        void Receive()
        {
            int recv;
            byte[] _data = new byte[2048];

            while (true)
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);

                Debug.Log("starting ReceiveFrom Server");
                recv = socket.ReceiveFrom(_data, ref Remote);

                Debug.Log(Encoding.ASCII.GetString(_data, 0, recv));

                //Creating user
                UserUDP user_ = new UserUDP(Remote, Encoding.ASCII.GetString(_data, 0, recv));
                user = user_;
                SendHello(Remote);

                Thread clientThread = new Thread(() => ReceiveJob(user_));
                clientThread.Start();
                _clientEndPointsThread.Add(clientThread);
            }
        }


        void SendHello(EndPoint Remote)
        {
            var command = new ReplicationMessage();
            command.NetID = 0;
            command.action = 3;

            var s = new StringJson("serverName");
            
            string json01 = JsonUtility.ToJson(command);
            string json02 = JsonUtility.ToJson(s);
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
            Debug.Log("startint receive");
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
            Debug.Log("deserializing data");
            MemoryStream stream = new MemoryStream();
            stream.Write(data_, 0, data_.Length);

            var command = new ReplicationMessage();
            var t = new testClass();
            BinaryReader reader = new BinaryReader(stream);
            stream.Seek(0, SeekOrigin.Begin);

            string json01 = reader.ReadString();
            string json02 = reader.ReadString();

            command = JsonUtility.FromJson<ReplicationMessage>(json01);
            Debug.Log(command.action);

            t = JsonUtility.FromJson<testClass>(json02);

            // trying to set position
            SetPlayerPosition(t);

            return command.action;
        }

        public void SendPlayerPositionToClient(Vector2 position)
        {
            var command = new ReplicationMessage();
            command.NetID = 0;
            command.action = 2;

            var t = new testClass();
            t.pos = position;
            string json01 = JsonUtility.ToJson(command);
            string json02 = JsonUtility.ToJson(t);
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(json01);
            writer.Write(json02);

            byte[] data = new byte[2048];
            data = stream.ToArray();

            Debug.Log("sending position update");
            
            socket.SendTo(data, SocketFlags.None, user.endPoint);
        }

        public void SetPlayerPosition(testClass pos)
        {
            Debug.Log("changing player position:  " + pos.pos[0] + "," + pos.pos[1]);
            if (playerScript != null)
            {
                playerScript.SetPosition(pos.pos);
            }
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
    }
}