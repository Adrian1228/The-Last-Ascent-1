using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public enum SheathPlace { LeftHip, Back }

    [Header("Espada")]
    [Tooltip("Prefab de la espada desde el Project (no el de la Hierarchy). Se le quitan solos el pickup y los colliders.")]
    public GameObject swordPrefab;
    [Tooltip("Desmárcalo para que al recogerla quede guardada en la cadera. Con R la sacas.")]
    public bool startDrawn = false;
    [Tooltip("Largo en METROS (lado más largo del modelo). 0.4 a 0.6 = espada corta. 0 = tamaño original.")]
    public float swordLength = 0.5f;
    [Tooltip("Actívalo si la espada queda al revés (la punta donde va el mango).")]
    public bool flipGrip = false;

    [Header("En la mano derecha (ajuste fino, se ve en vivo con el juego corriendo)")]
    [Tooltip("Metros, en los ejes del personaje: X derecha, Y arriba, Z adelante")]
    public Vector3 handPosition = Vector3.zero;
    [Tooltip("Grados, en los ejes de la espada")]
    public Vector3 handRotation = Vector3.zero;
    [Tooltip("1 = el largo que pusiste arriba")]
    public float handScale = 1f;

    [Header("Guardada (ajuste fino)")]
    public SheathPlace sheathPlace = SheathPlace.LeftHip;
    public Vector3 sheathPosition = Vector3.zero;
    public Vector3 sheathRotation = Vector3.zero;
    public float sheathScale = 1f;

    [Header("Controles")]
    public KeyCode toggleKey = KeyCode.R;   // guardar / sacar. Clic izq = ataque, clic der = defensa

    [Header("Animator (nombres de parámetros)")]
    public string drawTrigger = "Draw";
    public string sheathTrigger = "Sheath";
    public string attackTrigger = "Attack";
    public string blockBool = "Block";

    [Header("Tiempos en segundos")]
    [Tooltip("Segundo del clip en que la mano toma la espada")]
    public float drawAttachDelay = 0.4f;
    [Tooltip("Length del clip de sacar")]
    public float drawDuration = 1.0f;
    [Tooltip("Segundo del clip en que la espada queda guardada")]
    public float sheathAttachDelay = 0.8f;
    [Tooltip("Length del clip de guardar")]
    public float sheathDuration = 1.2f;
    [Tooltip("Segundo del clip en que el golpe conecta")]
    public float attackHitDelay = 0.35f;
    [Tooltip("Length del clip de ataque")]
    public float attackDuration = 0.8f;

    [Header("Ataque")]
    public int attackDamage = 1;
    public float attackRange = 1.4f;        // qué tan adelante está la zona de golpe
    public float attackRadius = 1.0f;
    public float attackOffsetY = 0f;
    public LayerMask hitMask = ~0;

    [Header("Debug")]
    public bool debugLogs = true;

    public bool HasSword { get; private set; }
    public bool IsDrawn { get; private set; }
    public bool IsBlocking { get; private set; }

    Animator animator;
    Transform root;                 // raíz del modelo (hacia donde mira el personaje)
    Transform handBone, sheathBone;
    Transform holder, model;

    // Medidas de la espada
    int bladeAxis;
    Vector3 swordCenter;
    float swordLen;
    Vector3 gripLocal;              // posición del mango dentro del holder
    Vector3 bladeDir;               // dirección mango -> punta, dentro del holder

    // Posición y rotación base (calculadas automáticamente)
    Vector3 handBasePos, sheathBasePos;
    Quaternion handBaseRot = Quaternion.identity, sheathBaseRot = Quaternion.identity;
    Quaternion handRootToBone = Quaternion.identity, sheathRootToBone = Quaternion.identity;

    SheathPlace lastPlace;
    bool lastFlip;
    bool swordInHand;
    bool busy;

    // ---------- Equipar ----------

    // Lo llama el pickup al cerrar la pantalla de inspección
    public void GiveSword()
    {
        if (HasSword) return;
        HasSword = true;

        animator = GetComponentInChildren<Animator>();
        if (animator == null || !animator.isHuman)
        {
            Debug.LogWarning("PlayerCombat: el modelo debe ser Humanoid (FBX del jugador > Rig > Animation Type: Humanoid > Apply).");
            return;
        }
        root = animator.transform;
        handBone = animator.GetBoneTransform(HumanBodyBones.RightHand);

        if (swordPrefab == null)
        {
            Debug.LogWarning("PlayerCombat: falta asignar Sword Prefab (arrastra el prefab desde el Project, no el de la Hierarchy).");
            return;
        }

        // Contenedor en el origen, sin rotación ni escala, para medir la espada limpia
        holder = new GameObject("Espada_Equipada").transform;
        holder.position = Vector3.zero;
        holder.rotation = Quaternion.identity;
        holder.localScale = Vector3.one;

        model = Instantiate(swordPrefab, holder, false).transform;
        model.name = "Modelo";
        model.localPosition = Vector3.zero;
        foreach (var p in model.GetComponentsInChildren<SwordPickup>()) { p.enabled = false; Destroy(p); }
        foreach (var c in model.GetComponentsInChildren<Collider>()) Destroy(c);
        foreach (var r in model.GetComponentsInChildren<Rigidbody>()) Destroy(r);

        if (!TryGetBounds(model.gameObject, out Bounds b))
        {
            Debug.LogWarning("PlayerCombat: el prefab de la espada no tiene ningún Renderer (no hay nada que ver).");
            Destroy(holder.gameObject);
            holder = null;
            return;
        }

        // Eje largo = la hoja. Se escala para que mida 'swordLength' metros
        Vector3 size = b.size;
        bladeAxis = (size.x >= size.y && size.x >= size.z) ? 0 : (size.y >= size.z ? 1 : 2);
        float original = size[bladeAxis];
        if (swordLength > 0f && original > 0.0001f)
            model.localScale *= swordLength / original;

        TryGetBounds(model.gameObject, out b);
        swordCenter = b.center;
        swordLen = b.size[bladeAxis];

        RecalculateGrip();
        RecalculateBase();

        IsDrawn = startDrawn;
        swordInHand = IsDrawn;
        ApplyOffsets();

        if (debugLogs)
            Debug.Log($"PlayerCombat: espada equipada. Tamaño original {original:F2} m, ahora {swordLen:F2} m. " +
                      $"Mano={(handBone != null ? handBone.name : "NO ENCONTRADA")}, " +
                      $"Guardada={(sheathBone != null ? sheathBone.name : "NO ENCONTRADA")}, en mano={IsDrawn}");
    }

    bool TryGetBounds(GameObject go, out Bounds bounds)
    {
        var rends = go.GetComponentsInChildren<Renderer>();
        bounds = new Bounds(go.transform.position, Vector3.zero);
        if (rends.Length == 0) return false;
        bounds = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) bounds.Encapsulate(rends[i].bounds);
        return true;
    }

    // Dónde está el mango y hacia dónde apunta la hoja, dentro del contenedor
    void RecalculateGrip()
    {
        Vector3 axisVec = Vector3.zero;
        axisVec[bladeAxis] = 1f;
        float half = swordLen * 0.5f;

        if (!flipGrip) { gripLocal = swordCenter - axisVec * half; bladeDir = axisVec; }
        else           { gripLocal = swordCenter + axisVec * half; bladeDir = -axisVec; }

        lastFlip = flipGrip;
    }

    // Calcula automáticamente cómo debe quedar la espada en la mano y guardada
    [ContextMenu("Recalcular posición de la espada")]
    public void RecalculateBase()
    {
        if (animator == null || !animator.isHuman || holder == null) return;
        root = animator.transform;

        // --- Mano: el mango en la palma, la hoja hacia adelante ---
        Vector3 palm = handBone.position;
        Transform mid = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
        if (mid != null) palm = Vector3.Lerp(handBone.position, mid.position, 0.6f);

        handBasePos = handBone.InverseTransformPoint(palm);
        Vector3 handDir = (root.forward * 0.9f + Vector3.up * 0.25f).normalized;
        handBaseRot = Quaternion.Inverse(handBone.rotation) * Quaternion.FromToRotation(bladeDir, handDir);
        handRootToBone = Quaternion.Inverse(handBone.rotation) * root.rotation;

        // --- Guardada: colgando de la cadera izquierda o cruzada en la espalda ---
        Vector3 anchor, dir;
        if (sheathPlace == SheathPlace.LeftHip)
        {
            sheathBone = animator.GetBoneTransform(HumanBodyBones.Hips);
            anchor = sheathBone.position - root.right * 0.20f + Vector3.up * 0.10f - root.forward * 0.05f;
            dir = (Vector3.down - root.forward * 0.15f).normalized;
        }
        else
        {
            sheathBone = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            if (sheathBone == null) sheathBone = animator.GetBoneTransform(HumanBodyBones.Chest);
            if (sheathBone == null) sheathBone = animator.GetBoneTransform(HumanBodyBones.Spine);
            anchor = sheathBone.position - root.forward * 0.14f + root.right * 0.12f + Vector3.up * 0.15f;
            dir = (Vector3.down - root.right * 0.5f).normalized;
        }

        if (sheathBone != null)
        {
            sheathBasePos = sheathBone.InverseTransformPoint(anchor);
            sheathBaseRot = Quaternion.Inverse(sheathBone.rotation) * Quaternion.FromToRotation(bladeDir, dir);
            sheathRootToBone = Quaternion.Inverse(sheathBone.rotation) * root.rotation;
        }

        lastPlace = sheathPlace;
    }

    void AttachSword(bool inHand)
    {
        if (holder == null) return;
        swordInHand = inHand;
        ApplyOffsets();
    }

    // Se aplica cada frame, así los cambios del Inspector se ven en vivo
    void ApplyOffsets()
    {
        if (holder == null || model == null) return;

        Transform bone = swordInHand ? handBone : sheathBone;
        if (bone == null) return;
        if (holder.parent != bone) holder.SetParent(bone, false);

        Vector3 ls = bone.lossyScale;
        if (Mathf.Abs(ls.x) < 0.0001f || Mathf.Abs(ls.y) < 0.0001f || Mathf.Abs(ls.z) < 0.0001f) return;

        Vector3 basePos = swordInHand ? handBasePos : sheathBasePos;
        Quaternion baseRot = swordInHand ? handBaseRot : sheathBaseRot;
        Quaternion rootToBone = swordInHand ? handRootToBone : sheathRootToBone;
        Vector3 userPos = swordInHand ? handPosition : sheathPosition;
        Vector3 userRot = swordInHand ? handRotation : sheathRotation;
        float f = swordInHand ? handScale : sheathScale;

        // El mango queda justo en el punto de anclaje
        model.localPosition = -gripLocal;

        Vector3 off = rootToBone * userPos;
        off = new Vector3(off.x / ls.x, off.y / ls.y, off.z / ls.z);

        holder.localPosition = basePos + off;
        holder.localRotation = baseRot * Quaternion.Euler(userRot);
        holder.localScale = new Vector3(f / ls.x, f / ls.y, f / ls.z);
    }

    void LateUpdate()
    {
        if (holder == null) return;

        // Si cambias estas opciones con el juego corriendo, se recalcula solo
        if (flipGrip != lastFlip) { RecalculateGrip(); RecalculateBase(); }
        else if (sheathPlace != lastPlace) RecalculateBase();

        ApplyOffsets();
    }

    // ---------- Control ----------

    void Update()
    {
        if (debugLogs && Input.GetMouseButtonDown(0))
            Debug.Log($"Click -> HasSword={HasSword}, sword={(holder != null)}, animator={(animator != null)}, IsDrawn={IsDrawn}, busy={busy}, blocking={IsBlocking}, timeScale={Time.timeScale}, inspector={ItemInspector.IsOpen}");

        if (!HasSword || holder == null || animator == null) return;

        // Sin control en la pantalla de inspección ni en pausa
        if (ItemInspector.IsOpen || Time.timeScale == 0f)
        {
            SetBlocking(false);
            return;
        }

        if (Input.GetKeyDown(toggleKey) && !busy)
            StartCoroutine(IsDrawn ? SheathRoutine() : DrawRoutine());

        if (!IsDrawn || busy)
        {
            SetBlocking(false);
            return;
        }

        if (Input.GetMouseButtonDown(0) && !IsBlocking)
            StartCoroutine(AttackRoutine());
        else
            SetBlocking(Input.GetMouseButton(1));
    }

    // ---------- Animator seguro (avisa si un parámetro no existe) ----------

    bool HasParam(string paramName)
    {
        foreach (var p in animator.parameters)
            if (p.name == paramName) return true;
        Debug.LogWarning($"PlayerCombat: el Animator no tiene el parámetro '{paramName}'. Revisa el nombre (debe ser exacto).");
        return false;
    }

    void SafeTrigger(string paramName)
    {
        if (HasParam(paramName)) animator.SetTrigger(paramName);
    }

    void SetBlocking(bool value)
    {
        if (IsBlocking == value) return;
        IsBlocking = value;
        if (HasParam(blockBool)) animator.SetBool(blockBool, value);
    }

    // ---------- Rutinas ----------

    IEnumerator DrawRoutine()
    {
        busy = true;
        SafeTrigger(drawTrigger);
        yield return new WaitForSeconds(drawAttachDelay);
        AttachSword(true);
        yield return new WaitForSeconds(Mathf.Max(0f, drawDuration - drawAttachDelay));
        IsDrawn = true;
        busy = false;
    }

    IEnumerator SheathRoutine()
    {
        busy = true;
        SetBlocking(false);
        SafeTrigger(sheathTrigger);
        yield return new WaitForSeconds(sheathAttachDelay);
        AttachSword(false);
        yield return new WaitForSeconds(Mathf.Max(0f, sheathDuration - sheathAttachDelay));
        IsDrawn = false;
        busy = false;
    }

    IEnumerator AttackRoutine()
    {
        busy = true;
        SafeTrigger(attackTrigger);
        yield return new WaitForSeconds(attackHitDelay);
        DoHit();
        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - attackHitDelay));
        busy = false;
    }

    // ---------- Golpe ----------

    Transform Facing()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        return animator != null ? animator.transform : transform;
    }

    Vector3 HitCenter()
    {
        return transform.position + Vector3.up * attackOffsetY + Facing().forward * attackRange;
    }

    void DoHit()
    {
        var hits = Physics.OverlapSphere(HitCenter(), attackRadius, hitMask, QueryTriggerInteraction.Ignore);
        var done = new HashSet<MummyHealth>();
        foreach (var h in hits)
        {
            var mummy = h.GetComponentInParent<MummyHealth>();
            if (mummy != null && done.Add(mummy))
            {
                mummy.TakeDamage(attackDamage, transform.position);
                if (debugLogs) Debug.Log($"PlayerCombat: golpeó a {mummy.name}");
            }
        }
    }

    // Muestra la zona de golpe en la Scene al seleccionar el Player
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(HitCenter(), attackRadius);
    }
}