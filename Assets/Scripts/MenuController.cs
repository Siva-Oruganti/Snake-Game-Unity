using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject readyPanel;

    public void ShowReadyPanel()
    {
        mainMenuPanel.SetActive(false);
        readyPanel.SetActive(true);
    }

    public void ShowMainMenu()
    {
        readyPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene"); // Make sure this matches your scene name
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
