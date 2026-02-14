using UnityEngine;
using System.Collections.Generic;

public class Minimap : MonoBehaviour
{
    [Header("Map Bounds")]
    public Vector2 worldMin = new Vector2(-100, -100);
    public Vector2 worldMax = new Vector2(100, 100);

    [Header("UI")]
    public Rect minimapRect = new Rect(10, 10, 200, 200);
    public Rect expandedRect = new Rect(10, 10, 400, 400);
    public bool isExpanded = false;

    [Header("Icons")]
    public float dotSize = 4f;

    [Header("Rotation")]
    public float rotationOffset = 90f;
    // Fixes the 90-degree mismatch between world and minimap
    
    public bool useIMGUI = true;

    Camera mainCam;

    // Resource type → color
    Dictionary<string, Color> resourceColors = new Dictionary<string, Color>();

    void Awake()
    {
        mainCam = Camera.main;
    }

    void OnGUI()
    {
        if (!useIMGUI) return;

        Rect r = isExpanded ? expandedRect : minimapRect;

        GUI.Box(r, "Minimap");

        // Expand/collapse button
        if (GUI.Button(new Rect(r.xMax - 25, r.y + 5, 20, 20), isExpanded ? "-" : "+"))
        {
            isExpanded = !isExpanded;
        }

        DrawEntities<Unit>(r);
        DrawEntities<Turret>(r);
        DrawEntities<ResourceNode>(r);
        DrawEntities<Civilian>(r);

        HandleClick(r);
    }

    void DrawEntities<T>(Rect r) where T : MonoBehaviour
    {
        T[] all = GameObject.FindObjectsOfType<T>();

        foreach (var e in all)
        {
            Vector3 pos = e.transform.position;
            Vector2 p = WorldToMinimap(pos, r);

            Color c = Color.white;

            if (e.TryGetComponent<Unit>(out var u))
                c = TeamColorUtils.Get(u.teamID);

            else if (e.TryGetComponent<Turret>(out var t))
                c = TeamColorUtils.Get(t.teamID);

            else if (e is ResourceNode rn)
            {
                if (resourceColors.TryGetValue(ResourceIdUtility.GetKey(rn.resource), out var rc))
                    c = rc;
                else
                    c = Color.magenta; // fallback for undefined types
            }

            DrawDot(p, c);
        }
    }

    Vector2 WorldToMinimap(Vector3 world, Rect r)
    {
        // Normalize world position
        float nx = Mathf.InverseLerp(worldMin.x, worldMax.x, world.x);
        float ny = Mathf.InverseLerp(worldMin.y, worldMax.y, world.z);

        // Apply rotation correction
        Vector2 centered = new Vector2(nx - 0.5f, ny - 0.5f);
        float rad = rotationOffset * Mathf.Deg2Rad;

        float rx = centered.x * Mathf.Cos(rad) - centered.y * Mathf.Sin(rad);
        float ry = centered.x * Mathf.Sin(rad) + centered.y * Mathf.Cos(rad);

        rx += 0.5f;
        ry += 0.5f;

        return new Vector2(
            r.x + rx * r.width,
            r.y + (1f - ry) * r.height
        );
    }

    void DrawDot(Vector2 pos, Color c)
    {
        Color prev = GUI.color;
        GUI.color = c;

        GUI.DrawTexture(
            new Rect(pos.x - dotSize * 0.5f, pos.y - dotSize * 0.5f, dotSize, dotSize),
            Texture2D.whiteTexture
        );

        GUI.color = prev;
    }

    void HandleClick(Rect r)
    {
        if (!Event.current.isMouse || Event.current.type != EventType.MouseDown)
            return;

        if (!r.Contains(Event.current.mousePosition))
            return;

        Vector2 local = Event.current.mousePosition;

        float nx = (local.x - r.x) / r.width;
        float ny = 1f - ((local.y - r.y) / r.height);

        float wx = Mathf.Lerp(worldMin.x, worldMax.x, nx);
        float wz = Mathf.Lerp(worldMin.y, worldMax.y, ny);

        Vector3 camPos = mainCam.transform.position;
        mainCam.transform.position = new Vector3(wx, camPos.y, wz);
    }
}