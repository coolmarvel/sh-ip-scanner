namespace ShIpScanner.Shared;

// 자동 종료 스케줄의 '순수 로직'. 실제 종료는 하지 않고 "언제/경고할지/연장" 만 계산한다.
// now 를 인자로 받으므로 테스트가 쉽다(시계 주입). 에이전트가 주기적으로 호출해 판단한다.
public sealed class ShutdownScheduler
{
    public TimeOnly ShutdownTime { get; }      // 매일 종료 시각(예: 19:00)
    public int WarnLeadSeconds { get; }         // 종료 몇 초 전부터 경고할지(예: 600 = 10분)
    public DateTime? DeferredUntil { get; private set; } // 연장으로 미뤄진 시각(이번 사이클 한정)

    public ShutdownScheduler(TimeOnly shutdownTime, int warnLeadSeconds = 600)
    {
        ShutdownTime = shutdownTime;
        WarnLeadSeconds = warnLeadSeconds;
    }

    // 이번 날짜 사이클의 실제 종료 예정 시각(연장 반영). now 기준 과거일 수도 있다(그럼 종료 대상).
    private DateTime EffectiveInstant(DateTime now)
    {
        var t = now.Date + ShutdownTime.ToTimeSpan();
        if (DeferredUntil is DateTime d && d.Date == now.Date && d > t) t = d;
        return t;
    }

    // 화면 표시용: 다가오는 종료 시각(now 이후).
    public DateTime NextScheduled(DateTime now)
    {
        var t = EffectiveInstant(now);
        return now < t ? t : now.Date.AddDays(1) + ShutdownTime.ToTimeSpan();
    }

    // 지금 경고를 띄워야 하나?(종료 WarnLeadSeconds 이내로 다가옴)
    public bool ShouldWarn(DateTime now)
    {
        var t = EffectiveInstant(now);
        return now < t && (t - now).TotalSeconds <= WarnLeadSeconds;
    }

    // 지금 종료해야 하나?(예정 시각 도달/경과)
    public bool ShouldShutdownNow(DateTime now) => now >= EffectiveInstant(now);

    // 연장근무자 연기 — 이번 사이클 종료를 minutes 만큼 미룬다.
    public void Defer(DateTime now, int minutes) => DeferredUntil = EffectiveInstant(now).AddMinutes(minutes);

    // 새 날 사이클로 넘어가면 연장 상태를 초기화(다음 날 다시 정상 스케줄).
    public void ResetIfNewDay(DateTime now)
    {
        if (DeferredUntil is DateTime d && d.Date != now.Date && now > d)
            DeferredUntil = null;
    }
}
