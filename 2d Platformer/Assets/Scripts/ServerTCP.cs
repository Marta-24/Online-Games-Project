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
    public class User
    {
        public string Name;
        public Socket Socket;

        public User(string name, Socket socket)
        {
            Name = name;
            Socket = socket;
        }

        public User()
        {
            Name = "";
            Socket = null;
        }
    }

    public class ServerTCP : MonoBehaviour
    {
        private AutoResetEvent _waitHandle = new AutoResetEvent(false);
        Socket socket;
        Socket socWorker;
        Thread mainThread = null;
        public AsyncCallback pfnCallBack;
        IAsyncResult m_asynResult;
        private List<User> _clientSockets = new List<User>();
        private List<Thread> _clientThreads = new List<Thread>();
        string serverText;
        public GameObject objectPlayer;
        public PlayerMovementServer PlayerScript;
        public void StartGame()
        {
            foreach (User client in _clientSockets)
            {
                //SendCommand(client.Socket, CommandMessage.StartGame);
            }
        }



        void Update()
        {

        }


        public void startServer()
        {
            Debug.Log("initializing startserver()");

            IPEndPoint localEp = new IPEndPoint(IPAddress.Any, 9090);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(localEp);
            socket.Listen(10);

            Debug.Log("Socket worked");

            mainThread = new Thread(CheckNewConnections);
            mainThread.Start();
        }

        async void CheckNewConnections()
        {
            while (true)
            {
                Debug.Log("starting checkNewConnections()");

                Socket _socket = socket.Accept();

                IPEndPoint clientep = (IPEndPoint)socket.RemoteEndPoint;

                Thread newConnection = new Thread(() => AcceptConnection(_socket));
                newConnection.Start();
            }
        }


        async void AcceptConnection(Socket sock)
        {
            byte[] data = new byte[1024];

            int res = sock.Receive(data);
            Debug.Log("Starting Recieve");

            // Data recieved
            string name = Encoding.ASCII.GetString(data, 0, data.Length);

            User user = new User(name, sock);
            _clientSockets.Add(user);
            Debug.Log("New socket added: " + name);
            Thread answer = new Thread(() => Send(user.Socket));
            answer.Start();

            Thread clientThread = new Thread(() => ReceiveJob(user));
            clientThread.Start();
            _clientThreads.Add(clientThread);
        }

        private void ReceiveJob(User user_)
        {
            while (true)
            {
                int rec = ReceiveTCP(user_);

                if (rec == 0)
                {
                    _clientSockets.Remove(user_);
                    break;
                }

            }
        }

        private int ReceiveTCP(User user_)
        {
            byte[] data = new byte[2048];
            int rec = user_.Socket.Receive(data);

            if (rec == 0)
            {
                return 0;
            }

            int com = DeserializeJson(data);

            return rec;
        }


        public void OnDataReceived(IAsyncResult state)
        {
            Debug.Log("sending ping");

            socWorker = socket.EndAccept(state);
        }

        void Send(Socket socket_)
        {

            SendServerName(socket_);
            Debug.Log("data Send and recieved");
        }

        public int DeserializeJson(byte[] data_)
        {
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

            return command.action   ;
        }

        public void SetPlayerPosition(testClass pos)
        {
            Debug.Log("changing player position:  " + pos.pos[0] + "," + pos.pos[1]);
            if (PlayerScript != null)
            {
                PlayerScript.SetPosition(pos.pos);
            }
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
            
            byte[] data = new byte[1024];
            Debug.Log("Sending position: " + Encoding.ASCII.GetString(stream.ToArray()));
            data = stream.ToArray();

            // For now we send the data to all clients

            foreach (User client in _clientSockets)
            {
                client.Socket.Send(data);
            }
            //_clientSockets.ForEach(Send(socket, data));
            //server.Send(data); //this should work;
        }

        public void SendServerName(Socket socket)
        {
            var command = new ReplicationMessage();
            command.NetID = 0;
            command.action = 1;

            string json01 = JsonUtility.ToJson(command);
             MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(json01);
            byte[] data = new byte[1024];
            data = stream.ToArray();
            socket.Send(data);
        }

        public void ConnectToPlayer()
        {
            if (PlayerScript == null)
            {
                objectPlayer = GameObject.Find("Player1");
                if (objectPlayer != null)
                {
                    PlayerScript = objectPlayer.GetComponent<PlayerMovementServer>();

                    if (PlayerScript != null)
                    {
                        Debug.Log("Player Script found!!!");
                    }
                }
            }
        }
    }
}