using UnityEngine;
using UnityEngine.SceneManagement; // Required for scene management

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
}
