using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts
{
    public class SceneLoader : MonoBehaviour
    {
        bool clientScene = false;
        NetIdManager idManager;
        Scene scene;
        void Start()
        {
            scene = SceneManager.GetActiveScene();
        }
        void Update()
        {
            if (clientScene)
            {
                clientScene = false;
                SceneManager.LoadScene("Scene1Client");
            }

            if (idManager == null && ((scene.name != "ServerScene") || (scene.name != "ClientScene") || (scene.name != "MainMenu")))
            {
                Debug.Log("Heeeeeelllooooooooo");
                FindManager();
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

        public void LoadScene01Client()
        {
            SceneManager.LoadScene("Scene1Client");
        }
        public void LoadScene01Server()
        {
            SceneManager.LoadScene("Scene1Server");
        }
        public void LoadScene01Client_()
        {
            clientScene = true;
        }

        public void ChangeToLevel(int level)
        {
            //Should call destroy everything minus players
            idManager.ChangeScenesSave();

            if (level == 1)
            {
                SceneManager.LoadScene("Scene1Server");
            }
            else if (level == 2)
            {
                SceneManager.LoadScene("Scene2Server");
            }
            else if (level == 3)
            {
                SceneManager.LoadScene("Scene3Server");
            }
        }

        void FindManager()
        {
            GameObject obj = GameObject.Find("NetIdManager");
            idManager = obj.GetComponent<NetIdManager>();
        }


    }
}