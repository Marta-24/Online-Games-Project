using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts
{

    public class SpawnManager : MonoBehaviour
    {
        public GameObject idManager;
        public Instanciator instanciator_;
        public NetIdManager netIdManager_;
        public int timerBeforeSpawn;
        public int spawnTypePlayer;
        // Start is called before the first frame update
        void Start()
        {
            FindComponents();
            timerBeforeSpawn = 60;
            spawnTypePlayer = 1;
        }

        // Update is called once per frame
        void Update()
        {
            if (instanciator_ == null)
            {
                FindComponents();
            }
            if (timerBeforeSpawn > 0)
            {
                timerBeforeSpawn--;
            }
            else if (timerBeforeSpawn == 0)
            {
                timerBeforeSpawn--;
                if (spawnTypePlayer == 1)
                {
                    CreatePlayer1();
                }
            }
        }

        void CreatePlayer1()
        {
            GameObject obj = instanciator_.InstancePlayerOne();
            List<Component> list = new List<Component>();
            PlayerMovementServer a = obj.GetComponent<PlayerMovementServer>();
            list.Add(a);
            NetId id = netIdManager_.CreateNetId(obj, GameObjectType.player1, list); // player one created, send the clients the command create!!!
            netIdManager_.SendObject(id);
        }

        void CreatePlayer2(Vector2 pos)
        {
            GameObject obj = instanciator_.InstancePlayerTwo();
            List<Component> list = new List<Component>();
            PlayerMovementServer a = obj.GetComponent<PlayerMovementServer>();
            list.Add(a);
            NetId id = netIdManager_.CreateNetId(obj, GameObjectType.player1, list); // player one created, send the clients the command create!!!
            netIdManager_.SendObject(id);
        }

        void CreateEnemyGround()
        {
            Vector2 pos = new Vector2(2, -2);
            GameObject obj = instanciator_.InstanceEnemyPrefab(pos);

            List<Component> list = new List<Component>();
            EnemyScript b = obj.GetComponent<EnemyScript>();
            LifeSystem hp = obj.GetComponent<LifeSystem>();
            list.Add(b);
            list.Add(hp);

            NetId id = netIdManager_.CreateNetId(obj, GameObjectType.enemyGround, list);
            netIdManager_.SendObject(id);
        }

        void CreateEnemyFly()
        {
            Vector2 pos = new Vector2(2, -2);
            GameObject obj = instanciator_.InstanceEnemyFlyPrefab(pos);

            List<Component> list = new List<Component>();
            EnemyFlyScript b_ = obj.GetComponent<EnemyFlyScript>();
            LifeSystem hp_ = obj.GetComponent<LifeSystem>();
            list.Add(b_);
            list.Add(hp_);

            NetId id = netIdManager_.CreateNetId(obj, GameObjectType.enemyFly, list);
            netIdManager_.SendObject(id);
        }
        void FindComponents()
        {
            idManager = GameObject.FindWithTag("NetIdManager");
            instanciator_ = idManager.GetComponent<Instanciator>();
            netIdManager_ = idManager.GetComponent<NetIdManager>();
        }
    }
}