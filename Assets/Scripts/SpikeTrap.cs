using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Tooltip("Si está activo, mata al jugador de una vez. Si no, solo quita 'damage' corazones.")]
    public bool instantKill = true;
    public int damage = 1;

    void OnTriggerEnter(Collider other)
    {
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health == null) return;

        if (instantKill) health.Kill();
        else health.TakeDamage(damage, transform.position);
    }
}