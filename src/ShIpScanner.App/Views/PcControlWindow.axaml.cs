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
