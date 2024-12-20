using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    public enum GameObjectType : int
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
        public GameObjectType type;
        public List<Component> compList;

        public NetId(int netId_, GameObject gameObject_, GameObjectType type)
        {
            this.netId = netId_;
            gameObject = gameObject_;
            this.type = type;
            compList = new List<Component>();
        }

        public NetId(int netId_, GameObject gameObject_, GameObjectType type, List<Component> compList)
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
        public Vector2 direction;
        public GameObjectType type;

        public FutureObject(int netID, Vector2 pos, GameObjectType type)
        {
            this.netID = netID;
            this.pos = pos;
            this.type = type;
            direction = new Vector2(0.0f, 0.0f);
        }

        public FutureObject(int netID, Vector2 pos, GameObjectType type, Vector2 direction)
        {
            this.netID = netID;
            this.pos = pos;
            this.type = type;
            this.direction = direction;
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
        }

        // Update is called once per frame
        void Update()
        {
            if (NeedToCreateList.Count != 0)
            {
                for (int i = 0; i < NeedToCreateList.Count; i++)
                {
                    Debug.Log(NeedToCreateList[i].pos);
                    CreateObject(NeedToCreateList[i].netID, NeedToCreateList[i].type, NeedToCreateList[i].pos, NeedToCreateList[i].direction);
                    NeedToCreateList[i].type = GameObjectType.none;
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
                    NetId id = CreateNetId(obj, GameObjectType.player1, list); // player one created, send the clients the command create!!!
                    server.SendCreateObject(id.netId, id.type, new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f));


                    obj = instanciator_.InstancePlayerTwo();
                    list = new List<Component>();
                    a = obj.GetComponent<PlayerMovementServer>();
                    Rigidbody2D body = obj.GetComponent<Rigidbody2D>();
                    body.bodyType = RigidbodyType2D.Kinematic;
                    list.Add(a);
                    id = CreateNetId(obj, GameObjectType.player2, list); // same
                    server.SendCreateObject(id.netId, id.type, new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f));

                    Vector2 pos = new Vector2(2, -2);
                    obj = instanciator_.InstanceEnemyPrefab(pos);

                    list = new List<Component>();
                    EnemyScript b = obj.GetComponent<EnemyScript>();
                    list.Add(b);

                    id = CreateNetId(obj, GameObjectType.enemy, list);
                    server.SendCreateObject(id.netId, id.type, pos, new Vector2(0.0f, 0.0f));
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
                if (GameObject.ReferenceEquals(obj, netIdList[i].gameObject)) //this is for the moment
                {
                    return netIdList[i].netId;
                }
            }

            return -1;
        }
        public NetId CreateNetId(GameObject obj, GameObjectType type, List<Component> list = null)
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

        public NetId AddNetId(int intId, GameObject obj, GameObjectType type, List<Component> optionalList)
        {
            NetId id;

            id = new NetId(intId, obj, type, optionalList);
            netIdList.Add(id);

            Debug.Log("Added new NetId with name and id:" + obj.name + " " + id.netId + " " + id.type);
            return id;
        }

        public NetId AddNetId(int intId, GameObject obj, GameObjectType type)
        {
            NetId id;
            Debug.Log("WORKS CORRECTLU");
            id = new NetId(intId, obj, type);


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

        public void CreateBullet(GameObject obj, Vector2 pos, int dir)
        {
            int id = GenerateId();
            Vector2 direction = new Vector2((float)dir, 0.0f);

            CreateNetId(obj, GameObjectType.bullet);

            if (server != null) server.SendCreateObject(id, GameObjectType.bullet, pos, direction);
            if (client != null) client.SendCreateObject(id, GameObjectType.bullet, pos, direction);
        }

        public void StackObject(int netId, GameObjectType type, Vector2 pos)
        {
            Debug.Log("object stacked: " + ((int)type));
            NeedToCreateList.Add(new FutureObject(netId, pos, type));
        }

        public void StackObject(int netId, GameObjectType type, Vector2 pos, Vector2 direction)
        {
            Debug.Log("object stacked: " + ((int)type) + "this is the weird one");
            NeedToCreateList.Add(new FutureObject(netId, pos, type, direction));
        }

        public void CreateObject(int netId, GameObjectType type, Vector2 pos, Vector2 direction)
        {
            if (startEnded)
            {
                if (type == GameObjectType.player1)
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
                        Rigidbody2D body = obj.GetComponent<Rigidbody2D>();
                        body.bodyType = RigidbodyType2D.Kinematic;
                        PlayerMovementServer a = obj.GetComponent<PlayerMovementServer>();
                        list.Add(a);

                        Debug.Log("adding playermovement to client");
                    }


                    AddNetId(netId, obj, GameObjectType.player1, list); // player one created, send the clients the command create!!!
                }
                else if (type == GameObjectType.player2)
                {
                    GameObject obj = instanciator_.InstancePlayerTwo();
                    List<Component> list = new List<Component>();
                    if (server != null)
                    {
                        obj.GetComponent<PlayerMovementServer>().enabled = true;
                        obj.GetComponent<PlayerMovement>().enabled = false;
                        Rigidbody2D body = obj.GetComponent<Rigidbody2D>();
                        body.bodyType = RigidbodyType2D.Kinematic;
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

                    AddNetId(netId, obj, GameObjectType.player2, list); // same
                }
                else if (type == GameObjectType.bullet)
                {

                    Vector3 vec = new Vector3(pos.x, pos.y, 0.0f);//new Vector3(pos.x, pos.y + 0.5f, 0.0f) * direction;
                    Debug.Log(vec.x + " " + vec.y + "" + direction);
                    vec += new Vector3(0.4f, 0.0f, 0.0f) * direction.x;
                    Debug.Log(vec.x + " " + vec.y + "second time");
                    GameObject obj = Instantiate(bulletPrefab, vec, Quaternion.identity);
                    BulletScript bulletScript = obj.GetComponent<BulletScript>();
                    bulletScript.Start_();
                    //Debug.Log(direction);
                    bulletScript.rb.velocity = 10f * direction * transform.right;

                    AddNetId(netId, obj, GameObjectType.bullet); // same
                }
                else if (type == GameObjectType.enemy)
                {
                    //Debug.Log("this is triggering an enourmous amount of times");
                    GameObject obj = instanciator_.InstanceEnemyPrefab(pos);

                    List<Component> list = new List<Component>();
                    EnemyScript a = obj.GetComponent<EnemyScript>();
                    list.Add(a);
                    AddNetId(netId, obj, GameObjectType.enemy, list);
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
                    if (obj.type == GameObjectType.player1)
                    {
                        if (client != null)
                        {
                            PlayerMovementServer a = obj.compList[0] as PlayerMovementServer;
                            a.SetPosition(pos);
                        }
                    }
                    else if (obj.type == GameObjectType.player2)
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

        public void TakeDamage(GameObject obj, int health)
        {
            int id = FindNetId(obj);

            if (server != null) server.SendDamage(id, health);
            if (client != null) client.SendDamage(id, health);
        }

        public void GiveDamage(int id, int health)
        {
            NetId obj = FindObject(id);

            EnemyScript a = obj.compList[0] as EnemyScript;
            a.ReceiveDamage(health);
        }
    }
}
