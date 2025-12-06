using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [Header("References")]
    [Tooltip("카메라 추적 대상")]
    public Transform holder;

    [Header("Follow Settings")]
    public Vector3 localOffset = new Vector3(0f, 0.5f, -0.3f); // 기본 오프셋
    public float followSmoothTime = 0.05f; // 추적 시간
    public bool matchRotation = true; // 회전 동기화

    [Header("Speed -> FOV Zoom")]
    public Camera cam;
    public bool enableFovSpeedZoom = true;
    public float speedForMaxFov = 15f; // 최대 FOV가 적용되는 최소 속도
    public float maxFovIncrease = 15f; // 기본 FOV에 더해질 최대 증가량 (degrees)
    public float fovSmoothTime = 0.06f; // FOV 보간 시간

    Vector3 followVelocity = Vector3.zero;
    float fovVelocity = 0f;
    float baseFov;
    float currentFov;

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

    void Update()
    {
        if (holder == null) return;

        // 부드러운 카메라 추적
        Vector3 desired = holder.TransformPoint(localOffset);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref followVelocity, followSmoothTime);

        // 회전 동기화
        if (matchRotation) transform.rotation = holder.rotation;

        // 수평 속도에 따른 FOV 보정
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