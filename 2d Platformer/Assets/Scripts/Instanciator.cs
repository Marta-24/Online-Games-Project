using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    public class Instanciator : MonoBehaviour
    {
        public GameObject player1Prefab;
        public GameObject player2Prefab;
        public GameObject enemyPrefab;
        public GameObject enemyFlyPrefab;

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
            GameObject obj = Instantiate(player1Prefab, new Vector3(0, 0, 0), Quaternion.identity);
            obj.name = "Player1";
            return obj;
        }

        public GameObject InstancePlayerTwo()
        {
          
            
            GameObject obj =  Instantiate(player2Prefab, new Vector3(0, 0, 0), Quaternion.identity);
            obj.name = "Player2";
            return obj;
        }

        public GameObject InstanceEnemyPrefab(Vector2 pos)
        {
            return Instantiate(enemyPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity);
        }

        public GameObject InstanceEnemyFlyPrefab(Vector2 pos)
        {
            return Instantiate(enemyFlyPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity);
        }
    }
}