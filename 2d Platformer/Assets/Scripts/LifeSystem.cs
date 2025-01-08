using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{

    public class LifeSystem : MonoBehaviour
    {
        int health = 0;
        public GameObject netIdManager;
        public NetIdManager netIdScript;
        GameObject parent;
        // Start is called before the first frame update
        void Start()
        {
            if (health == 0)
            {
                health = 100;
            }

            parent = gameObject;
            FindNetIdManager();
        }

        // Update is called once per frame
        void Update()
        {
            if (health <= 0)
            {
                Destroy(gameObject);
            }
        }

        public void TakeDamage(int dmg)
        {
            Debug.Log("taking damage");
            health -= dmg;

            netIdScript.TakeDamage(parent, dmg);
        }

        public void ReceiveDamage(int dmg)
        {
            health -= dmg;
        }

        public void FindNetIdManager()
        {
            netIdManager = GameObject.Find("NetIdManager");
            if (netIdManager != null) netIdScript = netIdManager.GetComponent<NetIdManager>();
        }
    }
}