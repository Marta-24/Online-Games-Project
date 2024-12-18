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
        public List<Component> compList;

        public NetId(int netId_, GameObject gameObject_, gameObjectType type)
        {
            this.netId = netId_;
            gameObject = gameObject_;
            this.type = type;
            compList = new List<Component>();
        }

        public NetId(int netId_, GameObject gameObject_, gameObjectType type, List<Component> compList)
        {
            this.netId = netId_;
            gameObject = gameObject_;
            this.type = type;
            this.compList = compList;
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

        public GameObject bulletPrefab;
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
                    GameObject obj = instanciator_.InstancePlayerOne();
                    List<Component> list = new List<Component>();
                    PlayerMovementServer a = obj.GetComponent<PlayerMovementServer>();
                    list.Add(a);

                    NetId id = CreateNetId(obj, gameObjectType.player1, list); // player one created, send the clients the command create!!!

                    server.SendCreateObject(id.netId, id.type, new Vector2(0.0f, 0.0f));


                    obj = instanciator_.InstancePlayerTwo();
                    list = new List<Component>();
                    a = obj.GetComponent<PlayerMovementServer>();
                    list.Add(a);

                    id = CreateNetId(obj, gameObjectType.player2, list); // same
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
        public NetId CreateNetId(GameObject obj, gameObjectType type, List<Component> list = null)
        {
            NetId id;

            if (list != null)
            {
                id = new NetId(GenerateId(), obj, type, list);
            }
            else
            {
                id = new NetId(GenerateId(), obj, type);
            }

            netIdList.Add(id);

            Debug.Log("Added new NetId with name and id:" + obj.name + " " + id.netId + " " + id.type);
            return id;
        }

        public NetId AddNetId(int intId, GameObject obj, gameObjectType type, List<Component> optionalList = null)
        {
            NetId id;
            if (optionalList.Count == 0 && (optionalList != null))
            {
                id = new NetId(intId, obj, type);
            }
            else
            {
                id = new NetId(intId, obj, type, optionalList);
            }

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

                    return netIdList[i];
                }
            }
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
                    GameObject obj = instanciator_.InstancePlayerOne();
                    List<Component> list = new List<Component>();
                    if (server != null)
                    {
                        obj.GetComponent<PlayerMovementServer>().enabled = false;
                        obj.GetComponent<PlayerMovement>().enabled = true;
                        PlayerMovement a = obj.GetComponent<PlayerMovement>();
                        list.Add(a);
                    }
                    if (client != null)
                    {
                        obj.GetComponent<PlayerMovementServer>().enabled = true;
                        obj.GetComponent<PlayerMovement>().enabled = false;
                        PlayerMovementServer a = obj.GetComponent<PlayerMovementServer>();
                        list.Add(a);
                        Debug.Log("adding playermovement to client");
                    }


                    AddNetId(netId, obj, gameObjectType.player1, list); // player one created, send the clients the command create!!!
                }
                else if (type == gameObjectType.player2)
                {
                    GameObject obj = instanciator_.InstancePlayerTwo();
                    List<Component> list = new List<Component>();
                    if (server != null)
                    {
                        obj.GetComponent<PlayerMovementServer>().enabled = true;
                        obj.GetComponent<PlayerMovement>().enabled = false;
                        PlayerMovementServer a = obj.GetComponent<PlayerMovementServer>();
                        list.Add(a);
                    }
                    if (client != null)
                    {
                        obj.GetComponent<PlayerMovementServer>().enabled = false;
                        obj.GetComponent<PlayerMovement>().enabled = true;
                        PlayerMovement a = obj.GetComponent<PlayerMovement>();
                        list.Add(a);
                    }





                    AddNetId(netId, obj, gameObjectType.player2, list); // same
                }
                else if (type == gameObjectType.bullet)
                {
                    Transform myTransform = new GameObject().transform;
                    myTransform.position = pos;
                    Instantiate(bulletPrefab, myTransform);
                }
            }
        }

        public void SetPosition(int id, Vector2 pos)
        {
            //Debug.Log("receiveing orders to move");
            NetId obj = FindObject(id);
            if (obj != null)
            {
                if (obj.compList != null || obj.compList.Count != 0)
                {
                    if (obj.type == gameObjectType.player1)
                    {
                        Debug.Log("player1is working");
                        if (client != null)
                        {
                            Debug.Log("this is working");
                            PlayerMovementServer a = obj.compList[0] as PlayerMovementServer;
                            a.SetPosition(pos);
                        }
                    }
                    else if (obj.type == gameObjectType.player2)
                    {
                        if (server != null)
                        {
                            PlayerMovementServer a = obj.compList[0] as PlayerMovementServer;
                            a.SetPosition(pos);
                        }
                    }
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
