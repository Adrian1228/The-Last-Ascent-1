using System.Collections;
using UnityEngine;
using TMPro;

// Va en el texto de la pantalla ("CHECKPOINT!"). Aparece, se queda un momento y se desvanece.
[RequireComponent(typeof(TMP_Text))]
public class CheckpointMessage : MonoBehaviour
{
    public float fadeInTime = 0.25f;
    public float holdTime = 1.5f;
    public float fadeOutTime = 0.6f;

    static CheckpointMessage instance;
    TMP_Text label;
    Coroutine routine;

    void Awake()
    {
        instance = this;
        label = GetComponent<TMP_Text>();
        SetAlpha(0f); // Empieza invisible (el objeto debe quedar ACTIVO)
    }

    public static void Show(string message)
    {
        if (instance == null) instance = FindFirstObjectByType<CheckpointMessage>();

        if (instance != null) instance.Play(message);
        else Debug.LogWarning("CheckpointMessage: no hay ningún texto con este script en la escena.");
    }

    void Play(string message)
    {
        label.text = message;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FadeRoutine());
    }

    IEnumerator FadeRoutine()
    {
        yield return Fade(0f, 1f, fadeInTime);
        yield return new WaitForSeconds(holdTime);
        yield return Fade(1f, 0f, fadeOutTime);
        routine = null;
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        SetAlpha(to);
    }

    void SetAlpha(float a)
    {
        Color c = label.color;
        c.a = a;
        label.color = c;
    }
}