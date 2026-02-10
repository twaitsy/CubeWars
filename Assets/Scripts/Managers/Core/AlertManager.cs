using UnityEngine;
using System.Collections.Generic;

public class AlertManager : MonoBehaviour
{
    public static AlertManager Instance;

    class Alert
    {
        public string text;
        public float timer;
    }

    List<Alert> alerts = new List<Alert>();
    readonly List<string> recentMessages = new List<string>();

    [Min(1)] public int maxRecent = 8;

    void Awake()
    {
        Instance = this;
    }

    void OnGUI()
    {
        float y = Screen.height - 200;

        for (int i = alerts.Count - 1; i >= 0; i--)
        {
            var a = alerts[i];
            a.timer -= Time.deltaTime;

            GUI.Label(new Rect(10, y, 400, 20), a.text);
            y -= 22;

            if (a.timer <= 0)
                alerts.RemoveAt(i);
        }
    }

    public void Push(string text, float duration = 3f)
    {
        alerts.Add(new Alert { text = text, timer = duration });

        recentMessages.Add(text);
        while (recentMessages.Count > maxRecent)
            recentMessages.RemoveAt(0);
    }

    public List<string> GetRecent(int maxCount)
    {
        if (maxCount <= 0 || recentMessages.Count == 0)
            return new List<string>();

        int count = Mathf.Min(maxCount, recentMessages.Count);
        return recentMessages.GetRange(recentMessages.Count - count, count);
    }
}
