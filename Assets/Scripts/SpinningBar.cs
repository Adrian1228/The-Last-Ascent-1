using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpinningBar : MonoBehaviour
{
    [Header("Giro")]
    [Tooltip("Grados por segundo. Negativo = gira al otro lado.")]
    public float rotationSpeed = 90f;
    [Tooltip("Punto alrededor del cual giran las barras (el poste). Si lo dejas vacío usa el centro de las barras.")]
    public Transform center;

    [Header("Empuje")]
    public float pushForce = 12f;
    public float pushUpForce = 4f;
    public float pushDuration = 0.25f;
    [Tooltip("Segundos antes de poder empujar de nuevo al jugador.")]
    public float pushCooldown = 0.6f;

    [Header("Daño (0 = solo empuja)")]
    public int damage = 0;

    Rigidbody rb;
    Vector3 centerPos;
    float nextPushTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        if (center != null)
        {
            centerPos = center.position;
        }
        else
        {
            // Centro real de las barras (no depende del pivote del modelo)
            var rends = GetComponentsInChildren<Renderer>();
            if (rends.Length > 0)
            {
                Bounds b = rends[0].bounds;
                foreach (var r in rends) b.Encapsulate(r.bounds);
                centerPos = b.center;
            }
            else
            {
                centerPos = transform.position;
            }
        }
    }

    void FixedUpdate()
    {
        // Gira alrededor del centro, sobre el eje vertical del mundo
        Quaternion q = Quaternion.AngleAxis(rotationSpeed * Time.fixedDeltaTime, Vector3.up);
        rb.MovePosition(centerPos + q * (rb.position - centerPos));
        rb.MoveRotation(q * rb.rotation);
    }

    void OnCollisionEnter(Collision collision) { TryPush(collision); }
    void OnCollisionStay(Collision collision) { TryPush(collision); }

    void TryPush(Collision collision)
    {
        if (Time.time < nextPushTime) return;

        var player = collision.collider.GetComponentInParent<PlayerController>();
        if (player == null) return;

        // Hacia afuera del poste y en el sentido en que barre la barra
        Vector3 radial = player.transform.position - centerPos;
        radial.y = 0f;
        if (radial.sqrMagnitude < 0.001f) radial = transform.forward;
        radial.Normalize();

        Vector3 tangent = Vector3.Cross(Vector3.up, radial) * Mathf.Sign(rotationSpeed);
        Vector3 dir = (radial * 0.5f + tangent);
        dir.y = 0f;

        player.ApplyKnockback(dir.normalized, pushForce, pushUpForce, pushDuration);
        nextPushTime = Time.time + pushCooldown;

        if (damage > 0)
        {
            var health = player.GetComponentInParent<PlayerHealth>();
            if (health != null) health.TakeDamage(damage, centerPos);
        }
    }
}