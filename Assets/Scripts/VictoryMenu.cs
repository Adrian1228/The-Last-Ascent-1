
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject victoryPanel;
    public Button restartButton;
    public Button mainMenuButton;

    [Header("Escenas (nombre EXACTO)")]
    public string mainMenuSceneName = "MainMenu";

    private bool victoryActivated = false;

    void Awake()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    void Start()
    {
        // Al empezar el juego la victoria NO se ve
        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    void OnTriggerEnter(Collider other)
    {
        if (victoryActivated) return;

        // Comprobar si el jugador tocó el cofre
        if (other.CompareTag("Player") ||
            other.transform.root.CompareTag("Player"))
        {
            Win();
        }
    }

    public void Win()
    {
        if (victoryActivated) return;

        victoryActivated = true;

        // Mostrar pantalla de victoria
        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        // Desactivar menú de pausa
        PauseMenu pauseMenu = FindObjectOfType<PauseMenu>();

        if (pauseMenu != null)
            pauseMenu.enabled = false;

        // Congelar el juego
        Time.timeScale = 0f;

        // Mostrar cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("¡VICTORIA!");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(mainMenuSceneName);
    }
}
