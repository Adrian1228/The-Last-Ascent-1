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

    [Header("Defensa (espada)")]
    [Tooltip("Ángulo máximo, desde el frente del personaje, en el que se bloquea un golpe")]
    public float blockAngle = 100f;
    [Tooltip("Si el golpe llega sin posición de origen, se bloquea igual cuando estás defendiendo")]
    public bool blockHitsWithoutSource = true;
    public bool debugBlock = true;

    [Header("Muerte")]
    [Tooltip("Segundos que se espera antes de reiniciar, para ver la animación de muerte.")]
    public float deathRestartDelay = 3f;
    [Tooltip("Nombre del Trigger de muerte en el Animator.")]
    public string deathTrigger = "Die";

    public int CurrentHearts { get; private set; }
    public bool IsInvulnerable { get; private set; }

    // La UI se suscribe a esto: (vidaActual, vidaMaxima)
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    PlayerController controller;
    PlayerCombat combat;
    CameraOrbit cameraOrbit;
    Animator animator;
    Rigidbody rb;
    CapsuleCollider capsule;
    Coroutine invulnerabilityCoroutine;
    bool isDead;

    void Awake()
    {
        CurrentHearts = Mathf.Clamp(startingHearts, 0, maxHearts);
        controller = GetComponent<PlayerController>();
        combat = GetComponent<PlayerCombat>();
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        // El Animator está en el modelo (T-Pose), que es hijo del Player
        animator = GetComponentInChildren<Animator>();

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

    // Devuelve true si la defensa con la espada anula el golpe
    bool TryBlock(Vector3? source)
    {
        // PlayerCombat se puede agregar en pleno juego (al recoger la espada)
        if (combat == null) combat = GetComponent<PlayerCombat>();
        bool blocking = combat != null && combat.IsBlocking;

        if (debugBlock)
            Debug.Log($"PlayerHealth: golpe recibido. defendiendo={blocking}, origen={(source.HasValue ? "sí" : "ninguno")}");

        if (!blocking) return false;

        if (source.HasValue)
        {
            Transform facing = animator != null ? animator.transform : transform;
            Vector3 toSource = source.Value - transform.position;
            toSource.y = 0f;
            float angle = Vector3.Angle(facing.forward, toSource);

            if (debugBlock)
                Debug.Log($"PlayerHealth: ángulo del golpe = {angle:F0}° (bloquea si es menor de {blockAngle:F0}°)");

            if (angle > blockAngle) return false;
        }
        else if (!blockHitsWithoutSource)
        {
            return false;
        }

        if (cameraOrbit != null) cameraOrbit.Shake(0.1f, 0.05f);
        if (debugBlock) Debug.Log("PlayerHealth: ¡golpe bloqueado!");
        return true;
    }

    void ApplyDamage(int amount, Vector3? source)
    {
        if (isDead || IsInvulnerable) return;

        // Defensa con la espada: anula el golpe
        if (TryBlock(source)) return;

        PlayHitSound();

        // Sin escarabajos: un golpe y mueres
        if (CurrentHearts <= 0)
        {
            Die(source);
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

        invulnerabilityCoroutine = StartCoroutine(InvulnerabilityRoutine());
    }

    // Muerte instantánea (trampas de pinchos, vacío, etc.). Ignora la invulnerabilidad y la defensa.
    public void Kill()
    {
        if (isDead) return;

        if (debugBlock) Debug.Log("PlayerHealth: Kill() llamado (muerte instantánea, no se puede bloquear)");

        CurrentHearts = 0;
        OnHealthChanged?.Invoke(CurrentHearts, maxHearts);
        Die(null);
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

    void Die(Vector3? source)
    {
        if (isDead) return;
        isDead = true;

        // Corta el parpadeo y deja el modelo visible para ver la animación
        if (invulnerabilityCoroutine != null) StopCoroutine(invulnerabilityCoroutine);
        SetRenderersVisible(true);
        IsInvulnerable = true;

        // Bloquea el control y frena al personaje
        if (controller != null) controller.enabled = false;
        if (rb != null) rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

        // Gira el modelo hacia el enemigo: así la caída "hacia atrás" lo aleja de él
        if (source.HasValue && animator != null)
        {
            Vector3 toSource = source.Value - transform.position;
            toSource.y = 0f;
            if (toSource.sqrMagnitude > 0.001f)
                animator.transform.rotation = Quaternion.LookRotation(toSource.normalized, Vector3.up);
        }

        // Achica la cápsula hacia abajo para que el cuerpo acostado quede en el piso
        ShrinkColliderToFloor();

        // Dispara la animación de muerte
        if (animator != null)
        {
            animator.ResetTrigger("Hit");
            animator.SetTrigger(deathTrigger);
        }

        OnDeath?.Invoke();
        Debug.Log("El jugador murió");

        StartCoroutine(RestartAfterDelay());
    }

    void ShrinkColliderToFloor()
    {
        if (capsule == null) return;

        float bottom = capsule.center.y - capsule.height * 0.5f; // punto más bajo actual
        capsule.height = capsule.radius * 2f;                    // altura mínima posible
        capsule.center = new Vector3(capsule.center.x, bottom + capsule.radius, capsule.center.z);
    }

    IEnumerator RestartAfterDelay()
    {
        // Realtime: sigue contando aunque Time.timeScale sea 0
        yield return new WaitForSecondsRealtime(deathRestartDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}