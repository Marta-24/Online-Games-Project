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
            AddServer();
            FindInstanciator();

            CreateNetId(instanciator_.InstancePlayerOne());
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

            SendNetId(id);
        }

        private int GenerateId()
        {
            return 0;
        }

        private void SendNetId(NetId id)
        {
            // Send to client this information se the lists are the same
        }
    }
}
