using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Curación")]
    public int healAmount = 1;
    [Tooltip("Si está activo, no se recoge cuando el jugador ya tiene la vida llena")]
    public bool onlyIfHurt = true;

    [Header("Flotar")]
    public float bobHeight = 0.25f;   // Qué tanto sube y baja
    public float bobSpeed = 2f;       // Qué tan rápido

    [Header("Girar")]
    public float rotateSpeed = 90f;   // Grados por segundo (0 = no gira)

    Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Sube y baja con una onda seno
        float offset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = startPosition + Vector3.up * offset;

        // Gira sobre el eje vertical del mundo
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health == null) return;

        if (onlyIfHurt && health.CurrentHearts >= health.maxHearts) return;

        health.Heal(healAmount);
        Destroy(gameObject);
    }
}