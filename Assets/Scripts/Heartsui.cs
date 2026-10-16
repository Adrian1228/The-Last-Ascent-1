using UnityEngine;
using UnityEngine.UI;
 
public class HeartsUI : MonoBehaviour
{
    public PlayerHealth playerHealth;   // Arrastra al jugador
    public Image[] hearts;              // Arrastra aquí los 5 corazones (de izquierda a derecha)
    public Sprite fullHeart;            // Corazón rojo
    [Tooltip("Opcional: si lo dejas vacío, el corazón simplemente desaparece al perderlo")]
    public Sprite emptyHeart;
 
    void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHearts;
            UpdateHearts(playerHealth.CurrentHearts, playerHealth.maxHearts);
        }
    }
 
    void OnDisable()
    {
        if (playerHealth != null) playerHealth.OnHealthChanged -= UpdateHearts;
    }
 
    void UpdateHearts(int current, int max)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i >= max) { hearts[i].enabled = false; continue; }
 
            if (i < current)
            {
                hearts[i].enabled = true;
                if (fullHeart != null) hearts[i].sprite = fullHeart;
            }
            else if (emptyHeart != null)
            {
                hearts[i].enabled = true;
                hearts[i].sprite = emptyHeart;
            }
            else
            {
                hearts[i].enabled = false; // Desaparece
            }
        }
    }
}