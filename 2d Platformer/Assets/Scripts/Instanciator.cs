using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{
    public class FutureInstance
    {
        public GameObject obj;
        public Vector3 pos;

        public FutureInstance(GameObject obj, Vector3 pos)
        {
            this.obj = obj;
            this.pos = pos;
        }
    }
    public class Instanciator : MonoBehaviour
    {
        public GameObject player1Prefab;
        public GameObject player2Prefab;
        public GameObject enemyPrefab;
        public GameObject UserPrefab;
        public List<FutureInstance> instances;
        
        // Start is called before the first frame update
        void Start()
        {
            instances = new List<FutureInstance>();
        }

        // Update is called once per frame
        void Update()
        {
            if (instances.Count != 0)
            {
                foreach(FutureInstance instance in instances)
                {
                    CreateInstance(instance);
                }
                instances.Clear();
            }
        }

        public void CreateInstance(FutureInstance instance)
        {
            Instantiate(instance.obj, instance.pos, Quaternion.identity);
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

        public GameObject InstanceEnemyPrefab(Vector2 pos)
        {
            return Instantiate(enemyPrefab, new Vector3(pos.x, pos.y, 0.0f), Quaternion.identity);
        }

        public GameObject IntanceUserPrefab()
        {
            FutureInstance ins = new FutureInstance(UserPrefab, new Vector3(0, 0, 0));
            instances.Add(ins);
            return null;
        }
    }
}