using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target;
    public Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Posición")]
    public float distance = 7f;
    public float startPitch = 30f;

    [Header("Mouse")]
    public float sensitivityX = 3f;
    public float sensitivityY = 2f;
    public float minPitch = 15f;
    public float maxPitch = 60f;
    public bool invertY = false;

    [Header("Colisión con paredes")]
    public LayerMask collisionMask = ~0;
    public float collisionRadius = 0.3f;

    float yaw;
    float pitch;

    // Temblor
    float shakeTimer;
    float shakeDuration;
    float shakeMagnitude;

    void Start()
    {
        pitch = startPitch;
        if (target != null) yaw = target.eulerAngles.y;
    }

    // Lo llama PlayerHealth al recibir daño
    public void Shake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeTimer = duration;
        shakeMagnitude = magnitude;
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (Time.timeScale > 0f && Cursor.lockState == CursorLockMode.Locked)
        {
            yaw += Input.GetAxis("Mouse X") * sensitivityX;
            pitch += Input.GetAxis("Mouse Y") * sensitivityY * (invertY ? 1f : -1f);
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 pivot = target.position + targetOffset;
        Vector3 desiredPos = pivot - rotation * Vector3.forward * distance;

        Vector3 dir = desiredPos - pivot;
        if (Physics.SphereCast(pivot, collisionRadius, dir.normalized, out RaycastHit hit, dir.magnitude, collisionMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.GetComponentInParent<PlayerHealth>() == null)
                desiredPos = pivot + dir.normalized * Mathf.Max(0.5f, hit.distance);
        }

        // Aplica el temblor (se va apagando)
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            float strength = shakeMagnitude * (shakeTimer / shakeDuration);
            desiredPos += Random.insideUnitSphere * strength;
        }

        transform.position = desiredPos;
        transform.rotation = rotation;
    }
}