using UnityEngine;

// Va en el objeto que tiene el collider "Is Trigger" del checkpoint
public class Checkpoint : MonoBehaviour
{
    [Header("Mensaje")]
    public string message = "CHECKPOINT!";

    [Header("Efecto al activarse (opcional)")]
    [Tooltip("Una luz, partículas o cualquier objeto que empiece APAGADO y se enciende al activar")]
    public GameObject activatedEffect;

    [Header("Sonido (opcional)")]
    public AudioClip activateSound;
    [Range(0f, 1f)] public float volume = 1f;

    Vector3Int id;

    void Awake()
    {
        // Cada checkpoint se identifica por su posición en el mapa
        id = Vector3Int.RoundToInt(transform.position);
    }

    void Start()
    {
        // Si ya estaba activado (después de reaparecer), se queda encendido
        if (activatedEffect != null)
            activatedEffect.SetActive(CheckpointManager.IsActivated(id));
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        if (player == null) return;
        if (CheckpointManager.IsActivated(id)) return; // Ya activado: no hace nada

        // Reaparecerá justo donde el jugador lo tocó, apoyado en el suelo
        Vector3 spawn = player.transform.position;
        if (Physics.Raycast(spawn, Vector3.down, out RaycastHit hit, 10f, ~0, QueryTriggerInteraction.Ignore))
            spawn = hit.point + Vector3.up * 1.1f;

        CheckpointManager.Activate(id, spawn);

        if (activatedEffect != null) activatedEffect.SetActive(true);
        CheckpointMessage.Show(message);

        if (activateSound != null && Camera.main != null)
            AudioSource.PlayClipAtPoint(activateSound, Camera.main.transform.position, volume);
    }
}