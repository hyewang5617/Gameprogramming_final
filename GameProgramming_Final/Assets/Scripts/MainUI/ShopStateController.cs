using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class ShopStateController : BaseStateController
{
    [Header("Skill UI References")]
    [SerializeField] private Button[] skillButtons;
    [SerializeField] private TMP_Text skillTitleText;
    [SerializeField] private TMP_Text skillDescText;
    [SerializeField] private TMP_Text skillCostText;
    [SerializeField] private TMP_Text skillLevelText;
    [SerializeField] private TMP_Text currencyText;
    [SerializeField] private Button buyButton;

    protected override void Reset()
    {
        base.Reset();
    }

    public override void Enter(object param = null)
    {
        base.Enter(param);
    }

    public override void Exit()
    {
        base.Exit();
    }
}
