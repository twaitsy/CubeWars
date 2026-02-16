using UnityEngine;

public class CarryingController : MonoBehaviour
{
    [SerializeField] private int capacity;

    public int Capacity => capacity;

    public void SetCapacity(int value)
    {
        capacity = value;
    }
}