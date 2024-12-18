using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    public enum gameObjectType : int
    {
        none = 0,
        player1 = 1,
        player2 = 2,
        enemy = 3,
        bullet = 4
    }
    public class NetId
    {
        public int netId;
        public GameObject gameObject;
        public gameObjectType type;

        public NetId(int netId_, GameObject gameObject_, gameObjectType type)
        {
            this.netId = netId_;
            gameObject = gameObject_;
            this.type = type;
        }
    }

    public class FutureObject
    {
        public int netID;
        public Vector2 pos;
        public gameObjectType type;

        public FutureObject(int netID, Vector2 pos, gameObjectType type)
        {
            this.netID = netID;
            this.pos = pos;
            this.type = type;
        }
    }
    public class NetIdManager : MonoBehaviour
    {
        public List<NetId> netIdList = new List<NetId>();
        public GameObject objectUDP;
        public ServerUDP server;
        public ClientUDP client;
        public GameObject netIdManagerGameObject;
        public Instanciator instanciator_;
        public bool startEnded = false;
        public List<FutureObject> NeedToCreateList = new List<FutureObject>();
        public int frameCounter = 60;

        void Start()
        {
            //Generate Random instance

            AddServer();
            FindInstanciator();
            FindServerOrClient();


            startEnded = true;
            Debug.Log("START ENDED");
        }

        // Update is called once per frame
        void Update()
        {
            if (NeedToCreateList.Count != 0)
            {
                for (int i = 0; i < NeedToCreateList.Count; i++)
                {
                    Debug.Log("creating object from update");
                    CreateObject(NeedToCreateList[i].netID, NeedToCreateList[i].type, NeedToCreateList[i].pos);
                }
                NeedToCreateList.Clear();
            }

            if (frameCounter > 0)
            {
                frameCounter--;
            }
            else if (frameCounter == 0)
            {
                frameCounter--;
                if (server != null)
                {
                    NetId id = CreateNetId(instanciator_.InstancePlayerOne(), gameObjectType.player1); // player one created, send the clients the command create!!!
                    
                    server.SendCreateObject(id.netId, id.type, new Vector2(0.0f, 0.0f));
                    id = CreateNetId(instanciator_.InstancePlayerTwo(), gameObjectType.player2); // same
                    server.SendCreateObject(id.netId, id.type, new Vector2(0.0f, 0.0f));

                }
            }

        }

        void AddServer()
        {
            objectUDP = GameObject.Find("ServerManager");
            if (objectUDP != null)
            {
                server = objectUDP.GetComponent<ServerUDP>();

                if (server != null)
                {
                    Debug.Log("Server found!!!, pinging him, netidmanager speaking :)");
                }
            }
        }

        void FindInstanciator()
        {
            instanciator_ = GetComponent<Instanciator>();
        }

        public int FindNetId(GameObject obj)
        {
            for (int i = 0; i < netIdList.Count; i++)
            {
                Debug.Log(netIdList[i].gameObject.name);
                if (GameObject.ReferenceEquals(obj, netIdList[i].gameObject)) //this is for the moment
                {
                    return netIdList[i].netId;
                }
            }

            return -1;
        }
        public NetId CreateNetId(GameObject obj, gameObjectType type)
        {
            NetId id = new NetId(GenerateId(), obj, type);
            netIdList.Add(id);

            Debug.Log("Added new NetId with name and id:" + obj.name + " " + id.netId + " " + id.type);
            return id;
        }

        public NetId AddNetId(int intId, GameObject obj, gameObjectType type)
        {
            NetId id = new NetId(intId, obj, type);
            netIdList.Add(id);

            Debug.Log("Added new NetId with name and id:" + obj.name + " " + id.netId + " " + id.type);
            return id;
        }

        public NetId FindObject(int id)
        {
            for (int i = 0; i < netIdList.Count; i++)
            {
                if (netIdList[i].netId == id)
                {
                    Debug.Log("object found between all id" + id);
                    return netIdList[i];
                }
            }
            Debug.Log("object not found in list" + id);
            return null;
        }
        private int GenerateId()
        {

            while (true)
            {
                bool sec = true;
                int num = Random.Range(0, int.MaxValue); //Generate random number

                //Check if number already exists 
                for (int i = 0; i < netIdList.Count; i++)
                {
                    if (netIdList[i].netId == num)
                    {
                        sec = false;
                    }
                }

                if (sec) return num;
            }
        }
        public void SendPosition(GameObject obj, Vector2 pos)
        {
            Debug.Log(obj.name);
            int id = FindNetId(obj);
            if (id != -1)
            {
                Debug.Log("sending a position that is not negative!");
                if (server != null) server.SendPosition(id, pos);
                if (client != null) client.SendPosition(id, pos);
            }
        }

        public void CreateBullet(GameObject obj, Vector2 pos)
        {
            int id = GenerateId();

            CreateNetId(obj, gameObjectType.bullet);

            if (server != null) server.SendCreateObject(id, gameObjectType.bullet, pos);
            if (client != null) client.SendCreateObject(id, gameObjectType.bullet, pos);
        }

        public void StackObject(int netId, gameObjectType type, Vector2 pos)
        {
            Debug.Log("object stacked: " + ((int)type));
            NeedToCreateList.Add(new FutureObject(netId, pos, type));
        }

        public void CreateObject(int netId, gameObjectType type, Vector2 pos)
        {
            if (startEnded)
            {
                Debug.Log("creating something");
                if (type == gameObjectType.player1)
                {
                    AddNetId(netId, instanciator_.InstancePlayerOne(), gameObjectType.player1); // player one created, send the clients the command create!!!
                }
                else if (type == gameObjectType.player2)
                {
                    AddNetId(netId, instanciator_.InstancePlayerTwo(), gameObjectType.player2); // same
                }
                else if (type == gameObjectType.bullet)
                {
                    Debug.Log("order of creating bullet reached!!!");
                }
            }
        }

        public void SetPosition(int id, Vector2 pos)
        {
            Debug.Log("receiveing orders to move");
            NetId obj = FindObject(id);

            if (obj != null)
            {
                if (obj.type == gameObjectType.player1 || obj.type == gameObjectType.player2)
                {
                    Debug.Log("changing player position");
                    obj.gameObject.GetComponent<PlayerMovementServer>().SetPosition(pos);
                }
            }


        }

        private void FindServerOrClient()
        {
            if (objectUDP == null) objectUDP = GameObject.Find("ClientManager");
            if (objectUDP == null) objectUDP = GameObject.Find("ServerManager");

            if (server == null)
            {
                server = objectUDP.GetComponent<ServerUDP>();
            }
            if (client == null)
            {
                client = objectUDP.GetComponent<ClientUDP>();
            }
        }
    }
}
