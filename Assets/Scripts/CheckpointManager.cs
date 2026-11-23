using System.Collections.Generic;
using UnityEngine;

// Guarda el progreso entre muertes. Es "static", así que sobrevive a la recarga de la escena.
public static class CheckpointManager
{
    static readonly HashSet<Vector3Int> activated = new HashSet<Vector3Int>();

    public static bool HasCheckpoint { get; private set; }
    public static Vector3 RespawnPosition { get; private set; }

    // Altura de partida original (para que el contador de altura no se reinicie)
    public static bool HasStartY { get; private set; }
    public static float StartY { get; private set; }

    // Tiempo acumulado (para que no se reinicie al morir después de un checkpoint)
    public static float SavedTime;

    public static bool IsActivated(Vector3Int id)
    {
        return activated.Contains(id);
    }

    public static void Activate(Vector3Int id, Vector3 respawnPosition)
    {
        activated.Add(id);
        RespawnPosition = respawnPosition;
        HasCheckpoint = true;
    }

    public static void RegisterStart(float y)
    {
        if (HasStartY) return;
        HasStartY = true;
        StartY = y;
    }

    // Partida nueva: borra todo
    public static void Reset()
    {
        activated.Clear();
        HasCheckpoint = false;
        RespawnPosition = Vector3.zero;
        HasStartY = false;
        StartY = 0f;
        SavedTime = 0f;
    }

    // Al darle Play en el editor siempre se empieza limpio
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetOnPlay()
    {
        Reset();
    }
}