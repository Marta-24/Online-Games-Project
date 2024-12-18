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
            /*GameObject player = new GameObject();
            player.name = "Player1_";
            
            SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
            renderer.
            player.GetComponent<PlayerMovement>();
            player.GetComponent<Rigidbody2D>();
            player.GetComponent<BoxCollider2D>();
            player.GetComponent<Animator>();
            return player;*/
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

        public GameObject InstanceEnemyPrefab()
        {
            return Instantiate(enemyPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        }

    }
}