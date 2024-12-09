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
    public enum UdpActions_ : int
    {
        Hello = 0,
        Position,
        Create
    }

    public enum FieldType_ : int
    {
        String = 0,
        DoubleInt
    }

    public abstract class Field
    {
        //public int type;
        public abstract string GetString();
        public abstract Vector2 GetPos();
    }

    public class FieldString : Field
    {
        public string data;

        public FieldString(string data)
        {
            this.data = data;
        }

        public override string GetString()
        {
            return data;
        }

        public override Vector2 GetPos()
        {
            Vector2 vec = new Vector2(0, 0);
            return vec;
        }
    }

    public class FieldDoubleInt : Field
    {
        Vector2 doubleInt;

        public FieldDoubleInt(Vector2 data)
        {
            doubleInt = data;
        }

        public override string GetString()
        {
            return "";
        }

        public override Vector2 GetPos()
        {
            return doubleInt;
        }
    }

    public class Command
    {
        public int netID;
        public int action;
        public List<int> fieldType;
        public List<Field> fieldList;

        public Command()
        {
            this.fieldType = new List<int>();
            this.fieldList = new List<Field>();
        }
        public Command(int netID, int action)
        {
            this.netID = netID;
            this.action = action;
            this.fieldType = new List<int>();
            this.fieldList = new List<Field>();
        }

        public Command(int netID, int action, List<int> typeList, List<Field> list)
        {
            this.netID = netID;
            this.action = action;
            this.fieldType = typeList;
            this.fieldList = list;
        }

        public Command(int netID, int action, int type, Field field)
        {
            this.netID = netID;
            this.action = action;
            this.fieldType = new List<int>();
            this.fieldType.Add(type);
            this.fieldList = new List<Field>();
            this.fieldList.Add(field);
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
            string str = "serverName";
            FieldString fld = new FieldString(str);
            Command com = new Command(0, 0, 0, fld);
            

            string json01 = JsonUtility.ToJson(com);
            string json02;
            
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(json01);

            for(int i = 0; i < com.fieldList.Count; i++)
            {
                json02 = JsonUtility.ToJson((com.fieldList[i]));
                Debug.Log(json02);
                writer.Write(json02);
            }

            byte[] data = new byte[2048];
            data = stream.ToArray();


            Debug.Log("SendHello Serialized:" + data);

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
            Debug.Log("starting deserialize");
            MemoryStream stream = new MemoryStream();
            stream.Write(data_, 0, data_.Length);

            //var command = new ReplicationMessage();
            var com = new Command();

            
            BinaryReader reader = new BinaryReader(stream);
            stream.Seek(0, SeekOrigin.Begin);

            string json01 = reader.ReadString();
            string json02;

            com = JsonUtility.FromJson<Command>(json01);
            com.fieldList = new List<Field>();

            Debug.Log(com.fieldType[0]);

            for (int i = 0; i < com.fieldType.Count; i++)
            {
                json02 = reader.ReadString();
                
                Debug.Log(json02);
                if (com.fieldType[i] == (int)FieldType_.String)
                {
                    com.fieldList.Add(JsonUtility.FromJson<FieldString>(json02));
                    Debug.Log(com.fieldList[0].GetString());
                }
                if(com.fieldType[i] == (int)FieldType_.DoubleInt)
                {
                    com.fieldList.Add(JsonUtility.FromJson<FieldDoubleInt>(json02));
                }
            }
            
            // DO Diferent actions with the data received
            if (com.action == (int)UdpActions_.Position)
            {
                SetPlayerPosition(com.fieldList[0].GetPos());
            }

            return 1;
        }

        public void SendPlayerPositionToClient(Vector2 position)
        {
            FieldDoubleInt intData = new FieldDoubleInt(position);
            Command com = new Command(0, (int)UdpActions_.Position, (int)FieldType_.DoubleInt, intData);
            

            string json01 = JsonUtility.ToJson(com);
            string json02;
            
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(json01);

            for(int i = 0; i < com.fieldList.Count; i++)
            {
                json02 = JsonUtility.ToJson((com.fieldList[i]));
                Debug.Log(json02);
                writer.Write(json02);
            }

            byte[] data = new byte[2048];
            data = stream.ToArray();

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

        public void SpawnEnemy()
        {
            Debug.Log("Spawning enemy on the server.");

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