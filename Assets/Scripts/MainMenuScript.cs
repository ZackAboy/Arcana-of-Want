using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    // Called by the Menu button
    public void StartGame()
    {
        SceneManager.LoadScene("TestScene");
    }
}
