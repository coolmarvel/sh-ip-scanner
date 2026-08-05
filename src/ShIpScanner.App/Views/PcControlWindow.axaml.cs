using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ShIpScanner.Shared;

namespace ShIpScanner.App.Views;

// 특정 PC(에이전트)를 제어하는 창. 콘솔에서 셀을 더블클릭하면 열린다.
public partial class PcControlWindow : Window
{
    private readonly string _ip;
    private readonly string _token;
    private readonly int _port;

    public PcControlWindow(string ip, string hostName, bool agentInstalled, string agentVersion, string token, int port)
    {
        InitializeComponent();
        _ip = ip;
        _token = token;
        _port = port;

        HeaderText.Text = string.IsNullOrWhiteSpace(hostName) ? ip : $"{hostName}  ({ip})";
        AgentText.Text = agentInstalled
            ? $"에이전트 설치됨 (v{agentVersion}) — 전원/세션 제어 가능"
            : "에이전트 미설치 — 원격 접속(RDP)만 가능, 전원 제어는 에이전트 설치 필요";

        // 에이전트가 있으면 현재 일정을 불러와 채운다.
        if (agentInstalled) _ = LoadScheduleAsync();
    }

    // 현재 에이전트 상태(관리자 일정)를 조회해 UI 에 반영.
    private async Task LoadScheduleAsync()
    {
        var status = await AgentClient.GetStatusAsync(_ip, _token, _port, timeoutMs: 1200);
        if (status == null) return;
        ScheduleEnabledBox.IsChecked = status.ScheduleEnabled;
        ShutdownTimeBox.Text = status.ShutdownTime;
        AllowExtendBox.IsChecked = status.AllowExtend;
    }

    // 관리자가 정한 종료 일정을 에이전트에 전송(종료 시각은 관리자만 설정).
    private async void OnApplySchedule(object? s, RoutedEventArgs e)
    {
        var time = (ShutdownTimeBox.Text ?? "").Trim();
        if (!TimeOnly.TryParse(time, out _))
        {
            ResultText.Foreground = Avalonia.Media.Brushes.IndianRed;
            ResultText.Text = "종료 시각 형식이 올바르지 않습니다 (예: 19:00).";
            return;
        }
        var req = new CommandRequest
        {
            Type = CommandType.SetSchedule,
            AuthToken = _token,
            ScheduleEnabled = ScheduleEnabledBox.IsChecked ?? false,
            ShutdownTime = time,
            AllowExtend = AllowExtendBox.IsChecked ?? false,
        };
        var resp = await AgentClient.SendAsync(_ip, req, _port, timeoutMs: 1500);
        if (resp is { Ok: true })
        {
            ResultText.Foreground = Avalonia.Media.Brushes.Green;
            ResultText.Text = $"일정 적용됨: {time} 종료 / 자체연장 {(req.AllowExtend ? "허용" : "불가")}.";
        }
        else
        {
            ResultText.Foreground = Avalonia.Media.Brushes.IndianRed;
            ResultText.Text = resp == null ? "실패: 에이전트 연결 불가." : $"거부됨: {resp.Error}";
        }
    }

    private async Task SendAsync(CommandType type, string? text = null, int extend = 60)
    {
        ResultText.Foreground = Avalonia.Media.Brushes.Gray;
        ResultText.Text = "전송 중…";
        var req = new CommandRequest { Type = type, AuthToken = _token, Text = text, DelaySeconds = 60, ExtendMinutes = extend };
        var resp = await AgentClient.SendAsync(_ip, req, _port, timeoutMs: 1500);

        if (resp == null)
        {
            ResultText.Foreground = Avalonia.Media.Brushes.IndianRed;
            ResultText.Text = "실패: 에이전트에 연결할 수 없습니다(미설치/방화벽/토큰 확인).";
        }
        else if (!resp.Ok)
        {
            ResultText.Foreground = Avalonia.Media.Brushes.IndianRed;
            ResultText.Text = $"거부됨: {resp.Error}";
        }
        else
        {
            ResultText.Foreground = Avalonia.Media.Brushes.Green;
            ResultText.Text = $"완료: {type} 명령을 보냈습니다.";
        }
    }

    private async void OnShutdown(object? s, RoutedEventArgs e) => await SendAsync(CommandType.Shutdown, "관리자 종료");
    private async void OnReboot(object? s, RoutedEventArgs e) => await SendAsync(CommandType.Reboot, "관리자 재부팅");
    private async void OnLock(object? s, RoutedEventArgs e) => await SendAsync(CommandType.Lock);
    private async void OnExtend(object? s, RoutedEventArgs e) => await SendAsync(CommandType.ExtendSession, extend: 60);
    private async void OnMessage(object? s, RoutedEventArgs e) => await SendAsync(CommandType.Message, MessageBox.Text ?? "");

    // RDP 접속 — 에이전트가 없어도 가능(Windows mstsc). 대상 PC 의 RDP 허용이 전제.
    private void OnRdp(object? s, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("mstsc", $"/v:{_ip}") { UseShellExecute = true });
            ResultText.Foreground = Avalonia.Media.Brushes.Green;
            ResultText.Text = "원격 데스크톱(mstsc)을 실행했습니다.";
        }
        catch (Exception ex)
        {
            ResultText.Foreground = Avalonia.Media.Brushes.IndianRed;
            ResultText.Text = $"RDP 실행 실패: {ex.Message}";
        }
    }

    private void OnClose(object? s, RoutedEventArgs e) => Close();
}
