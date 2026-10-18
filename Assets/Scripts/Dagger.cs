using UnityEngine;
 
[RequireComponent(typeof(Rigidbody))]
public class Dagger : MonoBehaviour
{
    public float speed = 12f;
    public int damage = 1;
    public float lifetime = 5f;
 
    void Awake()
    {
        var rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
    }
 
    void Start()
    {
        Destroy(gameObject, lifetime);
    }
 
    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }
 
    void OnTriggerEnter(Collider other)
    {
        // Ignora a otras momias y otras dagas
        if (other.GetComponentInParent<MummyRanged>() != null) return;
        if (other.GetComponentInParent<MummyMelee>() != null) return;
        if (other.GetComponent<Dagger>() != null) return;
 
        var health = other.GetComponentInParent<PlayerHealth>();
        if (health != null) health.TakeDamage(damage, transform.position);
 
        Destroy(gameObject); // Se destruye al pegar con el jugador o con una pared
    }
}