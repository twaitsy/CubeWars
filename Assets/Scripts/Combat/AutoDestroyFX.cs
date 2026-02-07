using UnityEngine;

public class AutoDestroyFX : MonoBehaviour
{
    public float lifetime = 1.5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
