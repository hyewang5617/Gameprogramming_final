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

    void Update()
    {
        if (holder == null) return;

        Vector3 desired = holder.TransformPoint(localOffset);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref followVelocity, followSmoothTime);

        if (matchRotation) transform.rotation = holder.rotation;

        if (cam != null && enableFovSpeedZoom)
        {
            Rigidbody holderRigid = holder.GetComponent<Rigidbody>();
            float horizSpeed = 0f;
            
            if (holderRigid != null)
            {
                Vector3 horizVel = new Vector3(holderRigid.velocity.x, 0f, holderRigid.velocity.z);
                horizSpeed = horizVel.magnitude;
            }
            else
            {
                Vector3 horizVel = new Vector3(followVelocity.x, 0f, followVelocity.z);
                horizSpeed = horizVel.magnitude;
            }

            float t = speedForMaxFov > 0 ? Mathf.Clamp01(horizSpeed / speedForMaxFov) : 0;
            float desiredFov = baseFov + maxFovIncrease * t;

            currentFov = Mathf.SmoothDamp(currentFov, desiredFov, ref fovVelocity, fovSmoothTime);
            cam.fieldOfView = currentFov;
        }
    }
}