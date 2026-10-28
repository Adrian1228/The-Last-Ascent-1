using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    public int maxHearts = 5;
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

    public int CurrentHearts { get; private set; }
    public bool IsInvulnerable { get; private set; }

    // La UI se suscribe a esto: (vidaActual, vidaMaxima)
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    PlayerController controller;
    CameraOrbit cameraOrbit;

    void Awake()
    {
        CurrentHearts = maxHearts;
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
        if (IsInvulnerable || CurrentHearts <= 0) return;

        CurrentHearts = Mathf.Max(0, CurrentHearts - amount);
        OnHealthChanged?.Invoke(CurrentHearts, maxHearts);

        if (CurrentHearts <= 0)
        {
            Die();
            return;
        }

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
        }

        if (cameraOrbit != null) cameraOrbit.Shake(shakeDuration, shakeMagnitude);

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