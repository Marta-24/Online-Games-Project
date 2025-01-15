using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts
{
    public class SceneLoader : MonoBehaviour
    {
        bool clientScene = false;
        public NetIdManager idManager;
        Scene scene;
        bool nextFrame = false;
        int nextFrameLevel;
        bool nextFramSpawnPlayer = false;
        void Start()
        {
            scene = SceneManager.GetActiveScene();
        }
        void Update()
        {
            if (nextFrame == true)
            {
                Debug.Log("Heeeeeelllooooooooo");
                nextFrame = false;
                ChangeToLevel(nextFrameLevel, false, nextFramSpawnPlayer);
            }

            if (clientScene)
            {
                clientScene = false;
                SceneManager.LoadScene("Scene1Client");
            }

            if (idManager == null)
            {
                if (scene.name != "MainMenu")
                {
                    Debug.Log(scene.name);
                    FindManager();
                }

            }


        }

        public void LoadSceneClient(string Client)
        {
            SceneManager.LoadScene("ClientScene");
        }
        public void LoadSceneServer(string Server)
        {
            SceneManager.LoadScene("ServerScene");
        }
        public void LoadSceneMainMenu(string MainMenu)
        {
            SceneManager.LoadScene("MainMenu");
        }
        public void LoadSceneWaitingRoom(string WaitingRoom)
        {
            SceneManager.LoadScene("WaitingRoom");
        }
        public void LoadScene01()
        {
            SceneManager.LoadScene("Scene1");
        }
        public void LoadScene01Client_()
        {
            clientScene = true;
        }

        public void ChangeToLevel(int level, bool notifyConnection, bool spawnPlayer) //if notify connection is true this trigger will be send online
        {
            //Should call destroy everything minus players
            if (idManager != null) idManager.ChangeScenesSave();

            idManager.ActivateSpawn(spawnPlayer);
            Debug.Log(level);

            if (level == 1)
            {
                SceneManager.LoadScene("Scene1");
            }
            else if (level == 2)
            {
                SceneManager.LoadScene("Scene2");
            }
            else if (level == 3)
            {
                SceneManager.LoadScene("Scene3");
            }

            //Change Scene in other computer
            if (notifyConnection) idManager.SendLevelChange(level, spawnPlayer);
        }

        public void NextFramChange(int level, bool notifyConnection, bool player) //if notify connection is true this trigger will be send online
        {
            Debug.Log("WORKED");
            nextFrame = true;
            nextFramSpawnPlayer = player;
            nextFrameLevel = level;
        }

        void FindManager()
        {
            GameObject obj = GameObject.Find("NetIdManager");
            idManager = obj.GetComponent<NetIdManager>();
        }


    }
}