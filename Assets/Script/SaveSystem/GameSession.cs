using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSession
{
    private static PlayerData current;
    public static PlayerData Current
    {
        get
        {
            if (current == null)
            {
                if (SaveSystem.HasSave() == true)
                {
                    current = SaveSystem.LoadGame();
                }
                if (current == null)
                {
                    current = new PlayerData();
                }
            }
            return current;
        }
    }

    public static void StartNewGame()
    {
        SaveSystem.DeleteGame();
        current = new PlayerData();
    }

    public static void SetCurrent(PlayerData data)
    {
        if (data != null)
        {
            current = data;
        }
    }

    public static void RestartCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        Time.timeScale = 1f;
        StartNewGame();
        SceneManager.LoadScene(sceneName);
    }
}
