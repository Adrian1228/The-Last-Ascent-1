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
    public LayerMask groundMask = ~0;
    public float groundCheckDistance = 0.15f;

    [Header("Agacharse")]
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float crouchHeight = 1f;
    [Tooltip("Opcional: el modelo/visual del jugador para achicarlo al agacharse")]
    public Transform visual;

    [Header("Animación")]
    [Tooltip("Si lo dejas vacío, lo busca solo en el personaje hijo")]
    public Animator animator;

    // Nombres de los parámetros del Animator (deben ser EXACTOS)
    static readonly int SpeedHash = Animator.StringToHash("Speed");          // Float
    static readonly int IsJumpingHash = Animator.StringToHash("IsJumping");  // Bool
    static readonly int HitHash = Animator.StringToHash("Hit");              // Trigger

    Rigidbody rb;
    CapsuleCollider col;
    Camera cam;

    float standHeight;
    Vector3 standCenter;
    Vector3 visualStandScale;

    Vector3 moveInput;
    bool jumpRequested;
    bool isCrouching;
    float knockbackTimer;

    bool isJumping;
    float jumpStartTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        cam = Camera.main;

        if (animator == null) animator = GetComponentInChildren<Animator>();

        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        standHeight = col.height;
        standCenter = col.center;
        if (visual != null) visualStandScale = visual.localScale;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return; // En pausa no leer teclas

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector3(h, 0f, v).normalized;

        // Velocidad para el Animator (0 = quieto, 1 = caminando)
        if (animator != null)
            animator.SetFloat(SpeedHash, moveInput.magnitude, 0.1f, Time.deltaTime);

        if (Input.GetButtonDown("Jump") && IsGrounded() && !isCrouching && knockbackTimer <= 0f)
            jumpRequested = true;

        // Terminar la animación de salto al aterrizar
        if (isJumping && Time.time - jumpStartTime > 0.2f && IsGrounded())
        {
            isJumping = false;
            if (animator != null) animator.SetBool(IsJumpingHash, false);
        }

        bool wantsCrouch = Input.GetKey(crouchKey);
        if (wantsCrouch && !isCrouching) SetCrouch(true);
        else if (!wantsCrouch && isCrouching && CanStandUp()) SetCrouch(false);
    }

    void FixedUpdate()
    {
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

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

            // Arranca la animación de salto
            isJumping = true;
            jumpStartTime = Time.time;
            if (animator != null) animator.SetBool(IsJumpingHash, true);
        }
        rb.linearVelocity = vel; // En Unity 2022 o anterior usa: rb.velocity

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(dir);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, rotationSpeed * Time.fixedDeltaTime));
        }
    }

    // Lo llama PlayerHealth cuando te pegan (sirve igual para las dos momias)
    public void PlayHit()
    {
        if (animator != null) animator.SetTrigger(HitHash);
    }

    // Lo llama PlayerHealth cuando te pegan
    public void ApplyKnockback(Vector3 direction, float force, float upForce, float duration)
    {
        knockbackTimer = duration;
        jumpRequested = false;

        Vector3 vel = direction * force;
        vel.y = upForce;
        rb.linearVelocity = vel; // En Unity 2022 o anterior usa: rb.velocity
    }

    bool IsGrounded()
    {
        float radius = col.radius * 0.9f;
        Vector3 origin = transform.TransformPoint(col.center);
        float dist = (col.height * 0.5f) - radius + groundCheckDistance;
        return Physics.SphereCast(origin, radius, Vector3.down, out _, dist, groundMask, QueryTriggerInteraction.Ignore);
    }

    bool CanStandUp()
    {
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