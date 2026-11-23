
using UnityEngine;

public class SwordPickup : MonoBehaviour
{
    [Header("Flotar")]
    public float bobHeight = 0.2f;
    public float bobSpeed = 2f;

    [Header("Girar")]
    public float rotateSpeed = 60f;   // 0 = no gira

    [Header("Sonido")]
    public AudioClip pickupSound;
    [Range(0f, 1f)]
    public float pickupVolume = 1f;

    [Header("Inspección")]
    [Tooltip("Modelo que se muestra. Si lo dejas vacío usa este mismo objeto.")]
    public GameObject inspectModel;
    public string itemTitle = "Espada de bronce";
    [TextArea(2, 4)]
    public string itemDescription = "Una hoja antigua forjada para los guardianes de la pirámide.";
    public float inspectScale = 1f;
    [Tooltip("Rotación con la que se ve en la pantalla. Si sale acostada, prueba (0,0,90) o (90,0,0).")]
    public Vector3 inspectRotation = Vector3.zero;

    Vector3 localCenter;
    float lastOffset;
    bool taken;

    void Start()
    {
        // Centro visual del modelo
        var rends = GetComponentsInChildren<Renderer>();

        if (rends.Length > 0)
        {
            Bounds b = rends[0].bounds;

            foreach (var r in rends)
                b.Encapsulate(r.bounds);

            localCenter = transform.InverseTransformPoint(b.center);
        }
    }

    void Update()
    {
        // Flota
        float offset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position += Vector3.up * (offset - lastOffset);
        lastOffset = offset;

        // Gira sobre su propio centro
        Vector3 worldCenter = transform.TransformPoint(localCenter);
        transform.RotateAround(worldCenter, Vector3.up, rotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (taken) return;

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health == null) return;

        taken = true;

        // Sonido al recoger la espada
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(
                pickupSound,
                transform.position,
                pickupVolume
            );
        }

        // Busca (o crea) PlayerCombat en el Player
        PlayerCombat combat = health.GetComponent<PlayerCombat>();

        if (combat == null)
            combat = health.gameObject.AddComponent<PlayerCombat>();

        if (ItemInspector.Instance != null)
        {
            GameObject model = inspectModel != null ? inspectModel : gameObject;

            ItemInspector.Instance.Open(
                model,
                itemTitle,
                itemDescription,
                inspectScale,
                inspectRotation,
                () => combat.GiveSword()
            );
        }
        else
        {
            combat.GiveSword();
        }

        Destroy(gameObject);
    }
}
