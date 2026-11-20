using System;
using UnityEngine;
using TMPro;

public class ItemInspector : MonoBehaviour
{
    public static ItemInspector Instance { get; private set; }
    public static bool IsOpen { get; private set; }

    [Header("Escena de inspección")]
    public Camera inspectCamera;
    public Transform itemAnchor;

    [Header("UI")]
    public GameObject panel;
    [Tooltip("Canvas con el velo oscuro (Screen Space - Camera). Se ve detrás del objeto.")]
    public GameObject dimCanvas;
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    [Header("Control")]
    public float rotateSpeed = 5f;      // sensibilidad del mouse
    public float autoSpin = 0f;         // 0 = solo gira cuando tú lo giras
    public KeyCode closeKey = KeyCode.E;

    [Header("Sonido")]
    public AudioClip openSound;
    [Range(0f, 1f)] public float openVolume = 1f;

    GameObject current;
    AudioSource audioSource;
    Action onCloseCallback;
    float prevTimeScale = 1f;
    CursorLockMode prevLock;
    bool prevCursorVisible;
    float openedAt;

    void Awake()
    {
        Instance = this;
        IsOpen = false;
        if (panel != null) panel.SetActive(false);
        if (dimCanvas != null) dimCanvas.SetActive(false);
        if (inspectCamera != null) inspectCamera.enabled = false;

        // AudioSource 2D propio: suena igual aunque el juego esté pausado
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    // eulerRotation: cómo se ve el objeto al abrirse (útil para espadas, que salen acostadas)
    // onClose: se ejecuta cuando el jugador cierra la pantalla
    public void Open(GameObject modelPrefab, string title, string description,
                     float scale = 1f, Vector3 eulerRotation = default, Action onClose = null)
    {
        if (IsOpen || modelPrefab == null) return;
        IsOpen = true;
        openedAt = Time.unscaledTime;
        onCloseCallback = onClose;

        // Copia del objeto en la mini escena
        current = Instantiate(modelPrefab, itemAnchor);
        current.transform.localPosition = Vector3.zero;
        current.transform.localRotation = Quaternion.Euler(eulerRotation);
        current.transform.localScale = Vector3.one * scale;
                // Centra el modelo en el ancla aunque su pivote esté lejos
        var rends = current.GetComponentsInChildren<Renderer>();
        if (rends.Length > 0)
        {
            Bounds b = rends[0].bounds;
            foreach (var r in rends) b.Encapsulate(r.bounds);
            current.transform.position += itemAnchor.position - b.center;
        }

        // Sin colisiones ni scripts: es solo para mirar
        foreach (var c in current.GetComponentsInChildren<Collider>()) c.enabled = false;
        foreach (var m in current.GetComponentsInChildren<MonoBehaviour>()) m.enabled = false;
        foreach (var r in current.GetComponentsInChildren<Rigidbody>()) r.isKinematic = true;

        itemAnchor.localRotation = Quaternion.identity;

        titleText.text = title;
        descriptionText.text = description;
        if (dimCanvas != null) dimCanvas.SetActive(true);
        panel.SetActive(true);
        inspectCamera.enabled = true;

        // Sonido al abrir la interfaz
        if (openSound != null) audioSource.PlayOneShot(openSound, openVolume);

        // Pausa y cursor libre
        prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        prevLock = Cursor.lockState;
        prevCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (!IsOpen) return;

        if (Input.GetMouseButton(0))
        {
            float x = Input.GetAxis("Mouse X") * rotateSpeed;
            float y = Input.GetAxis("Mouse Y") * rotateSpeed;
            itemAnchor.Rotate(Vector3.up, -x, Space.World);
            itemAnchor.Rotate(Vector3.right, y, Space.World);
        }
        else if (autoSpin != 0f)
        {
            itemAnchor.Rotate(Vector3.up, autoSpin * Time.unscaledDeltaTime, Space.World);
        }

        // Pequeña espera para no cerrar con la misma pulsación que lo abrió
        if (Time.unscaledTime - openedAt > 0.3f && Input.GetKeyDown(closeKey))
            Close();
    }

    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;

        if (current != null) Destroy(current);
        panel.SetActive(false);
        if (dimCanvas != null) dimCanvas.SetActive(false);
        inspectCamera.enabled = false;

        Time.timeScale = prevTimeScale;
        Cursor.lockState = prevLock;
        Cursor.visible = prevCursorVisible;

        // Avisa que se cerró (por ejemplo, para equipar la espada)
        var cb = onCloseCallback;
        onCloseCallback = null;
        cb?.Invoke();
    }
}