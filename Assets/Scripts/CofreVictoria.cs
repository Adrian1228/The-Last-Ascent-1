
using UnityEngine;

public class CofreVictoria : MonoBehaviour
{
    [Header("Pantalla de victoria")]
    public GameObject pantallaVictoria;

    private bool victoriaActivada = false;

    void Start()
    {
        // Ocultar la pantalla al comenzar el juego
        if (pantallaVictoria != null)
        {
            pantallaVictoria.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (victoriaActivada) return;

        // Comprobar que quien toca el cofre sea el jugador
        if (other.CompareTag("Player"))
        {
            victoriaActivada = true;

            // Mostrar la pantalla de victoria
            if (pantallaVictoria != null)
            {
                pantallaVictoria.SetActive(true);
            }

            Debug.Log("¡VICTORIA! El jugador encontró el tesoro.");
        }
    }
}
