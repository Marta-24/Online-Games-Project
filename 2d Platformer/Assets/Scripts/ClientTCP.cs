using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;
using System;

namespace Scripts
{
    public class ReplicationMessage
    {
        public int NetID = 0;
        public int action = 0;
    }

    public class testClass
    {
        public Vector2 pos;
    }

    public class ClientTCP : MonoBehaviour
    {
        Socket server;
        bool goToSampleScene = false;
        bool serializeTry = true;
        static MemoryStream stream;
        public GameObject objectPlayer;
        public PlayerMovementServer PlayerScript;

        public void StartClient()
        {
            Thread connect = new Thread(Connect);
            connect.Start();
        }

        void Connect()
        {
            Debug.Log("connecting to server");

            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9090);

            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Connect(ipep);

            Thread sendThread = new Thread(Send);
            sendThread.Start();

            Thread receiveThread = new Thread(ReceiveJob);
            receiveThread.Start();
        }

        private void ReceiveJob()
        {
            while (true)
            {
                int rec = ReceiveTCP();

                if (rec == 0)
                {
                    break;
                }
            }
        }

        private int ReceiveTCP()
        {
            byte[] data = new byte[2048];
            Debug.Log("receiving data");
            int rec = server.Receive(data);

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
            byte[] data = new byte[1024];
            Debug.Log("sending userName");
            data = Encoding.ASCII.GetBytes("userName");
            server.Send(data);
        }

        void Receive()
        {
            byte[] data = new byte[1024];
            int recv = 0;
            recv = server.Receive(data);
            Debug.Log("data recieved: " + Encoding.ASCII.GetString(data));
        }

        public void SendPosition(Vector2 position)
        {
            var command = new ReplicationMessage();
            command.NetID = 0;
            command.action = 2;

            var t = new testClass();
            t.pos = position;
            string json01 = JsonUtility.ToJson(command);
            string json02 = JsonUtility.ToJson(t);
            stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(json01);
            writer.Write(json02);

            byte[] data = new byte[1024];
            Debug.Log("sSending position: " + Encoding.ASCII.GetString(stream.ToArray()));
            data = stream.ToArray();

            server.Send(data); 
        }

        int deserializeJson(byte[] data_)
        {
            Debug.Log("starting deserialize");
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

            if (command.action == 1)
            {   
                Debug.Log("Servername received");
            }
            else if (command.action == 2)
            {
                

                t = JsonUtility.FromJson<testClass>(json02);

                // trying to set position
                SetPlayerPosition(t);
            }
            return command.action;
        }

        void SetPlayerPosition(testClass pos)
        {
            if (PlayerScript == null)
            {
                Debug.Log("player not found");
            }
            else if (PlayerScript != null)
            {
                PlayerScript.SetPosition(pos.pos);
            }
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