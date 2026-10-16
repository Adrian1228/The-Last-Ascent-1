using UnityEngine;
 
public class CameraOrbit : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target;                         // Arrastra aquí al jugador
    public Vector3 targetOffset = new Vector3(0f, 1.5f, 0f); // Punto al que mira (altura de la cabeza)
 
    [Header("Posición")]
    public float distance = 7f;                      // Qué tan lejos está del jugador
    public float startPitch = 30f;                   // Ángulo inicial (más alto = más desde arriba)
 
    [Header("Mouse")]
    public float sensitivityX = 3f;
    public float sensitivityY = 2f;
    public float minPitch = 15f;                     // Límite mirando desde abajo
    public float maxPitch = 60f;                     // Límite mirando desde arriba
    public bool invertY = false;
    public bool lockCursor = true;
 
    [Header("Colisión con paredes")]
    public LayerMask collisionMask = ~0;
    public float collisionRadius = 0.3f;
 
    float yaw;
    float pitch;
 
    void Start()
    {
        pitch = startPitch;
        if (target != null) yaw = target.eulerAngles.y;
 
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
 
    void Update()
    {
        // ESC suelta el mouse, clic lo vuelve a atrapar
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (lockCursor && Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
 
    void LateUpdate()
    {
        if (target == null) return;
 
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            yaw += Input.GetAxis("Mouse X") * sensitivityX;
            pitch += Input.GetAxis("Mouse Y") * sensitivityY * (invertY ? 1f : -1f);
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
 
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 pivot = target.position + targetOffset;
        Vector3 desiredPos = pivot - rotation * Vector3.forward * distance;
 
        // Si hay una pared entre el jugador y la cámara, la acerca
        Vector3 dir = desiredPos - pivot;
        if (Physics.SphereCast(pivot, collisionRadius, dir.normalized, out RaycastHit hit, dir.magnitude, collisionMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.GetComponentInParent<PlayerHealth>() == null)
                desiredPos = pivot + dir.normalized * Mathf.Max(0.5f, hit.distance);
        }
 
        transform.position = desiredPos;
        transform.rotation = rotation;
    }
}