using UnityEngine;

public class MomiaAnimEvents : MonoBehaviour
{
    MummyRanged ranged;

    void Awake()
    {
        ranged = GetComponentInParent<MummyRanged>();
    }

    // Lo llama el Animation Event
    public void LanzarDaga()
    {
        if (ranged != null) ranged.LanzarDaga();
    }
}