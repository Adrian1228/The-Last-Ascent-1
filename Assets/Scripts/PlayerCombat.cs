using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public bool HasSword { get; private set; }

    // Lo llama el pickup al cerrar la pantalla de inspección
    public void GiveSword()
    {
        HasSword = true;
        Debug.Log("Espada obtenida");
        // Paso 2: aquí se equipa en la mano
    }
}