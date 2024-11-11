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

        void Start()
        {
            AddServer();

            if (server != null) // this is hardcoded, should not be here
            {
                // create players and assing Id´s
                // create all objects that require net id
                GameObject exampleOne = new GameObject();
                exampleOne.name = "GameObject1";
            }
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
