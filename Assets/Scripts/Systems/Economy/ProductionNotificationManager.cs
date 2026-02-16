using System.Collections.Generic;
using UnityEngine;

public class ProductionNotificationManager : MonoBehaviour
{
    public static ProductionNotificationManager Instance;

    [Min(1f)] public float repeatDelaySeconds = 5f;

    readonly Dictionary<string, float> cooldowns = new();

    void Awake()
    {
        Instance = this;
    }

    public void NotifyIfReady(string key, string message)
    {
        float time = Time.time;
        if (cooldowns.TryGetValue(key, out float next) && time < next)
            return;

        cooldowns[key] = time + repeatDelaySeconds;
        AlertManager.Instance?.Push(message);
        Debug.Log($"[ProductionNotification] {message}");
    }
}
