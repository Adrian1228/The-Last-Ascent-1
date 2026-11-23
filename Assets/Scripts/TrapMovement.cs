using UnityEngine;

public class TrapMovement : MonoBehaviour
{
    public Transform soporteConPuas;

    public float distanciaBarra = 0.5f;
    public float velocidadBarra = 0.5f;

    public float limitePuas = 1.0f;
    public float velocidadPuas = 0.8f;

    private Vector3 posicionInicial;
    private Vector3 posicionPuasInicial;

    void Start()
    {
        posicionInicial = transform.localPosition;
        posicionPuasInicial = soporteConPuas.localPosition;
    }

    void Update()
    {
        // Barra principal: adelante y atras (eje X)
        Vector3 nuevaPosicion = posicionInicial;
        nuevaPosicion.x += Mathf.Sin(Time.time * velocidadBarra)
                           * distanciaBarra;

        transform.localPosition = nuevaPosicion;

        // Soporte y puas: izquierda y derecha (eje Z)
        Vector3 nuevaPosicionPuas = posicionPuasInicial;
        nuevaPosicionPuas.z += Mathf.Sin(Time.time * velocidadPuas)
                               * limitePuas;

        soporteConPuas.localPosition = nuevaPosicionPuas;
    }
}

