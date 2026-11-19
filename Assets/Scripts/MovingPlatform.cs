using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Recorrido")]
    [Tooltip("Dirección del riel en ejes LOCALES de la plataforma. Prueba (1,0,0), (0,0,1)... mira la línea en la Scene.")]
    public Vector3 localDirection = Vector3.right;
    [Tooltip("Largo del recorrido en metros. Negativo = va hacia el otro lado.")]
    public float distance = 8f;
    public float speed = 2f;
    [Tooltip("Segundos que se queda quieta en cada extremo (para que el jugador se suba).")]
    public float waitTime = 1.5f;

    Rigidbody rb;
    Vector3 pointA, pointB, target;
    float waitTimer;
    readonly List<Rigidbody> riders = new List<Rigidbody>();

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        Vector3 dir = transform.TransformDirection(localDirection.normalized);
        pointA = transform.position;
        pointB = pointA + dir * distance;
        target = pointB;
    }

    void FixedUpdate()
    {
        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector3 newPos = Vector3.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        Vector3 delta = newPos - rb.position;
        rb.MovePosition(newPos);

        // Lleva consigo a quien esté encima
        foreach (var r in riders)
            if (r != null) r.position += delta;

        // Llegó al extremo: espera y regresa
        if ((newPos - target).sqrMagnitude < 0.0001f)
        {
            target = (target == pointB) ? pointA : pointB;
            waitTimer = waitTime;
        }
    }

    // Lo llama la zona trigger (ZonaSubida) de encima de la plataforma
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerHealth>() == null) return;
        var body = other.attachedRigidbody;
        if (body != null && !riders.Contains(body)) riders.Add(body);
    }

    void OnTriggerExit(Collider other)
    {
        var body = other.attachedRigidbody;
        if (body != null) riders.Remove(body);
    }

#if UNITY_EDITOR
    // Dibuja el recorrido en la Scene al seleccionarla (visible a través de las paredes)
    void OnDrawGizmosSelected()
    {
        // Centro visual de la plataforma (el pivote puede estar en otro lado)
        Vector3 center = transform.position;
        var rends = GetComponentsInChildren<Renderer>();
        if (rends.Length > 0)
        {
            Bounds b = rends[0].bounds;
            foreach (var r in rends) b.Encapsulate(r.bounds);
            center = b.center;
        }
        Vector3 offset = center - transform.position;

        Vector3 dir = transform.TransformDirection(localDirection.normalized);
        Vector3 a = (Application.isPlaying ? pointA : transform.position) + offset;
        Vector3 end = (Application.isPlaying ? pointB : transform.position + dir * distance) + offset;

        UnityEditor.Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawLine(a, end, 4f);
        UnityEditor.Handles.DrawWireDisc(a, Vector3.up, 0.4f);
        UnityEditor.Handles.DrawWireDisc(end, Vector3.up, 0.4f);
    }
#endif
}