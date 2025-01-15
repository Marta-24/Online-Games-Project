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

    public class ServerUDP : MonoBehaviour
    {

        private AutoResetEvent _waitHandle = new AutoResetEvent(false);
        private List<UserUDP> _clientEndPoints = new List<UserUDP>();
        private List<Thread> _clientEndPointsThread = new List<Thread>();
        public GameObject objectPlayer;
        public PlayerMovementCopy playerScript;
        Socket socket;

        public GameObject enemyPrefab;
        public GameObject netManager;
        public NetIdManager netIdScript;
        public Instanciator instanciator;
        public GameObject panelUi;
        public bool receiveNewConnections = true;
        public InformationBetweenScenes info;
        Thread newConnection;
        public SceneLoader sceneLoader;

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

            while (receiveNewConnections)
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint Remote = (EndPoint)(sender);


                recv = socket.ReceiveFrom(_data, ref Remote);

                string str = FirstDeserialize(_data);

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

            sceneLoader.ChangeToLevel(1, true, true);
        }

        void SendHello(EndPoint Remote)
        {
            string str = "serverName";

            StringPacket packet = new StringPacket(0, str);

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

            int com = DeserializeJson(data);

            return rec;
        }

        public string FirstDeserialize(byte[] data_)
        {
            MemoryStream stream = new MemoryStream();
            stream.Write(data_, 0, data_.Length);

            ActionType actionType;

            BinaryReader reader = new BinaryReader(stream);
            stream.Seek(0, SeekOrigin.Begin);

            string json01 = reader.ReadString();
            string json02 = reader.ReadString();

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
            }
            else if (actionType == ActionType.Create)
            {
                CreatePacket packet = JsonUtility.FromJson<CreatePacket>(str);
                if (netIdScript != null)
                {
                    netIdScript.StackObject(packet.netId, packet.objType, packet.position, packet.direction, packet.rotation);
                }
            }
            else if (actionType == ActionType.Damage)
            {
                IntPacket packet = JsonUtility.FromJson<IntPacket>(str);
                netIdScript.GiveDamage(packet.netId, packet.a);
            }
            else if (actionType == ActionType.Hello)
            {
            }
            else if (actionType == ActionType.ChangeLevel)
            {
                StartGamePacket packet = JsonUtility.FromJson<StartGamePacket>(str);
                sceneLoader.NextFramChange(packet.a, false, packet.player);
            }
        }

        public void SendPosition(int netId, Vector2 position)
        {
            MovementPacket packet = new MovementPacket(netId, position);
            string json01 = JsonUtility.ToJson(packet);
            SendString(json01, ActionType.Position);
        }

        public void SendCreateObject(int netId, GameObjectType type, Vector2 pos, Vector2 direction, Vector3 rotation)
        {
            CreatePacket packet = new CreatePacket(netId, pos, direction, rotation, type);

            string json01 = JsonUtility.ToJson(packet);
            SendString(json01, ActionType.Create);
        }

        public void SendDamage(int netId, int health)
        {
            IntPacket packet = new IntPacket(netId, health);

            string json01 = JsonUtility.ToJson(packet);
            SendString(json01, ActionType.Damage);
        }

        public void SendLevelChange(int level, bool playerSpawn)
        {
            StartGamePacket packet = new StartGamePacket(0, level, playerSpawn);

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

            foreach (UserUDP u in _clientEndPoints)
            {
                socket.SendTo(data, SocketFlags.None, u.endPoint);
            }

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
                    playerScript = objectPlayer.GetComponent<PlayerMovementCopy>();

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

            GameObject obj = GameObject.Find("SceneLoader");
            sceneLoader = obj.GetComponent<SceneLoader>();
        }
    }
}