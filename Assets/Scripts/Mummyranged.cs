using UnityEngine;
 
[RequireComponent(typeof(Rigidbody))]
public class MummyRanged : MonoBehaviour
{
    [Header("Comportamiento")]
    public float detectionRange = 15f;
    public float preferredDistance = 8f;   // Distancia a la que le gusta quedarse
    public float moveSpeed = 2.5f;
 
    [Header("Ataque")]
    public GameObject daggerPrefab;
    public Transform firePoint;            // Opcional: si es null dispara desde el centro
    public float fireRate = 2f;            // Segundos entre disparos
    public bool requireLineOfSight = true;
    public LayerMask lineOfSightMask = ~0;
 
    Rigidbody rb;
    Transform player;
    float nextFireTime;
 
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
        Vector3 flat = new Vector3(toPlayer.x, 0f, toPlayer.z);
        float dist = flat.magnitude;
 
        Vector3 vel = rb.linearVelocity; // En Unity 2022 o anterior usa: rb.velocity
        vel.x = 0f;
        vel.z = 0f;
 
        if (dist <= detectionRange)
        {
            Vector3 dir = flat.normalized;
            rb.MoveRotation(Quaternion.LookRotation(dir));
 
            // Se acerca si está lejos, se aleja si el jugador se le pega
            if (dist > preferredDistance + 1f)
            {
                vel.x = dir.x * moveSpeed;
                vel.z = dir.z * moveSpeed;
            }
            else if (dist < preferredDistance - 1f)
            {
                vel.x = -dir.x * moveSpeed;
                vel.z = -dir.z * moveSpeed;
            }
 
            if (Time.time >= nextFireTime && HasLineOfSight())
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
 
        rb.linearVelocity = vel; // En Unity 2022 o anterior usa: rb.velocity
    }
 
    bool HasLineOfSight()
    {
        if (!requireLineOfSight) return true;
 
        Vector3 origin = GetFirePosition();
        Vector3 dir = player.position - origin;
 
        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dir.magnitude, lineOfSightMask, QueryTriggerInteraction.Ignore))
            return hit.collider.GetComponentInParent<PlayerHealth>() != null;
 
        return true;
    }
 
    Vector3 GetFirePosition()
    {
        return firePoint != null ? firePoint.position : transform.position + transform.forward * 0.8f;
    }
 
    void Shoot()
    {
        if (daggerPrefab == null) return;
 
        Vector3 origin = GetFirePosition();
        Vector3 dir = (player.position - origin).normalized;
 
        Instantiate(daggerPrefab, origin, Quaternion.LookRotation(dir));
    }
}