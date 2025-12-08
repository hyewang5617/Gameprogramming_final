using UnityEngine;
using UnityEngine.EventSystems;

public class UIBehaviourController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tween Settings")]
    public GameObject tweenTarget; // 대상 오브젝트 (비워두면 자기 자신)
    public float tweenSpeed = 4f;
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool useScaleLerp;
    public bool usePositionLerp;

    protected RectTransform rect;
    protected Vector3 originalScale;
    protected Vector3 originalPos;

    protected Vector3 scaleTarget;
    protected Vector3 posTarget;

    private float t = 0;

    protected virtual void Awake()
    {
        if (tweenTarget == null)
        {
            tweenTarget = gameObject;
        }
        rect = tweenTarget.GetComponent<RectTransform>();
        originalScale = rect.localScale;
        originalPos = rect.anchoredPosition;

        scaleTarget = originalScale;
        posTarget = originalPos;
    }

    protected virtual void Update()
    {
        t += Time.unscaledDeltaTime * tweenSpeed;
        if (useScaleLerp)
        {
            ScaleLerp();
        }
        if (usePositionLerp)
        {
            PositionLerp();
        }
    }

    protected void ScaleLerp()
    {
        float curveT = easeCurve.Evaluate(Mathf.Clamp01(t));
        rect.localScale = Vector3.Lerp(rect.localScale, scaleTarget, curveT);
    }

    protected void PositionLerp()
    {
        float curveT = easeCurve.Evaluate(Mathf.Clamp01(t));
        rect.anchoredPosition = Vector3.Lerp(rect.anchoredPosition, posTarget, curveT);
    }

    protected void RestartTween()
    {
        t = 0f;
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        
    }

    public void SetScaleTarget(Vector3 vector3)
    {
        scaleTarget = vector3;
        RestartTween();
    }

    public void SetPositionTarget(Vector3 vector3)
    {
        posTarget = vector3;
        RestartTween();
    }
}