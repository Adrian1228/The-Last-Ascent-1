using UnityEngine;
 
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 5f;
    public float crouchSpeed = 2.5f;
    public float rotationSpeed = 12f;
 
    [Header("Salto")]
    public float jumpForce = 7f;
    public LayerMask groundMask = ~0;      // Capas que cuentan como suelo
    public float groundCheckDistance = 0.15f;
 
    [Header("Agacharse")]
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float crouchHeight = 1f;
    [Tooltip("Opcional: el modelo/visual del jugador para achicarlo al agacharse")]
    public Transform visual;
 
    Rigidbody rb;
    CapsuleCollider col;
    Camera cam;
 
    float standHeight;
    Vector3 standCenter;
    Vector3 visualStandScale;
 
    Vector3 moveInput;
    bool jumpRequested;
    bool isCrouching;
 
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        cam = Camera.main;
 
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
 
        standHeight = col.height;
        standCenter = col.center;
        if (visual != null) visualStandScale = visual.localScale;
    }
 
    void Update()
    {
        // Input (WASD)
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(h, 0f, v).normalized;
 
        if (Input.GetButtonDown("Jump") && IsGrounded() && !isCrouching)
            jumpRequested = true;
 
        // Agacharse (mantener)
        bool wantsCrouch = Input.GetKey(crouchKey);
        if (wantsCrouch && !isCrouching) SetCrouch(true);
        else if (!wantsCrouch && isCrouching && CanStandUp()) SetCrouch(false);
    }
 
    void FixedUpdate()
    {
        // Dirección relativa a la cámara (si no hay, relativa al mundo)
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;
        if (cam != null)
        {
            forward = cam.transform.forward; forward.y = 0f; forward.Normalize();
            right = cam.transform.right;    right.y = 0f;   right.Normalize();
        }
 
        Vector3 dir = forward * moveInput.z + right * moveInput.x;
        float speed = isCrouching ? crouchSpeed : walkSpeed;
 
        Vector3 vel = rb.linearVelocity; // En Unity 2022 o anterior usa: rb.velocity
        vel.x = dir.x * speed;
        vel.z = dir.z * speed;
 
        if (jumpRequested)
        {
            vel.y = jumpForce;
            jumpRequested = false;
        }
        rb.linearVelocity = vel; // En Unity 2022 o anterior usa: rb.velocity
 
        // Rotar hacia donde se mueve
        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(dir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, rotationSpeed * Time.fixedDeltaTime));
        }
    }
 
    bool IsGrounded()
    {
        // Esfera desde el centro hacia abajo
        float radius = col.radius * 0.9f;
        Vector3 origin = transform.TransformPoint(col.center);
        float dist = (col.height * 0.5f) - radius + groundCheckDistance;
        return Physics.SphereCast(origin, radius, Vector3.down, out _, dist, groundMask, QueryTriggerInteraction.Ignore);
    }
 
    bool CanStandUp()
    {
        // Revisa que no haya techo encima para pararse
        float radius = col.radius * 0.9f;
        Vector3 origin = transform.position + Vector3.up * (crouchHeight * 0.5f);
        float dist = standHeight - crouchHeight;
        return !Physics.SphereCast(origin, radius, Vector3.up, out _, dist, groundMask, QueryTriggerInteraction.Ignore);
    }
 
    void SetCrouch(bool crouch)
    {
        isCrouching = crouch;
 
        if (crouch)
        {
            col.height = crouchHeight;
            // Mantiene los pies en el mismo lugar
            col.center = new Vector3(standCenter.x, standCenter.y - (standHeight - crouchHeight) * 0.5f, standCenter.z);
            if (visual != null)
                visual.localScale = new Vector3(visualStandScale.x, visualStandScale.y * (crouchHeight / standHeight), visualStandScale.z);
        }
        else
        {
            col.height = standHeight;
            col.center = standCenter;
            if (visual != null) visual.localScale = visualStandScale;
        }
    }
}
