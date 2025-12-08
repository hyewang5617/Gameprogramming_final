using UnityEngine;

/// <summary>
/// MainStateController는 공통 UI 동작을 BaseUIController에 위임하고,
/// 스테이지 선택 전용 초기화/정리만 담당합니다.
/// - Enter(): base.Enter()로 페이드인 + moving UI 타겟 적용, 필요시 스테이지 관련 초기화 추가
/// - Exit():  base.Exit()로 페이드아웃 + moving UI 타겟 적용
/// </summary>
public class MainStateController : BaseStateController
{
    protected override void Reset()
    {
        base.Reset();
        // 기본값을 여기서 세팅할 수 있습니다.
        // 예: fadeDuration = 0.35f;
    }

    /// <summary>
    /// Enter 시 공통 동작을 수행한 뒤 스테이지 전용 초기화(버튼 배치 등)를 수행합니다.
    /// 반드시 base.Enter(param)을 호출하여 BaseUIController의 공통 동작을 실행합니다.
    /// </summary>
    public override void Enter(object param = null)
    {
        // Base 에서 활성화, 페이드인, movingUI 설정을 처리합니다.
        base.Enter(param);
    }

    /// <summary>
    /// Exit 시 공통 동작(페이드아웃 + 이동 타겟 적용)을 수행합니다.
    /// 필요하면 추가 정리 코드를 여기에 둡니다.
    /// </summary>
    public override void Exit()
    {
        // MainState 전용 정리(있다면) 먼저 수행
        // ...

        // Base 에서 페이드아웃 및 비활성화를 처리합니다.
        base.Exit();
    }
}
