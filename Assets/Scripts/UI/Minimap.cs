using UnityEngine;
using System.Collections.Generic;

public class Minimap : MonoBehaviour
{
    [Header("Map")]
    public Vector2 worldMin = new Vector2(-100, -100);
    public Vector2 worldMax = new Vector2(100, 100);

    [Header("UI")]
    public Rect minimapRect = new Rect(10, 10, 200, 200);

    [Header("Icons")]
    public float dotSize = 4f;

    Camera mainCam;

    void Awake()
    {
        mainCam = Camera.main;
    }

    void OnGUI()
    {
        GUI.Box(minimapRect, "Minimap");

        DrawEntities<Unit>();
        DrawEntities<Turret>();
        DrawEntities<ResourceNode>();

        HandleClick();
    }

    void DrawEntities<T>() where T : MonoBehaviour
    {
        T[] all = GameObject.FindObjectsOfType<T>();

        foreach (var e in all)
        {
            Vector3 pos = e.transform.position;
            Vector2 p = WorldToMinimap(pos);

            Color c = Color.white;

            if (e.TryGetComponent<Unit>(out var u))
                c = TeamColorUtils.Get(u.teamID);
            else if (e.TryGetComponent<Turret>(out var t))
                c = TeamColorUtils.Get(t.teamID);
            else if (e is ResourceNode)
                c = Color.yellow;

            DrawDot(p, c);
        }
    }

    Vector2 WorldToMinimap(Vector3 world)
    {
        float x = Mathf.InverseLerp(worldMin.x, worldMax.x, world.x);
        float y = Mathf.InverseLerp(worldMin.y, worldMax.y, world.z);

        return new Vector2(
            minimapRect.x + x * minimapRect.width,
            minimapRect.y + (1f - y) * minimapRect.height
        );
    }

    void DrawDot(Vector2 pos, Color c)
    {
        Color prev = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(
            new Rect(pos.x, pos.y, dotSize, dotSize),
            Texture2D.whiteTexture
        );
        GUI.color = prev;
    }

    void HandleClick()
    {
        if (!Event.current.isMouse || Event.current.type != EventType.MouseDown)
            return;

        if (!minimapRect.Contains(Event.current.mousePosition))
            return;

        Vector2 local = Event.current.mousePosition;
        float nx = (local.x - minimapRect.x) / minimapRect.width;
        float ny = 1f - ((local.y - minimapRect.y) / minimapRect.height);

        float wx = Mathf.Lerp(worldMin.x, worldMax.x, nx);
        float wz = Mathf.Lerp(worldMin.y, worldMax.y, ny);

        Vector3 camPos = mainCam.transform.position;
        mainCam.transform.position = new Vector3(wx, camPos.y, wz);
    }
}
