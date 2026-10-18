using UnityEngine;
 
[RequireComponent(typeof(Rigidbody))]
public class MummyMelee : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float detectionRange = 12f;
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
        toPlayer.y = 0f;
 
        Vector3 vel = rb.linearVelocity; // En Unity 2022 o anterior usa: rb.velocity
 
        if (toPlayer.magnitude <= detectionRange)
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
 
    // Daño al tocar. La invulnerabilidad de 1s la maneja PlayerHealth.
    void OnCollisionStay(Collision collision)
    {
        TryDamage(collision.gameObject);
    }
 
    void TryDamage(GameObject other)
    {
        var health = other.GetComponentInParent<PlayerHealth>();
        if (health != null) health.TakeDamage(damage, transform.position);
    }
}