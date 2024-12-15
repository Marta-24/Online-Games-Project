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
                if (netIdList[i].gameObject == obj) return netIdList[i].netId;
            }

            return -1;
        }
        public NetId CreateNetId(GameObject gameObject_, gameObjectType type)
        {
            NetId id = new NetId(GenerateId(), gameObject_, type);
            netIdList.Add(id);

            Debug.Log("Added new NetId with name and id:" + gameObject_.name + " " + id.netId + " " + id.type);
            return id;
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
            int id = FindNetId(obj);

            if (server != null) server.SendPosition(id, pos);
            if (client != null) client.SendPosition(id, pos);
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
                    CreateNetId(instanciator_.InstancePlayerOne(), gameObjectType.player1); // player one created, send the clients the command create!!!
                }
                else if (type == gameObjectType.player2)
                {
                    CreateNetId(instanciator_.InstancePlayerTwo(), gameObjectType.player2); // same
                }
            }
        }

        public void SetPosition(int netID, Vector2 pos)
        {
            GameObject obj = null;
            gameObjectType type = gameObjectType.none;
            for (int i = 0; i < netIdList.Count; i++)
            {
                if (netIdList[i].netId == netID)
                {
                    obj = netIdList[i].gameObject;
                    type = netIdList[i].type;
                    break;
                }
            }

            if (obj == null)
            {
                Debug.Log("obj does not exist!!!");
                return;
            }

            Debug.Log("attempting shit");

            if (type == gameObjectType.player1 || type == gameObjectType.player2)
            {
                Debug.Log("changing player position");
                obj.GetComponent<PlayerMovementServer>().SetPosition(pos);
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
