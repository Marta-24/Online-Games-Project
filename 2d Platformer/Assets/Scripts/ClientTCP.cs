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

    public class testClass
    {
        public int comand = 1;
        public List<int> pos = new List<int> { 3, 3 };
    }

    public class ClientTCP : MonoBehaviour
    {


        //public GameObject UItextObj;
        public TextMeshProUGUI UIText;
        string clientText;
        Socket server;
        bool goToSampleScene = false;
        bool serializeTry = true;
        static MemoryStream stream;

        void Start()
        {
        }

        void Update()
        {
            if (goToSampleScene)
            {
                SceneManager.LoadScene("WaitingRoom");
                goToSampleScene = false;
            }

            if (goToSampleScene == true)
            {
                if (serializeTry)
                {
                    //SendPosition();

                    //Serialize shit
                    //SerializePos();
                    //DeserializePos();
                    serializeTry = false;
                }
            }
        }

        public void StartClient()
        {
            Thread connect = new Thread(Connect);
            connect.Start();
        }

        void Connect()
        {
            Debug.Log("connecting to server");

            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);

            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.Connect(ipep);

            Thread sendThread = new Thread(Send);
            sendThread.Start();

            Thread receiveThread = new Thread(Receive);
            receiveThread.Start();
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
            goToSampleScene = true;
            CommandMessage command = (CommandMessage)BitConverter.ToInt32(data, 0);

            if (command == CommandMessage.FirstMessage)
            {
                goToSampleScene = true;
            }
            else if (command == CommandMessage.StartGame)
            {
                Debug.Log("StartGame command received.");
                SceneManager.LoadScene("Scene1");
            }
        }

        public void SendPosition() // This is a test
        {
            var t = new testClass();
            t.comand = 2;
            t.pos = new List<int> { 10, 3 };

            string json = JsonUtility.ToJson(t);

            stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(json);

            byte[] data_ = new byte[stream.ToString().Length];
            data_ = Encoding.ASCII.GetBytes(stream.ToString());
            Debug.Log(data_);

            server.Send(data_);

            ReceiveTest(stream.ToString());
        }

        public void ReceiveTest(string data)
        {
            Debug.Log("starting receive");
            byte[] data_ = new byte[data.Length];

            data_ = Encoding.ASCII.GetBytes(data.ToString());




            MemoryStream _stream = new MemoryStream();
            _stream.Write(data_, 0, data_.Length);
            Debug.Log(data_.Length);

            var t = new testClass();
            BinaryReader reader = new BinaryReader(_stream);



            stream.Seek(0, SeekOrigin.Begin);

            string json = reader.ReadString();
            Debug.Log(json);
            t = JsonUtility.FromJson<testClass>(json);
            Debug.Log(t);



        }
    }
}