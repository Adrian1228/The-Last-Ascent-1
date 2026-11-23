using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MummyMelee : MonoBehaviour
{
    [Header("Detección")]
    public float detectionRange = 8f;          // Distancia horizontal a la que te ve
    public float maxHeightDifference = 3f;     // Diferencia de altura máxima (pisos)

    [Header("Movimiento y daño")]
    public float moveSpeed = 3f;
    public int damage = 1;

    Rigidbody rb;
    Transform player;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector3 toPlayer = player.position - transform.position;
        float heightDiff = Mathf.Abs(toPlayer.y);
        toPlayer.y = 0f;

        // Te persigue solo si estás cerca Y en un piso parecido
        bool canSeePlayer = toPlayer.magnitude <= detectionRange && heightDiff <= maxHeightDifference;

        Vector3 vel = rb.linearVelocity; // En Unity 2022 o anterior usa: rb.velocity

        if (canSeePlayer)
        {
            Vector3 dir = toPlayer.normalized;
            vel.x = dir.x * moveSpeed;
            vel.z = dir.z * moveSpeed;

            if (dir.sqrMagnitude > 0.01f)
                rb.MoveRotation(Quaternion.LookRotation(dir));
        }
        else
        {
            vel.x = 0f;
            vel.z = 0f;
        }

        rb.linearVelocity = vel; // En Unity 2022 o anterior usa: rb.velocity
    }

    // Daño al tocar. La invulnerabilidad la maneja PlayerHealth.
    void OnCollisionStay(Collision collision)
    {
        TryDamage(collision.gameObject);
    }

    void TryDamage(GameObject other)
    {
        var health = other.GetComponentInParent<PlayerHealth>();
        if (health != null) health.TakeDamage(damage, transform.position);
    }

    // Al seleccionar la momia, dibuja la zona de detección en la Scene
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(detectionRange * 2f, maxHeightDifference * 2f, detectionRange * 2f));
    }
}