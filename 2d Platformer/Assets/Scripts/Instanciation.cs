using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    public class Instance : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public GameObject InstancePlayerOne()
        {
            GameObject instance = new GameObject();
            return instance;
        }

        public GameObject InstancePlayerTwo()
        {
            GameObject instance = new GameObject();
            return instance;
        }
    }
}