using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Escena del juego (nombre EXACTO)")]
    public string gameSceneName = "SampleScene";

    [Header("Botones")]
    public Button playButton;
    public Button quitButton;

    void Awake()
    {
        if (playButton != null) playButton.onClick.AddListener(PlayGame);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }

    void Start()
    {
        // Por si venimos del juego con el mouse bloqueado o el tiempo pausado
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}