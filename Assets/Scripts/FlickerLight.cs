using UnityEngine;

[RequireComponent(typeof(Light))]
public class FlickerLight : MonoBehaviour
{
    public float baseIntensity = 2f;   // Intensidad promedio
    public float flickerAmount = 0.6f; // Cuánto varía
    public float speed = 8f;           // Qué tan rápido parpadea

    Light lightComp;
    float seed;

    void Awake()
    {
        lightComp = GetComponent<Light>();
        seed = Random.value * 100f;    // Cada antorcha parpadea distinto
    }

    void Update()
    {
        float n = Mathf.PerlinNoise(seed, Time.time * speed);
        lightComp.intensity = baseIntensity + (n - 0.5f) * 2f * flickerAmount;
    }
}