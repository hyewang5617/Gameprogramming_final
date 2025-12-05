using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [Header("References")]
    public Transform holder;

    [Header("Follow Settings")]
    public Vector3 localOffset = new Vector3(0f, 0.5f, -0.3f);
    public float followSmoothTime = 0.1f;
    public bool matchRotation = true;

    [Header("Speed FOV Zoom")]
    public Camera cam;
    public bool enableFovSpeedZoom = true;
    public float speedForMaxFov = 15f;
    public float maxFovIncrease = 15f;
    public float fovSmoothTime = 0.06f;

    Vector3 followVelocity;
    float fovVelocity;
    float baseFov;
    float currentFov;

    void Start()
    {
        if (holder == null && transform.parent != null)
            holder = transform.parent;

        if (cam == null)
            cam = GetComponent<Camera>();

        if (cam != null)
        {
            baseFov = cam.fieldOfView;
            currentFov = baseFov;
        }
    }

    void FixedUpdate()
    {
        if (holder == null) return;

        float dt = Mathf.Max(Time.fixedDeltaTime, 1e-6f);

        Vector3 desired = holder.TransformPoint(localOffset);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref followVelocity, followSmoothTime, Mathf.Infinity, dt);

        if (matchRotation) transform.rotation = holder.rotation;

        if (cam != null && enableFovSpeedZoom)
        {
            Vector3 horizVel = new Vector3(followVelocity.x, 0f, followVelocity.z);
            float horizSpeed = horizVel.magnitude;

            float t = speedForMaxFov > 0 ? Mathf.Clamp01(horizSpeed / speedForMaxFov) : 0;
            float desiredFov = baseFov + maxFovIncrease * t;

            currentFov = Mathf.SmoothDamp(currentFov, desiredFov, ref fovVelocity, fovSmoothTime, Mathf.Infinity, dt);
            cam.fieldOfView = currentFov;
        }
    }
}