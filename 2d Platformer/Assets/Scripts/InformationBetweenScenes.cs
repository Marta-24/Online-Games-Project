using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{

    public class InformationBetweenScenes : MonoBehaviour
    {
        public int typeOfPlayer;
        public bool clientReady = false;
        public bool serverReady = false;

        // Start is called before the first frame update
        void Start()
        {
            typeOfPlayer = 2;
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}