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
    [Tooltip("Segundos entre lanzamientos. Debe ser mayor que lo que dura la animación (2.2 s).")]
    public float fireRate = 3f;
    public bool requireLineOfSight = true;
    public LayerMask lineOfSightMask = ~0;

    [Header("Animación")]
    [Tooltip("Segundos que se queda quieta lanzando (duración de la animación).")]
    public float throwLockTime = 2.2f;
    public string throwTrigger = "Throw";

    Rigidbody rb;
    Animator animator;
    Transform player;
    float nextFireTime;
    float throwEndTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // El Animator está en el modelo, que es hijo de esta momia
        animator = GetComponentInChildren<Animator>();
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

            bool isThrowing = Time.time < throwEndTime;

            // Mientras lanza se queda quieta; si no, se acerca o se aleja
            if (!isThrowing)
            {
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
            }

            if (!isThrowing && Time.time >= nextFireTime && HasLineOfSight())
            {
                StartThrow();
                nextFireTime = Time.time + fireRate;
            }
        }

        rb.linearVelocity = vel; // En Unity 2022 o anterior usa: rb.velocity
    }

    // Arranca la animación. La daga sale cuando llegue el Animation Event.
    void StartThrow()
    {
        if (animator != null)
        {
            throwEndTime = Time.time + throwLockTime;
            animator.SetTrigger(throwTrigger);
        }
        else
        {
            // Sin Animator: dispara de una vez, como antes
            LanzarDaga();
        }
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

    // Lo llama el Animation Event en el frame 23 (a través de MomiaAnimEvents)
    public void LanzarDaga()
    {
        if (daggerPrefab == null || player == null) return;

        Vector3 origin = GetFirePosition();
        Vector3 dir = (player.position - origin).normalized;

        Instantiate(daggerPrefab, origin, Quaternion.LookRotation(dir));
    }
}