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
        public GameObject textPanel;
        string text;
        public GameObject enemyPrefab;
        void Start()
        {
        }

        void Update()
        {

        }

        public void StartClient()
        {
            //127.0.0.1
            text = textPanel.GetComponent<TMP_InputField>().text;
            Thread connect = new Thread(Connect);
            connect.Start();

        }

        void Connect()
        {
            Debug.Log("connecting to server");

            ipep = new IPEndPoint(IPAddress.Parse(text), 9090);

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
            
            deserializeJson(data);
            Thread receiveThread = new Thread(ReceiveJob);
            receiveThread.Start();
        }

        int deserializeJson(byte[] data_)
        {
            Debug.Log("starting deserialize");
            MemoryStream stream = new MemoryStream();
            stream.Write(data_, 0, data_.Length);

            //var command = new ReplicationMessage();
            var com = new Command();

            //var t = new testClass();
            BinaryReader reader = new BinaryReader(stream);
            stream.Seek(0, SeekOrigin.Begin);

            string json01 = reader.ReadString();
            string json02 = reader.ReadString();

            com = JsonUtility.FromJson<Command>(json01);
            com.fieldList = JsonUtility.FromJson<List<Field>>(json02);
            Debug.Log(com.netID);
            Debug.Log(com.fieldList[1]);

            if (com.action == (int)UdpActions.Create)
            {
                // Assume position fields are sent as integers
                var field = com.fieldList[0] as FieldDoubleInt;
                Vector3 position = new Vector3(field.a, field.b, 0);

                Debug.Log($"Spawning enemy at position {position} on client.");
                Instantiate(enemyPrefab, position, Quaternion.identity);
            }


            //if (command.action == 1)
            //{
            //    Debug.Log("Servername received");
            //}
            //else if (command.action == 2)
            //{
            //    t = JsonUtility.FromJson<testClass>(json02);
            //
            //    // trying to set position
            //    SetPlayerPosition(t);
            //}
            return 1;
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