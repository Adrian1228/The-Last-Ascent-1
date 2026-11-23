using UnityEngine;

// Va en el Player. Al cargar la escena (por ejemplo después de morir) lo coloca en el último checkpoint.
[RequireComponent(typeof(Rigidbody))]
public class PlayerRespawn : MonoBehaviour
{
    void Awake()
    {
        // Guarda la altura de salida original (solo la primera vez)
        CheckpointManager.RegisterStart(transform.position.y);

        if (!CheckpointManager.HasCheckpoint) return;

        Rigidbody rb = GetComponent<Rigidbody>();
        Vector3 pos = CheckpointManager.RespawnPosition;

        transform.position = pos;
        rb.position = pos;
        rb.linearVelocity = Vector3.zero; // En Unity 2022 o anterior usa: rb.velocity
        Physics.SyncTransforms();
    }
}
