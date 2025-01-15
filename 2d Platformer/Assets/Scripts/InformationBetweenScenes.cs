using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{

    public class InformationBetweenScenes : MonoBehaviour
    {
        public int typeOfPlayer;
        public bool isServer = false;

        // Start is called before the first frame update
        void Start()
        {
            typeOfPlayer = 2;
            CheckForServer();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void CheckForServer()
        {
            GameObject obj = GameObject.Find("ServerManager");
            if (obj != null) isServer = true;
            else
            {
                obj = GameObject.Find("ClientManager");
                if(obj != null) isServer = false;
            }

            Debug.Log("WE ARE LOOKIN FOR THIS" + isServer);
        }
    }
}