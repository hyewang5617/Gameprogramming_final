using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [Header("References")]
    public Transform holder;

    [Header("Follow Settings")]
    public Vector3 localOffset = new Vector3(0f, 0.5f, -0.3f);
    public float followSmoothTime = 0.02f;
    public float rotationSmoothTime = 0.05f;
    public bool matchRotation = true;

    [Header("Speed -> FOV Zoom")]
    public Camera cam;
    public bool enableFovSpeedZoom = true;
    public float speedForMaxFov = 15f; // 최대 FOV가 적용되는 최소 속도
    public float maxFovIncrease = 15f; // 기본 FOV 증가량
    public float fovSmoothTime = 0.06f; // FOV 보간 시간

    Vector3 followVelocity = Vector3.zero; // 카메라 추적 속도
    float fovVelocity = 0f; // FOV 변화 속도
    float baseFov; // 기본 FOV 값
    float currentFov; // 현재 FOV 값

    void Start()
    {
        if (holder == null)
        {
            if (transform.parent != null) holder = transform.parent;
            else Debug.LogWarning("[CameraMove] holder 없음");
        }

        if (cam == null)
        {
            cam = GetComponent<Camera>();
            if (cam == null) Debug.LogWarning("[CameraMove] Camera 할당 안됨");
        }

        if (cam != null)
        {
            baseFov = cam.fieldOfView;
            currentFov = baseFov;
        }
    }

    void LateUpdate()
    {
        if (holder == null) return;

        Vector3 desired = holder.TransformPoint(localOffset);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref followVelocity, followSmoothTime);

        if (matchRotation)
        {
            Quaternion desiredRot = holder.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Time.deltaTime / rotationSmoothTime);
        }

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