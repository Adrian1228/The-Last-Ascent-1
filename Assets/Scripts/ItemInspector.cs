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
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    [Header("Control")]
    public float rotateSpeed = 5f;      // sensibilidad del mouse
    public float autoSpin = 25f;        // giro lento cuando no arrastras
    public KeyCode closeKey = KeyCode.E;

    GameObject current;
    float prevTimeScale = 1f;
    CursorLockMode prevLock;
    bool prevCursorVisible;
    float openedAt;

    void Awake()
    {
        Instance = this;
        IsOpen = false;
        if (panel != null) panel.SetActive(false);
        if (inspectCamera != null) inspectCamera.enabled = false;
    }

    public void Open(GameObject modelPrefab, string title, string description, float scale = 1f)
    {
        if (IsOpen || modelPrefab == null) return;
        IsOpen = true;
        openedAt = Time.unscaledTime;

        // Copia del objeto en la mini escena
        current = Instantiate(modelPrefab, itemAnchor);
        current.transform.localPosition = Vector3.zero;
        current.transform.localRotation = Quaternion.identity;
        current.transform.localScale = Vector3.one * scale;

        // Sin colisiones ni scripts: es solo para mirar
        foreach (var c in current.GetComponentsInChildren<Collider>()) c.enabled = false;
        foreach (var m in current.GetComponentsInChildren<MonoBehaviour>()) m.enabled = false;
        foreach (var r in current.GetComponentsInChildren<Rigidbody>()) r.isKinematic = true;

        itemAnchor.localRotation = Quaternion.identity;

        titleText.text = title;
        descriptionText.text = description;
        panel.SetActive(true);
        inspectCamera.enabled = true;

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
        else
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
        inspectCamera.enabled = false;

        Time.timeScale = prevTimeScale;
        Cursor.lockState = prevLock;
        Cursor.visible = prevCursorVisible;
    }
}