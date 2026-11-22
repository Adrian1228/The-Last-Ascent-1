using System.Collections;
using UnityEngine;

public class AlignModelToCapsule : MonoBehaviour
{
    [Tooltip("Un poco más arriba (+) o más abajo (-) si quieres afinar. En metros.")]
    public float extraOffset = 0f;
    public bool debugLog = true;

    IEnumerator Start()
    {
        // Espera un frame para que el Animator ponga la pose inicial
        yield return null;

        var capsule = GetComponent<CapsuleCollider>();
        var animator = GetComponentInChildren<Animator>();
        if (capsule == null || animator == null)
        {
            Debug.LogWarning("AlignModelToCapsule: falta el CapsuleCollider o el Animator del modelo.");
            yield break;
        }

        // Punto más bajo de la cápsula, en el mundo
        float scaleY = Mathf.Abs(transform.lossyScale.y);
        float capsuleBottom = transform.position.y + (capsule.center.y - capsule.height * 0.5f) * scaleY;

        // Punto más bajo del modelo (suelas)
        float modelBottom = float.MaxValue;
        foreach (var r in animator.GetComponentsInChildren<Renderer>())
            modelBottom = Mathf.Min(modelBottom, r.bounds.min.y);

        if (modelBottom == float.MaxValue)
        {
            Debug.LogWarning("AlignModelToCapsule: el modelo no tiene ningún Renderer.");
            yield break;
        }

        float delta = capsuleBottom - modelBottom + extraOffset;
        animator.transform.position += Vector3.up * delta;

        if (debugLog)
            Debug.Log($"AlignModelToCapsule: modelo movido {delta:F3} m (pies ahora en la base de la cápsula)");
    }
}