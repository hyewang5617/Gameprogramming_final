using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonBehaviourController : UIBehaviourController
{
    [Header("Hover Target")]
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1f);
    public Vector3 hoverPosOffset = Vector3.zero;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        scaleTarget = hoverScale;
        posTarget = originalPos + hoverPosOffset;
        RestartTween();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        scaleTarget = originalScale;
        posTarget = originalPos;
        RestartTween();
    }
}