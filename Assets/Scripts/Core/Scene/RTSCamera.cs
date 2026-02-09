using UnityEngine;

public class RTSCamera : MonoBehaviour
{
    [Header("Keyboard Pan (WASD)")]
    public float keyboardPanSpeed = 20f;

    [Header("Mouse Wheel Zoom")]
    public float zoomSensitivity = 25f;
    public float minY = 10f;
    public float maxY = 60f;

    [Header("Edge Scrolling")]
    public bool edgeScrollEnabled = true;
    public float edgeBorderPixels = 18f;
    public float edgeScrollSpeed = 22f;

    [Header("Right Click Rotate (Spin)")]
    public bool rightClickRotateEnabled = true;
    public float rightClickYawSensitivity = 0.25f;

    [Header("Middle Click Drag Pan")]
    public bool middleClickDragEnabled = true;
    public float middleDragPanSensitivity = 0.02f;

    [Header("Optional Rotate Keys (Q/E)")]
    public bool enableQE = false;
    public float rotateKeySpeed = 120f;

    [Header("Optional Map Bounds")]
    public bool clampToBounds = false;
    public float minX = -200f;
    public float maxX = 200f;
    public float minZ = -200f;
    public float maxZ = 200f;

    private bool isRotating;
    private bool isDragging;
    private Vector3 lastMousePos;
    private float targetY;

    void Start()
    {
        targetY = transform.position.y;
    }

    void Update()
    {
        Vector3 pos = transform.position;

        // Update flat movement basis only when not rotating
        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 flatRight = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

        // RMB rotate
        if (rightClickRotateEnabled)
        {
            if (Input.GetMouseButtonDown(1))
            {
                isRotating = true;
                lastMousePos = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(1)) isRotating = false;

            if (isRotating)
            {
                Vector3 delta = Input.mousePosition - lastMousePos;
                lastMousePos = Input.mousePosition;

                float yawDegrees = delta.x * rightClickYawSensitivity;
                transform.Rotate(Vector3.up, yawDegrees, Space.World);

                flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
                flatRight = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
            }
        }

        // MMB drag pan
        if (middleClickDragEnabled)
        {
            if (Input.GetMouseButtonDown(2))
            {
                isDragging = true;
                lastMousePos = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(2)) isDragging = false;

            if (isDragging)
            {
                Vector3 delta = Input.mousePosition - lastMousePos;
                lastMousePos = Input.mousePosition;

                float heightScale = Mathf.Clamp01((pos.y - minY) / Mathf.Max(0.001f, (maxY - minY)));
                float scaledSensitivity = middleDragPanSensitivity * Mathf.Lerp(0.75f, 2.0f, heightScale);

                Vector3 dragMove = (-flatRight * delta.x + -flatForward * delta.y) * scaledSensitivity;
                pos += dragMove;
            }
        }

        // Keyboard pan
        Vector3 panVel = Vector3.zero;
        float x = 0f, z = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) z += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) z -= 1f;

        if (Mathf.Abs(x) > 0.01f || Mathf.Abs(z) > 0.01f)
        {
            Vector3 kb = (flatRight * x + flatForward * z).normalized;
            panVel += kb * keyboardPanSpeed;
        }

        // Edge scrolling
        if (edgeScrollEnabled && !isDragging && !isRotating)
        {
            Vector3 mp = Input.mousePosition;
            Vector3 edgeDir = Vector3.zero;

            if (mp.x <= edgeBorderPixels) edgeDir += -flatRight;
            if (mp.x >= Screen.width - edgeBorderPixels) edgeDir += flatRight;
            if (mp.y <= edgeBorderPixels) edgeDir += -flatForward;
            if (mp.y >= Screen.height - edgeBorderPixels) edgeDir += flatForward;

            if (edgeDir.sqrMagnitude > 0.0001f)
            {
                edgeDir.Normalize();
                panVel += edgeDir * edgeScrollSpeed;
            }
        }

        // Apply pan velocity
        pos += panVel * Time.deltaTime;

        // Mouse wheel zoom (smooth)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.00001f)
        {
            targetY += scroll * zoomSensitivity;
            targetY = Mathf.Clamp(targetY, minY, maxY);
        }

        // Smooth zoom
        pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * 10f);

        // Optional rotate keys
        if (enableQE)
        {
            if (Input.GetKey(KeyCode.Q))
                transform.Rotate(Vector3.up, -rotateKeySpeed * Time.deltaTime, Space.World);
            if (Input.GetKey(KeyCode.E))
                transform.Rotate(Vector3.up, rotateKeySpeed * Time.deltaTime, Space.World);
        }

        // Optional bounds clamp
        if (clampToBounds)
        {
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        }

        transform.position = pos;

        // Dynamic tilt based on zoom
        float tiltAngle = Mathf.Lerp(30f, 60f, (targetY - minY) / (maxY - minY));
        transform.rotation = Quaternion.Euler(tiltAngle, transform.eulerAngles.y, 0f);
    }
}