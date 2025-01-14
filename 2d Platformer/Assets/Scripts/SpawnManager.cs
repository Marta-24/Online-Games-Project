using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts
{

    public class SpawnManager : MonoBehaviour
    {
        public GameObject idManager;
        public Instanciator instanciator_;
        public NetIdManager netIdManager_;
        public InformationBetweenScenes InformationBetweenScenes_;
        public int timerBeforeSpawn;
        public int spawnTypePlayer;
        public bool connectionType;
        public int framesForSpawn = -1;
        // Start is called before the first frame update
        void Start()
        {
            connectionType = false;
            //FindComponents();
            timerBeforeSpawn = 450;
            spawnTypePlayer = 1;
        }

        // Update is called once per frame
        void Update()
        {
            if (framesForSpawn >= 0)
            {
                Debug.Log(framesForSpawn);
                framesForSpawn--;
            }
            else if (framesForSpawn == 0)
            {
                framesForSpawn--;
                Debug.Log("S{AASDGA{}W{EFOA}{ERPGA}E{RG}}");
                //Check information

                connectionType = netIdManager_.CheckConnection();
                ActivateSpawn();
            }

            if (instanciator_ == null)
            {
                FindComponents();
            }

            if (InformationBetweenScenes_ == null)
            {
                FindInfo();
            }


        }

        public void SpawnPlayers()
        {

        }

        public void ActivateSpawn()
        {
            Scene scene = SceneManager.GetActiveScene();

            if (scene.name == "Scene1Server")
            {
                SpawnLvl1();
            }
            else if (scene.name == "Scene2Server")
            {
                SpawnLvl2();
            }
            else if (scene.name == "Scene3Server")
            {
                SpawnLvl3();
            }
        }
        private void SpawnLvl1()
        {
            /*spawnTypePlayer = InformationBetweenScenes_.typeOfPlayer;
            if (spawnTypePlayer == 1)
            {
                CreatePlayer1();
            }
            else if (spawnTypePlayer == 2)
            {
                CreatePlayer2();
            }*/
            Debug.Log("creating");
            CreatePlayer2();
            if (connectionType)
            {
                CreateEnemyGround(new Vector2(6.0f, -2.0f));
                CreateEnemyGround(new Vector2(25.0f, 0.0f));
                CreateEnemyGround(new Vector2(21.0f, -3.0f));

                CreateEnemyFly(new Vector2(33.0f, 1.0f));
            }
        }

        public void SpawnLvl2()
        {
            Debug.Log("SpawnLVL2 working!!!!!!!!!!!!!!!!!!!");
        }

        public void SpawnLvl3()
        {

        }
        void CreatePlayer1()
        {
            GameObject obj = instanciator_.InstancePlayerOne();
            List<Component> list = new List<Component>();
            PlayerMovement a = obj.GetComponent<PlayerMovement>();
            list.Add(a);
            NetId id = netIdManager_.CreateNetId(obj, GameObjectType.player1, list); // player one created, send the clients the command create!!!
            netIdManager_.SendObject(id);
        }

        void CreatePlayer2()
        {
            GameObject obj = instanciator_.InstancePlayerTwo();
            List<Component> list = new List<Component>();
            PlayerMovement a = obj.GetComponent<PlayerMovement>();
            list.Add(a);
            NetId id = netIdManager_.CreateNetId(obj, GameObjectType.player2, list); // player two created, send the clients the command create!!!
            netIdManager_.SendObject(id);
        }

        void CreateEnemyGround(Vector2 pos)
        {
            GameObject obj = instanciator_.InstanceEnemyPrefab(pos);

            List<Component> list = new List<Component>();
            EnemyScript b = obj.GetComponent<EnemyScript>();
            LifeSystem hp = obj.GetComponent<LifeSystem>();
            list.Add(b);
            list.Add(hp);

            NetId id = netIdManager_.CreateNetId(obj, GameObjectType.enemyGround, list);
            netIdManager_.SendObject(id);
        }

        void CreateEnemyFly(Vector2 pos)
        {
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
            string name = SceneManager.GetActiveScene().name;
            if (name == "MainMenu")
            {
            idManager = GameObject.FindWithTag("NetIdManager");
            netIdManager_ = idManager.GetComponent<NetIdManager>();

            
                GameObject objServer = GameObject.Find("ServerManager");
                if (objServer == null) objServer = GameObject.Find("ClientManager");

                instanciator_ = objServer.GetComponent<Instanciator>();
            }
        }
        void FindInfo()
        {
            InformationBetweenScenes_ = GameObject.Find("InformationBetweenScenes").GetComponent<InformationBetweenScenes>();
            if (InformationBetweenScenes_ != null)
            {
                Debug.Log("connectiontype should work!!!!" + InformationBetweenScenes_.isServer);
                connectionType = InformationBetweenScenes_.isServer;
            }
        }
    }
}