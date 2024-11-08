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
            //if (goToSampleScene)
            //{
            //    SceneManager.LoadScene("WaitingRoom");
            //    goToSampleScene = false;
            //}

            if (goToSampleScene == true)
            {
                if (serializeTry)
                {
                    //serializeJson();
                    //deserializeJson();
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
            //CommandMessage command = (CommandMessage)BitConverter.ToInt32(data, 0);
//
            //if (command == CommandMessage.FirstMessage)
            //{
            //    goToSampleScene = true;
            //}
            //else if (command == CommandMessage.StartGame)
            //{
            //    Debug.Log("StartGame command received.");
            //    SceneManager.LoadScene("Scene1");
            //}
        }

         public void serializeJson()
        {
            var command = new CommandMessage();
            command.com = 2;

            var t = new testClass();
            t.pos = new List<int> { 69, 9 };
            string json01 = JsonUtility.ToJson(command);
            string json02 = JsonUtility.ToJson(t);
            stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(json01);
            writer.Write(json02);
            
            byte[] data = new byte[1024];
            Debug.Log("sSending position: " + Encoding.ASCII.GetString(stream.ToArray()));
            data = stream.ToArray();

            server.Send(data); //this should work;
        }
        
        void deserializeJson()
        {
            var t = new testClass();
            BinaryReader reader = new BinaryReader(stream);
            stream.Seek(0, SeekOrigin.Begin);

            string json = reader.ReadString();
            Debug.Log(json);
            t = JsonUtility.FromJson<testClass>(json);
           
        }
    }
}