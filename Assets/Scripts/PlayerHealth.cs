using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida (escarabajos)")]
    public int maxHearts = 5;
    [Tooltip("Con cuántos escarabajos empieza el jugador. 0 = un golpe y muere.")]
    public int startingHearts = 0;
    public float invulnerabilityTime = 1f;

    [Header("Parpadeo al recibir daño")]
    [Tooltip("Se llena solo con las mallas del personaje. Normalmente no hay que tocarlo.")]
    public Renderer[] renderersToBlink;
    public float blinkInterval = 0.1f;

    [Header("Retroceso (knockback)")]
    public float knockbackForce = 7f;
    public float knockbackUpForce = 3f;
    public float knockbackDuration = 0.2f;

    [Header("Temblor de cámara")]
    public float shakeDuration = 0.25f;
    public float shakeMagnitude = 0.2f;

    [Header("Sonido")]
    public AudioClip hitSound;
    [Range(0f, 1f)] public float hitVolume = 1f;

    public int CurrentHearts { get; private set; }
    public bool IsInvulnerable { get; private set; }

    // La UI se suscribe a esto: (vidaActual, vidaMaxima)
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    PlayerController controller;
    CameraOrbit cameraOrbit;
    bool isDead;

    void Awake()
    {
        CurrentHearts = Mathf.Clamp(startingHearts, 0, maxHearts);
        controller = GetComponent<PlayerController>();

        // Parpadea solo el personaje (así nunca reaparece la cápsula)
        var skinned = GetComponentsInChildren<SkinnedMeshRenderer>();
        if (skinned.Length > 0) renderersToBlink = skinned;
    }

    void Start()
    {
        cameraOrbit = FindFirstObjectByType<CameraOrbit>();
        OnHealthChanged?.Invoke(CurrentHearts, maxHearts);
    }

    // Daño sin dirección (sin retroceso)
    public void TakeDamage(int amount = 1)
    {
        ApplyDamage(amount, null);
    }

    // Daño con origen: empuja al jugador lejos de quien lo golpeó
    public void TakeDamage(int amount, Vector3 sourcePosition)
    {
        ApplyDamage(amount, sourcePosition);
    }

    void ApplyDamage(int amount, Vector3? source)
    {
        if (isDead || IsInvulnerable) return;

        PlayHitSound();

        // Sin escarabajos: un golpe y mueres
        if (CurrentHearts <= 0)
        {
            Die();
            return;
        }

        CurrentHearts = Mathf.Max(0, CurrentHearts - amount);
        OnHealthChanged?.Invoke(CurrentHearts, maxHearts);

        if (controller != null)
        {
            Vector3 dir = -transform.forward;
            if (source.HasValue)
            {
                dir = transform.position - source.Value;
                dir.y = 0f;
                if (dir.sqrMagnitude < 0.001f) dir = -transform.forward;
            }
            controller.ApplyKnockback(dir.normalized, knockbackForce, knockbackUpForce, knockbackDuration);
            controller.PlayHit(); // Animación de golpe (igual para cualquier enemigo)
        }

        if (cameraOrbit != null) cameraOrbit.Shake(shakeDuration, shakeMagnitude);

        StartCoroutine(InvulnerabilityRoutine());
    }

    // Muerte instantánea (trampas de pinchos, vacío, etc.). Ignora la invulnerabilidad.
    public void Kill()
    {
        if (isDead) return;

        CurrentHearts = 0;
        OnHealthChanged?.Invoke(CurrentHearts, maxHearts);
        Die();
    }

    public void Heal(int amount = 1)
    {
        if (isDead) return;

        CurrentHearts = Mathf.Min(maxHearts, CurrentHearts + amount);
        OnHealthChanged?.Invoke(CurrentHearts, maxHearts);
    }

    void PlayHitSound()
    {
        if (hitSound != null && Camera.main != null)
            AudioSource.PlayClipAtPoint(hitSound, Camera.main.transform.position, hitVolume);
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
        if (isDead) return;
        isDead = true;

        OnDeath?.Invoke();
        Debug.Log("El jugador murió");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}