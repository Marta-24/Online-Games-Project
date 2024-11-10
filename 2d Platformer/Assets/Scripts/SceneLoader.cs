using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
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
}
