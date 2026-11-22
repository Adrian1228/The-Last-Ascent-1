using System.Collections;
using UnityEngine;

public class MummyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public float destroyDelay = 2f;
    public string dieTrigger = "Die";

    int current;
    bool dead;
    Rigidbody rb;
    Animator animator;

    void Awake()
    {
        current = maxHealth;
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    public void TakeDamage(int amount, Vector3 from)
    {
        if (dead) return;
        current -= amount;
        if (current <= 0) Die();
    }

    void Die()
    {
        dead = true;

        // Apaga la IA (Mummymelee, MummyRanged...) y los golpes
        foreach (var mb in GetComponents<MonoBehaviour>())
            if (mb != this) mb.enabled = false;
        foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (animator != null && HasParam(dieTrigger)) animator.SetTrigger(dieTrigger);

        Destroy(gameObject, destroyDelay);
    }

    bool HasParam(string paramName)
    {
        foreach (var p in animator.parameters)
            if (p.name == paramName) return true;
        return false;
    }
}