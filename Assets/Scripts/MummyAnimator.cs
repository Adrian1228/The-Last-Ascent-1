using UnityEngine;

// Va en la momia (el objeto con Rigidbody). Le avisa al Animator si está quieta o caminando.
[RequireComponent(typeof(Rigidbody))]
public class MummyAnimator : MonoBehaviour
{
    [Tooltip("Si lo dejas vacío, lo busca solo en el modelo hijo")]
    public Animator animator;
    public float dampTime = 0.1f;

    static readonly int SpeedHash = Animator.StringToHash("Speed"); // Parámetro Float del Animator

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (animator == null) return;

        // Velocidad horizontal real de la momia (0 = quieta, moveSpeed = caminando)
        Vector3 v = rb.linearVelocity; // En Unity 2022 o anterior usa: rb.velocity
        v.y = 0f;

        animator.SetFloat(SpeedHash, v.magnitude, dampTime, Time.deltaTime);
    }
}