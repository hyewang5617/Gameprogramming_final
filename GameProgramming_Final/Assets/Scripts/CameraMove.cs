using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    [Header("References")]
    [Tooltip("카메라 추적 대상")]
    public Transform holder;

    [Header("Follow Settings")]
    public Vector3 localOffset = new Vector3(0f, 0.5f, -0.3f); // 기본 오프셋
    public float followSmoothTime = 0.1f; // 추적 시간
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

    void FixedUpdate()
    {
        if (holder == null) return;

        float dt = Mathf.Max(Time.fixedDeltaTime, 1e-6f);

        // 부드러운 카메라 추적
        Vector3 desired = holder.TransformPoint(localOffset);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref followVelocity, followSmoothTime, Mathf.Infinity, dt);

        // 회전 동기화
        if (matchRotation) transform.rotation = holder.rotation;

        // 수평 속도에 따른 FOV 보정
        if (cam != null && enableFovSpeedZoom)
        {
            Vector3 horizVel = new Vector3(followVelocity.x, 0f, followVelocity.z);
            float horizSpeed = horizVel.magnitude;

            float t = speedForMaxFov > 0 ? Mathf.Clamp01(horizSpeed / speedForMaxFov) : 0;
            float desiredFov = baseFov + maxFovIncrease * t;

            // FOV 부드럽게 보간
            currentFov = Mathf.SmoothDamp(currentFov, desiredFov, ref fovVelocity, fovSmoothTime, Mathf.Infinity, dt);
            cam.fieldOfView = currentFov;
        }
    }
}