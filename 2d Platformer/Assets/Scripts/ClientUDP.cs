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
        public GameObject textPanelIp;
        public GameObject textPanelName;
        private TMP_InputField textIp;
        private TMP_InputField textName;
        public GameObject enemyPrefab;
        public GameObject netManager;
        public NetIdManager netIdScript;
        string userName;
        string nameIp;
        public InformationBetweenScenes info;
        public SceneLoader sceneLoader;
        public int ColdDown = 20;

        //new message thingies;
        public bool jitter = true;
        public bool packetLoss = true;
        public int minJitt = 0;
        public int maxJitt = 800;
        public int lossThreshold = 50;
        public bool exit = false;
        public List<Message> messageBuffer = new List<Message>();
        public List<ParentPacket> messageSend = new List<ParentPacket>();
        object valueTypeLock = new object();
        Message mesConnection;
        public int mesId = 0;
        public int resendTimer = 5;
        void Start()
        {
            textIp = textPanelIp.GetComponent<TMP_InputField>();
            textName = textPanelName.GetComponent<TMP_InputField>();

            GameObject obj = GameObject.Find("SceneLoader");
            sceneLoader = obj.GetComponent<SceneLoader>();

            Thread MessageJitter = new Thread(sendMessages);
            MessageJitter.Start();
        }

        void Update()
        {
            if (netManager == null) // Even though this is called at start, we had some problems and for now this is a way to make sure we find the server or client
            {
                FindNetIdManager();
            }

            if (goToScene1)
            {
                Debug.Log("loading scene1");
                sceneLoader.LoadScene01Client();
                goToScene1 = false;
            }

            resendTimer--;
            if (resendTimer == 0)
            {
                resendTimer = 5;
                foreach(var p in messageSend)
                {
                    if (p.ty)
                }
            }

            if (ColdDown > 0) ColdDown--;
        }

        void OnDestroy()
        {
            exit = true;
        }
        public void StartClient()
        {
            //127.0.0.1
            if (ColdDown == 0)
            {
                Thread connect = new Thread(Connect);
                connect.Start();
            }
        }

        void Connect()
        {
            nameIp = textIp.text;
            userName = textName.text;
            ipep = new IPEndPoint(IPAddress.Parse(nameIp), 9090);

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

            //Message mes = deserializeMessage(data);
            deserializeJson(data);

            return rec;
        }
        void Send()
        {
            byte[] data = new byte[2048];

            StringPacket packet = new StringPacket(0, userName, mesId);
            mesId++;

            string json01 = JsonUtility.ToJson(packet);
            Message mes = SendString(json01, ActionType.Hello, packet);
            mesConnection = mes;

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
                netIdScript.StackObject(packet.netId, packet.objType, packet.position, packet.direction, packet.rotation);
            }
            else if (actionType == ActionType.Damage)
            {
                IntPacket packet = JsonUtility.FromJson<IntPacket>(str);
                netIdScript.GiveDamage(packet.netId, packet.a);
            }
            else if (actionType == ActionType.ChangeLevel)
            {
                StartGamePacket packet = JsonUtility.FromJson<StartGamePacket>(str);
                sceneLoader.NextFramChange(packet.a, false, packet.player);
            }
            else if (actionType == ActionType.Confirmation)
            {
                ConfirmationPacket packet = JsonUtility.FromJson<ConfirmationPacket>(str);

                List<ParentPacket> auxBuffer;
                int ID = 0;
                int i = 0;
                lock (valueTypeLock)
                {
                    auxBuffer = new List<ParentPacket>(messageSend);
                }

                foreach (var p in auxBuffer)
                {
                    if (p.id == packet.id)
                    {
                        lock (valueTypeLock)
                        {
                            messageSend.RemoveAt(i);
                            Debug.Log("removed especific value");
                            i--;
                        }
                        i++;
                    }

                }
            }
        }

        public void SendPosition(int netId, Vector2 position)
        {
            MovementPacket packet = new MovementPacket(netId, position, mesId);
            mesId++;
            string json01 = JsonUtility.ToJson(packet);
            SendString(json01, ActionType.Position, packet);
        }

        public void SendCreateObject(int netId, GameObjectType type, Vector2 pos, Vector2 direction, Vector3 rotation)
        {
            CreatePacket packet = new CreatePacket(netId, pos, direction, rotation, type, mesId);
            mesId++;
            string json01 = JsonUtility.ToJson(packet);
            SendString(json01, ActionType.Create, packet);
        }

        public void SendDamage(int netId, int health)
        {
            IntPacket packet = new IntPacket(netId, health, mesId);
            mesId++;
            string json01 = JsonUtility.ToJson(packet);
            SendString(json01, ActionType.Damage, packet);
        }

        public void SendLevelChange(int level, bool playerSpawn)
        {
            StartGamePacket packet = new StartGamePacket(0, level, playerSpawn, mesId);
            mesId++;
            string json01 = JsonUtility.ToJson(packet);
            SendString(json01, ActionType.ChangeLevel, packet);
        }

        public Message SendString(string str, ActionType type, ParentPacket p)
        {
            string json02 = JsonUtility.ToJson(type);
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(json02);
            writer.Write(str);

            byte[] data = new byte[2048];
            data = stream.ToArray();

            messageSend.Add(p);
            return sendMessage(data, false);
        }

        public void FindNetIdManager()
        {
            netManager = GameObject.Find("NetIdManager");
            if (netManager != null) netIdScript = netManager.GetComponent<NetIdManager>();

            GameObject obj = GameObject.Find("SceneLoader");
            sceneLoader = obj.GetComponent<SceneLoader>();
        }

        Message sendMessage(Byte[] text, bool check)
        {
            System.Random r = new System.Random();
            Message m = new Message();
            if (((r.Next(0, 100) > lossThreshold) && packetLoss) || !packetLoss) // Don't schedule the message with certain probability
            {

                m.message = new byte[2048];
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

            return m;
        }

        //Run this always in a separate Thread, to send the delayed messages
        void sendMessages()
        {
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
                            server.SendTo(m.message, SocketFlags.None, ipep);
                           

                            lock (valueTypeLock)
                            {
                                messageBuffer.RemoveAt(i);
                            }

                            i--;
                            if (m.message == mesConnection.message) //We need to know when this is happening to the thread of updjob can start
                            {
                                Thread receiveThread = new Thread(ReceiveJob);
                                receiveThread.Start();
                            }
                        }
                        i++;
                    }
                }
            }
        }
    }
}