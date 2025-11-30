using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 공통 Enter/Exit 페이드 + moving UI 타깃 적용 기능을 제공하는 추상 베이스 클래스.
/// 하위 클래스는 필요하면 Enter/Exit를 오버라이드하고 base.Enter()/base.Exit()를 호출하면 됨.
/// </summary>
public abstract class BaseStateController : MonoBehaviour
{
    [Header("Canvas Fade")]
    [SerializeField] protected CanvasGroup aGroup;
    [SerializeField, Tooltip("언스케일드 시간 기준 페이드 지속시간(초)")]
    protected float fadeDuration = 0.3f;

    [System.Serializable]
    public struct MovingUIEntry
    {
        public UIBehaviourController ui;
        public bool useEnterPosition;
        public Vector3 enterTarget;
        public bool useExitPosition;
        public Vector3 exitTarget;
        public bool useEnterScale;
        public Vector3 enterScale;
        public bool useExitScale;
        public Vector3 exitScale;
    }

    [Header("Moving UI Entries")]
    [SerializeField] protected List<MovingUIEntry> movingUIEntries = new List<MovingUIEntry>();

    [Header("Gizmo")]
    [SerializeField] protected float gizmoSphereRadius = 6f;

    protected Coroutine runningCoroutine;

    protected virtual void Reset()
    {
        if (aGroup == null) aGroup = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// 상태 진입 (외부에서 호출). 하위에서 오버라이드하면 반드시 base.Enter(param)을 호출하거나
    /// base.Enter를 통해 공통 동작을 수행해야 공통 페이드/이동이 작동합니다.
    /// </summary>
    public virtual void Enter(object param = null)
    {
        // 활성화
        gameObject.SetActive(true);

        // 이전 작업 정리
        if (runningCoroutine != null)
        {
            StopCoroutine(runningCoroutine);
            runningCoroutine = null;
        }

        // 초기 alpha 세팅
        if (aGroup != null)
        {
            aGroup.alpha = 0f;
            aGroup.interactable = false;
            aGroup.blocksRaycasts = false;
        }

        // 각 moving UI에 대해 enter target 적용
        ApplyTargets(isEnter: true);

        // 페이드인 시작
        runningCoroutine = StartCoroutine(FadeCanvas(0f, 1f, fadeDuration, () =>
        {
            if (aGroup != null)
            {
                aGroup.interactable = true;
                aGroup.blocksRaycasts = true;
            }
        }));
    }

    /// <summary>
    /// 상태 종료. 하위에서 오버라이드하면 반드시 base.Exit()를 호출하거나 위임.
    /// </summary>
    public virtual void Exit()
    {
        // 이전 작업 정리
        if (runningCoroutine != null)
        {
            StopCoroutine(runningCoroutine);
            runningCoroutine = null;
        }

        // 각 moving UI에 대해 exit target 적용
        ApplyTargets(isEnter: false);

        // 페이드아웃 시작
        runningCoroutine = StartCoroutine(FadeCanvas(1f, 0f, fadeDuration, () =>
        {
            if (aGroup != null)
            {
                aGroup.interactable = false;
                aGroup.blocksRaycasts = false;
            }
            gameObject.SetActive(false);
        }));
    }

    protected virtual void ApplyTargets(bool isEnter)
    {
        if (movingUIEntries == null) return;

        for (int i = 0; i < movingUIEntries.Count; ++i)
        {
            var e = movingUIEntries[i];
            if (e.ui == null) continue;

            if (isEnter)
            {
                if (e.useEnterPosition) e.ui.SetPositionTarget(e.enterTarget);
                if (e.useEnterScale) e.ui.SetScaleTarget(e.enterScale);
            }
            else
            {
                if (e.useExitPosition) e.ui.SetPositionTarget(e.exitTarget);
                if (e.useExitScale) e.ui.SetScaleTarget(e.exitScale);
            }
        }
    }

    protected IEnumerator FadeCanvas(float fromAlpha, float toAlpha, float duration, Action onComplete = null)
    {
        if (aGroup == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        float elapsed = 0f;
        aGroup.alpha = fromAlpha;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            aGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            yield return null;
        }

        aGroup.alpha = toAlpha;
        onComplete?.Invoke();
        runningCoroutine = null;
    }

    // 선택된 상태에서 각 UI의 현재 위치 -> enter/exit 목표를 그려준다.
    protected virtual void OnDrawGizmosSelected()
    {
        if (movingUIEntries == null) return;

        foreach (var e in movingUIEntries)
        {
            if (e.ui == null) continue;
            var rt = e.ui.GetComponent<RectTransform>();
            if (rt == null) continue;

            // anchoredPosition을 월드 공간으로 변환: TransformPoint expects local position relative to RectTransform.
            Vector3 currentWorld = rt.TransformPoint(rt.anchoredPosition);
            Vector3 enterWorld = rt.TransformPoint(e.enterTarget);
            Vector3 exitWorld = rt.TransformPoint(e.exitTarget);

            // current
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(currentWorld, gizmoSphereRadius * 0.8f);

            // enter
            Gizmos.color = Color.green;
            Gizmos.DrawLine(currentWorld, enterWorld);
            Gizmos.DrawSphere(enterWorld, gizmoSphereRadius);

            // exit
            Gizmos.color = Color.red;
            Gizmos.DrawLine(currentWorld, exitWorld);
            Gizmos.DrawSphere(exitWorld, gizmoSphereRadius);

#if UNITY_EDITOR
            // 작은 라벨(에디터 전용)
            Handles.Label(currentWorld + Vector3.up * (gizmoSphereRadius * 0.2f), $"{e.ui.gameObject.name}\n(cur)");
            Handles.Label(enterWorld + Vector3.up * (gizmoSphereRadius * 0.2f), "Enter");
            Handles.Label(exitWorld + Vector3.up * (gizmoSphereRadius * 0.2f), "Exit");
#endif
        }
    }
}
