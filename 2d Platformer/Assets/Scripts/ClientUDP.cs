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
        bool goToSampleScene = false;
        static MemoryStream stream;
        public GameObject objectPlayer;
        public PlayerMovementServer playerScript;
        IPEndPoint ipep;

        void Start()
        {
        }

        void Update()
        {

        }

        public void StartClient()
        {
            Thread connect = new Thread(Connect);
            connect.Start();
        }

        void Connect()
        {
            Debug.Log("connecting to server");

            ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9090);

            server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
          

            Thread sendThread = new Thread(Send);
            sendThread.Start();

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
            Debug.Log("receiving data");

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
            Debug.Log("sending userName");
            data = Encoding.ASCII.GetBytes("userName");
            server.SendTo(data, 0, SocketFlags.None, ipep);
            Receive();
        }

        void Receive()
        {
            byte[] data = new byte[2048];
            int recv = 0;
            Debug.Log("receiveing name");
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);

            recv = server.ReceiveFrom(data, data.Length, SocketFlags.None, ref Remote);
            Debug.Log("data recieved: " + Encoding.ASCII.GetString(data));

            Thread receiveThread = new Thread(ReceiveJob);
            receiveThread.Start();
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
            if (playerScript == null)
            {
                Debug.Log("player not found");
            }
            else if (playerScript != null)
            {
                playerScript.SetPosition(pos.pos);
            }
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

            byte[] data = new byte[2048];
            data = stream.ToArray();
            Debug.Log("Sending position to server: " + data);
            

            server.SendTo(data, SocketFlags.None, ipep); //this should work;
        }

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