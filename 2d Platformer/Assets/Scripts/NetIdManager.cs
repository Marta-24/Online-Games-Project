using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    public class NetId
    {
        public int netId;
        public GameObject gameObject;
        
        public NetId(int netId_, GameObject gameObject_)
        {
            this.netId = netId_;
            gameObject = gameObject_;
        }
    }
    public class NetIdManager : MonoBehaviour
    {
        public List<NetId> netIdList = new List<NetId>();
        public GameObject objectServer;
        public ServerTCP server;
        public GameObject netIdManagerGameObject;
        public Instanciator instanciator_;
        
        
        

        void Start()
        {
            //Generate Random instance

            AddServer();
            FindInstanciator();

            CreateNetId(instanciator_.InstancePlayerOne()); // player one created, send the clients the command create!!!
            CreateNetId(instanciator_.InstancePlayerTwo()); // same
        }

        // Update is called once per frame
        void Update()
        {

        }

        void AddServer()
        {
            objectServer = GameObject.Find("ServerManager");
            if (objectServer != null)
            {
                server = objectServer.GetComponent<ServerTCP>();

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

        public void CreateNetId(GameObject gameObject_)
        {
            NetId id = new NetId(GenerateId(), gameObject_);
            netIdList.Add(id);

            Debug.Log("Added new NetId with name and id:" + gameObject_.name + " " + id.netId);
        }

        private int GenerateId()
        {
            
            while(true)
            {
            bool sec = true;
            int num = Random.Range(0, int.MaxValue); //Generate random number

            //Check if number already exists 
            for(int i = 0; i < netIdList.Count; i++)
            {
                if(netIdList[i].netId == num)
                {
                    sec = false;
                }
            }
            
            if (sec) return num;
            }
        }
    }
}
