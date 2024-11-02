using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneClient(string Client)
    {
        SceneManager.LoadScene("Client");
    }
    public void LoadSceneServer(string Server)
    {
        SceneManager.LoadScene("Server");
    }
    public void LoadSceneMainMenu(string MainMenu)
    {
        SceneManager.LoadScene("MainMenu");
    }
}
