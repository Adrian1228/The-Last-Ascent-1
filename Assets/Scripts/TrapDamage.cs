
using UnityEngine;

public class TrapDamage : MonoBehaviour
{
    public float tiempoEntreDanios = 1f;

    private float siguienteDanio;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (Time.time >= siguienteDanio)
        {
            other.SendMessage(
                "TakeDamage",
                1,
                SendMessageOptions.DontRequireReceiver
            );

            siguienteDanio = Time.time + tiempoEntreDanios;
        }
    }
}

