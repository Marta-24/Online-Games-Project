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
        enemyGround = 3,
        enemyFly = 4,
        bullet = 5
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
        public SpawnManager spawn;
        public Instanciator instanciator_;
        public bool startEnded = false;
        public List<FutureObject> NeedToCreateList = new List<FutureObject>();
        public int frameCounter = 60;
        public bool sendReady = false;
        public GameObject bulletPrefab;
        public bool serverReady = false;
        public bool clientReady = false;
        public InformationBetweenScenes info;
        void Start()
        {
            //Generate Random instance

            AddServer();
            FindComponents();
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
                    CreateObject(NeedToCreateList[i].netID, NeedToCreateList[i].type, NeedToCreateList[i].pos, NeedToCreateList[i].direction);
                    NeedToCreateList[i].type = GameObjectType.none;
                }

                NeedToCreateList.Clear();
            }

            if (sendReady == false)
            {
                SendReadyToCreate();
                sendReady = true;
            }


            if (info.serverReady && info.clientReady)
            {

                info.serverReady = false;
                info.clientReady = false;
                ActivateSpawn();

            }
        }

        void SendReadyToCreate()
        {

            if (server != null)
            {
                info.serverReady = true;
                server.SendReadyToCreate();
            }
            if (client != null)
            {
                info.clientReady = true;
                client.SendReadyToCreate();
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

        void FindComponents()
        {
            instanciator_ = GetComponent<Instanciator>();

            GameObject obj = GameObject.Find("InformationBetweenScenes");
            info = obj.GetComponent<InformationBetweenScenes>();
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
            return id;
        }

        public NetId AddNetId(int intId, GameObject obj, GameObjectType type, List<Component> optionalList)
        {
            NetId id;

            id = new NetId(intId, obj, type, optionalList);
            netIdList.Add(id);
            return id;
        }

        public NetId AddNetId(int intId, GameObject obj, GameObjectType type)
        {
            NetId id;
            id = new NetId(intId, obj, type);

            netIdList.Add(id);
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
            int id = FindNetId(obj);
            if (id != -1)
            {

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

            Debug.Log("future object: " + netId + "" + type);
            NeedToCreateList.Add(new FutureObject(netId, pos, type));
        }

        public void StackObject(int netId, GameObjectType type, Vector2 pos, Vector2 direction)
        {
            Debug.Log("future object: " + netId + "" + type);
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

                    obj.GetComponent<PlayerMovementCopy>().enabled = true;
                    obj.GetComponent<PlayerMovement>().enabled = false;
                    Rigidbody2D body = obj.GetComponent<Rigidbody2D>();
                    body.bodyType = RigidbodyType2D.Kinematic;
                    PlayerMovementCopy a = obj.GetComponent<PlayerMovementCopy>();
                    list.Add(a);

                    AddNetId(netId, obj, GameObjectType.player1, list); // player one created, send the clients the command create!!!
                }
                else if (type == GameObjectType.player2)
                {
                    GameObject obj = instanciator_.InstancePlayerTwo();
                    List<Component> list = new List<Component>();

                    obj.GetComponent<PlayerMovementCopy>().enabled = true;
                    obj.GetComponent<PlayerMovement>().enabled = false;
                    Rigidbody2D body = obj.GetComponent<Rigidbody2D>();
                    body.bodyType = RigidbodyType2D.Kinematic;
                    PlayerMovementCopy a = obj.GetComponent<PlayerMovementCopy>();
                    list.Add(a);

                    AddNetId(netId, obj, GameObjectType.player2, list); // same
                }
                else if (type == GameObjectType.bullet)
                {

                    Vector3 vec = new Vector3(pos.x, pos.y, 0.0f);//new Vector3(pos.x, pos.y + 0.5f, 0.0f) * direction;
                    vec += new Vector3(0.4f, 0.0f, 0.0f) * direction.x;
                    GameObject obj = Instantiate(bulletPrefab, vec, Quaternion.identity);
                    BulletScript bulletScript = obj.GetComponent<BulletScript>();
                    bulletScript.Start_();

                    bulletScript.rb.velocity = 10f * direction * transform.right;

                    AddNetId(netId, obj, GameObjectType.bullet); // same
                }
                else if (type == GameObjectType.enemyGround)
                {
                    GameObject obj = instanciator_.InstanceEnemyPrefab(pos);

                    List<Component> list = new List<Component>();
                    EnemyScript a = obj.GetComponent<EnemyScript>();
                    LifeSystem hp = obj.GetComponent<LifeSystem>();
                    list.Add(a);
                    list.Add(hp);
                    AddNetId(netId, obj, GameObjectType.enemyGround, list);
                }
                else if (type == GameObjectType.enemyFly)
                {
                    GameObject obj = instanciator_.InstanceEnemyFlyPrefab(pos);

                    List<Component> list = new List<Component>();
                    EnemyFlyScript a = obj.GetComponent<EnemyFlyScript>();
                    LifeSystem hp = obj.GetComponent<LifeSystem>();
                    list.Add(a);
                    list.Add(hp);
                    AddNetId(netId, obj, GameObjectType.enemyFly, list);
                }
            }
        }

        public void SendObject(NetId id)
        {
            if (server != null) server.SendCreateObject(id.netId, id.type, new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f));
            if (client != null) client.SendCreateObject(id.netId, id.type, new Vector2(0.0f, 0.0f), new Vector2(0.0f, 0.0f));

        }
        public void SetPosition(int id, Vector2 pos)
        {
            NetId obj = FindObject(id);
            if (obj != null)
            {
                if (obj.compList != null || obj.compList.Count != 0)
                {
                    foreach (Component c in obj.compList)
                    {
                        if (c.GetType() == typeof(PlayerMovementCopy))
                        {
                            PlayerMovementCopy a = c as PlayerMovementCopy;
                            a.SetPosition(pos);
                        }
                        else if (c.GetType() == typeof(EnemyScript))
                        {

                        }
                        else if (c.GetType() == typeof(EnemyFlyScript))
                        {

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

            foreach (Component c in obj.compList)
            {
                if (c.GetType() == typeof(LifeSystem))
                {
                    LifeSystem a = c as LifeSystem;
                    a.ReceiveDamage(health);
                }
            }
        }

        public bool CheckConnection()
        {
            if (server != null) return true;
            //if (client != null) return false;

            return false;
        }

        public void ActivateSpawn()
        {
            spawn.SpawnLvl1();
        }
    }
}
