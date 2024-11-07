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
    public enum CommandMessage : int
    {
        FirstMessage = 0,
        StartGame = 1,
        UpdatePosition = 2,
    }

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
        static MemoryStream stream;

        string serverText;

        public void StartGame()
        {
            foreach (User client in _clientSockets)
            {
                SendCommand(client.Socket, CommandMessage.StartGame);
            }
        }

        void SendCommand(Socket socket, CommandMessage command)
        {
            byte[] data = BitConverter.GetBytes((int)command);
            socket.Send(data);
            Debug.Log("Sent StartGame command to client.");
        }


        void Update()
        {

        }


        public void startServer()
        {
            Debug.Log("initializing startserver()");

            IPEndPoint localEp = new IPEndPoint(IPAddress.Any, 9050);

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

                Thread newConnection = new Thread(() => Receive(_socket));
                newConnection.Start();
            }
        }


        async void Receive(Socket sock)
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
        }

        public void OnDataReceived(IAsyncResult state)
        {
            Debug.Log("sending ping");

            socWorker = socket.EndAccept(state);
        }

        void Send(Socket socket_)
        {

            byte[] data_ = new byte[1024];

            data_ = Encoding.ASCII.GetBytes("serverName");
            socket_.Send(data_);
            Debug.Log("data Send and recieved");
        }

        public void ReceivePosition()
        {
            Debug.Log("starting receive");
            byte[] data_ = new byte[1024];

            User user = _clientSockets.Find(x => x.Socket != null);

            if (user.Socket != null)
            {
                Debug.Log("socket works!");
                user.Socket.Receive(data_);
                Debug.Log("data received" + data_);

                MemoryStream _stream = new MemoryStream();
                _stream.Write(data_, 0, data_.Length);
                Debug.Log(data_.Length);

                var t = new testClass();
                BinaryReader reader = new BinaryReader(_stream);
                

                
                //stream.Seek(0, SeekOrigin.Begin);
                
                //string json = reader.ReadString();
                //Debug.Log(json);
                //t = JsonUtility.FromJson<testClass>(json);
                //Debug.Log(t);
            }
            else
            {
                Debug.Log("it didn't work");
            }


        }
    }
}