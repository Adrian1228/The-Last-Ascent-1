using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
 
public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    public int maxHearts = 5;
    public float invulnerabilityTime = 1f;
 
    [Header("Feedback (opcional)")]
    [Tooltip("Renderer del jugador para que parpadee al recibir daño")]
    public Renderer[] renderersToBlink;
    public float blinkInterval = 0.1f;
 
    public int CurrentHearts { get; private set; }
    public bool IsInvulnerable { get; private set; }
 
    // La UI se suscribe a esto: (vidaActual, vidaMaxima)
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;
 
    void Awake()
    {
        CurrentHearts = maxHearts;
    }
 
    void Start()
    {
        OnHealthChanged?.Invoke(CurrentHearts, maxHearts);
    }
 
    public void TakeDamage(int amount = 1)
    {
        Debug.Log("TakeDamage llamado. Corazones: " + CurrentHearts + " Invulnerable: " + IsInvulnerable);
        if (IsInvulnerable || CurrentHearts <= 0) return;
 
        CurrentHearts = Mathf.Max(0, CurrentHearts - amount);
        OnHealthChanged?.Invoke(CurrentHearts, maxHearts);
 
        if (CurrentHearts <= 0)
        {
            Die();
            return;
        }
 
        StartCoroutine(InvulnerabilityRoutine());
    }
 
    public void Heal(int amount = 1)
    {
        CurrentHearts = Mathf.Min(maxHearts, CurrentHearts + amount);
        OnHealthChanged?.Invoke(CurrentHearts, maxHearts);
    }
 
    IEnumerator InvulnerabilityRoutine()
    {
        IsInvulnerable = true;
        float timer = 0f;
        bool visible = true;
 
        while (timer < invulnerabilityTime)
        {
            visible = !visible;
            SetRenderersVisible(visible);
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }
 
        SetRenderersVisible(true);
        IsInvulnerable = false;
    }
 
    void SetRenderersVisible(bool visible)
    {
        if (renderersToBlink == null) return;
        foreach (var r in renderersToBlink)
            if (r != null) r.enabled = visible;
    }
 
    void Die()
    {
        OnDeath?.Invoke();
        Debug.Log("El jugador murió");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}