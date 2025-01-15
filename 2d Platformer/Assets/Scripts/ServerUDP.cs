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
    public class Message
    {
        public byte[] message = new byte[2048];
        public DateTime time;
        public DateTime resendTimer;
        public UInt32 id;
        public bool serverOriginated;
        public int iteration = 0;
        public Message()
        {
            this.message = new byte[2048];
        }
        public Message(byte[] message, DateTime time, UInt32 id)
        {
            this.time = time;
            this.id = id;

            this.message = new byte[2048];
            this.message = message;
        }
    }


    public class ServerUDP : MonoBehaviour
    {
        private List<UserUDP> _clientEndPoints = new List<UserUDP>();
        private List<Thread> _clientEndPointsThread = new List<Thread>();
        Socket socket;
        public GameObject netManager;
        public NetIdManager netIdScript;
        public Instanciator instanciator;
        public GameObject panelUi;
        public bool receiveNewConnections = true;
        public InformationBetweenScenes info;
        Thread newConnection;
        public SceneLoader sceneLoader;
        //new message thingies;
        public bool jitter = true;
        public bool packetLoss = true;
        public int minJitt = 0;
        public int maxJitt = 800;
        public int lossThreshold = 50;
        public bool exit = false;
        public List<Message> messageBuffer = new List<Message>();
        public ParentPacket lastReceived = new ParentPacket();
        object valueTypeLock = new object();
        public int mesId = 0;
        public void Start()
        {
            GameObject obj = GameObject.Find("SceneLoader");
            sceneLoader = obj.GetComponent<SceneLoader>();
        }
        // Function called with a button to start the udp server
        public void StartServer()
        {
            Debug.Log("Initializing udp server");

            IPEndPoint localEp = new IPEndPoint(IPAddress.Any, 9090);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.Bind(localEp);

            newConnection = new Thread(Receive);
            newConnection.Start();

            Thread MessageJitter = new Thread(sendMessages);
            MessageJitter.Start();
        }

        void Update()
        {
            if (netManager == null) // Even though this is called at start, we had some problems and for now this is a way to make sure we find the server or client
            {
                FindNetIdManager();
            }

            lock (valueTypeLock)
            {
                SendConfirmation(lastReceived.id);
            }
        }

        void Receive()
        {
            int recv;
            byte[] _data = new byte[2048];

            while (receiveNewConnections)
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);


                recv = socket.ReceiveFrom(_data, ref Remote);

                //Message mes = deserializeMessage(_data);
                //Debug.Log(mes.message);
                string str = FirstDeserialize(_data);
                Debug.Log(str);

                if (str != null)
                {
                    UserUDP user_ = new UserUDP(Remote, str);

                    _clientEndPoints.Add(user_);


                    SendHello(Remote);


                    Thread clientThread = new Thread(() => ReceiveJob(user_));
                    clientThread.Start();
                    _clientEndPointsThread.Add(clientThread);
                }
            }
        }

        public void StartGame()
        {
            receiveNewConnections = false;
            newConnection.Abort();
            Debug.Log("start");
            sceneLoader.ChangeToLevel(1, true, true);
        }

        void SendHello(EndPoint Remote)
        {
            string str = "serverName";

            StringPacket packet = new StringPacket(0, str, mesId);
            mesId++;

            string json01 = JsonUtility.ToJson(packet);
            SendString(json01, ActionType.Hello);
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
            if (rec == 0)
            {
                //return 0;
            }
            //Message mes = deserializeMessage(data);
            int com = DeserializeJson(data);

            return rec;
        }

        public Message deserializeMessage(byte[] data)
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(data, 0, data.Length);

            Message message_ = new Message();
            BinaryReader reader = new BinaryReader(stream);
            stream.Seek(0, SeekOrigin.Begin);

            string json01 = reader.ReadString();

            message_ = JsonUtility.FromJson<Message>(json01);
            Debug.Log(message_.time);
            return message_;
        }

        public string FirstDeserialize(byte[] data_)
        {
            Debug.Log(data_);
            MemoryStream stream = new MemoryStream();
            stream.Write(data_, 0, data_.Length);

            ActionType actionType;

            BinaryReader reader = new BinaryReader(stream);
            stream.Seek(0, SeekOrigin.Begin);

            string json01 = reader.ReadString();
            string json02 = reader.ReadString();
            Debug.Log(json01.Length);
            actionType = JsonUtility.FromJson<ActionType>(json01);
            Debug.Log(actionType);
            if (actionType == ActionType.Hello)
            {
                StringPacket packet = JsonUtility.FromJson<StringPacket>(json02);
                instanciator.IntanceUserPrefab(panelUi, packet.str);
                return packet.str;
            }

            return null;
        }

        public int DeserializeJson(byte[] data_)
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(data_, 0, data_.Length);

            ActionType actionType;

            BinaryReader reader = new BinaryReader(stream);
            stream.Seek(0, SeekOrigin.Begin);

            string json01 = reader.ReadString();
            string json02 = reader.ReadString();

            actionType = JsonUtility.FromJson<ActionType>(json01);

            GiveManagerAction(actionType, json02);
            return 1;
        }


        public void GiveManagerAction(ActionType actionType, string str)
        {
            if (actionType == ActionType.Position)
            {
                MovementPacket packet = JsonUtility.FromJson<MovementPacket>(str);
                // Setting position by netId
                SetPosition(packet.netId, packet.position);
                //lastReceived = packet;
            }
            else if (actionType == ActionType.Create)
            {
                CreatePacket packet = JsonUtility.FromJson<CreatePacket>(str);
                if (netIdScript != null)
                {
                    netIdScript.StackObject(packet.netId, packet.objType, packet.position, packet.direction, packet.rotation);
                }
                SendConfirmation(lastReceived.id);
                lastReceived = packet;
                SendConfirmation(lastReceived.id);
            }
            else if (actionType == ActionType.Damage)
            {
                IntPacket packet = JsonUtility.FromJson<IntPacket>(str);
                netIdScript.GiveDamage(packet.netId, packet.a);
                SendConfirmation(lastReceived.id);
                lastReceived = packet;
                SendConfirmation(lastReceived.id);
            }
            else if (actionType == ActionType.Hello)
            {
            }
            else if (actionType == ActionType.ChangeLevel)
            {
                StartGamePacket packet = JsonUtility.FromJson<StartGamePacket>(str);
                sceneLoader.NextFramChange(packet.a, false, packet.player);
                SendConfirmation(lastReceived.id);
                lastReceived = packet;
                SendConfirmation(lastReceived.id);
            }
        }

        public void SendPosition(int netId, Vector2 position)
        {
            MovementPacket packet = new MovementPacket(netId, position, mesId);
            mesId++;
            string json01 = JsonUtility.ToJson(packet);
            SendString(json01, ActionType.Position);
        }

        public void SendCreateObject(int netId, GameObjectType type, Vector2 pos, Vector2 direction, Vector3 rotation)
        {
            CreatePacket packet = new CreatePacket(netId, pos, direction, rotation, type, mesId);
            mesId++;
            string json01 = JsonUtility.ToJson(packet);
            SendString(json01, ActionType.Create);
        }

        public void SendDamage(int netId, int health)
        {
            IntPacket packet = new IntPacket(netId, health, mesId);
            mesId++;
            string json01 = JsonUtility.ToJson(packet);
            SendString(json01, ActionType.Damage);
        }

        public void SendConfirmation(int id)
        {
            ConfirmationPacket packet = new ConfirmationPacket(id);
            string json01 = JsonUtility.ToJson(packet);
            SendString(json01, ActionType.Confirmation);
        }
        public void SendLevelChange(int level, bool playerSpawn)
        {
            StartGamePacket packet = new StartGamePacket(0, level, playerSpawn, mesId);
            mesId++;
            string json01 = JsonUtility.ToJson(packet);
            SendString(json01, ActionType.ChangeLevel);
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

            sendMessage(data);
        }

        public void SetPosition(int netId, Vector2 pos)
        {
            if (netManager != null) netIdScript.SetPosition(netId, pos);
        }

        public void FindNetIdManager()
        {
            netManager = GameObject.Find("NetIdManager");
            if (netManager != null) netIdScript = netManager.GetComponent<NetIdManager>();

            GameObject obj = GameObject.Find("SceneLoader");
            sceneLoader = obj.GetComponent<SceneLoader>();
        }

        void sendMessage(Byte[] text)
        {
            System.Random r = new System.Random();
            if (((r.Next(0, 100) > lossThreshold) && packetLoss) || !packetLoss) // Don't schedule the message with certain probability
            {
                Message m = new Message();
                m.message = text;
                if (jitter)
                {
                    m.time = DateTime.Now.AddMilliseconds(r.Next(minJitt, maxJitt)); // delay the message sending according to parameters
                }
                else
                {
                    m.time = DateTime.Now;
                }
                m.id = 0;


                lock (valueTypeLock)
                {
                    messageBuffer.Add(m);
                }


            }

        }
        //Run this always in a separate Thread, to send the delayed messages
        void sendMessages()
        {
            Debug.Log("really sending..");
            while (!exit)
            {
                DateTime d = DateTime.Now;
                int i = 0;
                if (messageBuffer.Count > 0)
                {
                    List<Message> auxBuffer;

                    lock (valueTypeLock)
                    {
                        auxBuffer = new List<Message>(messageBuffer);
                    }

                    foreach (var m in auxBuffer)
                    {
                        if (m.time < d)
                        {

                            lock (valueTypeLock)
                            {
                                foreach (UserUDP u in _clientEndPoints)
                                {
                                    Debug.Log("sending message" + m.message);
                                    socket.SendTo(m.message, SocketFlags.None, u.endPoint);
                                }
                                messageBuffer.RemoveAt(i);
                            }

                            i--;
                        }
                        i++;
                    }
                }
            }
        }
    }
}