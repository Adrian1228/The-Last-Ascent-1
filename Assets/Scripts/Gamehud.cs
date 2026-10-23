using UnityEngine;
using TMPro;

public class GameHUD : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;          // Arrastra al jugador
    public TMP_Text heightText;       // Texto de la altura
    public TMP_Text timeText;         // Texto del tiempo

    [Header("Altura")]
    [Tooltip("Si está activo muestra la altura MÁXIMA alcanzada. Si no, la altura actual.")]
    public bool showMaxHeight = false;

    [Header("Tiempo")]
    public bool timerRunning = true;

    public float ElapsedTime { get; private set; }
    public float CurrentHeight { get; private set; }
    public float MaxHeight { get; private set; }

    float startY;

    void Start()
    {
        if (player != null) startY = player.position.y;
        ElapsedTime = 0f;
        MaxHeight = 0f;
        UpdateTexts();
    }

    void Update()
    {
        // En pausa Time.timeScale = 0, entonces deltaTime es 0 y el tiempo se detiene solo
        if (timerRunning) ElapsedTime += Time.deltaTime;

        if (player != null)
        {
            CurrentHeight = Mathf.Max(0f, player.position.y - startY);
            if (CurrentHeight > MaxHeight) MaxHeight = CurrentHeight;
        }

        UpdateTexts();
    }

    void UpdateTexts()
    {
        if (heightText != null)
        {
            float h = showMaxHeight ? MaxHeight : CurrentHeight;
            heightText.text = "Altura: " + h.ToString("F1") + " m";
        }

        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(ElapsedTime / 60f);
            int seconds = Mathf.FloorToInt(ElapsedTime % 60f);
            timeText.text = "Tiempo: " + minutes.ToString("00") + ":" + seconds.ToString("00");
        }
    }

    public void StopTimer() { timerRunning = false; }
    public void ResetTimer() { ElapsedTime = 0f; timerRunning = true; }
}