using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShIpScanner.Core.Naming;
using ShIpScanner.Core.Net;
using ShIpScanner.Core.Scanning;

namespace ShIpScanner.App.ViewModels;

// [개념: MVVM 의 VM] 화면(View)과 로직 사이의 상태 + 명령 계층.
// View 는 여기의 프로퍼티/커맨드에 바인딩만 하고, 값이 바뀌면 화면이 따라온다.
public partial class MainViewModel : ViewModelBase
{
    private readonly SubnetScanner _scanner = new(maxParallel: 128, timeoutMs: 1000);

    // 이름 조회: NetBIOS(한글 PC명) 우선 → 실패 시 역DNS. (→ Core/Naming)
    private readonly IHostNameResolver _resolver =
        new CompositeHostNameResolver(new NetBiosNameResolver(), new ReverseDnsResolver());

    private CancellationTokenSource? _cts;

    // 바둑판 셀 254개. View 는 이 컬렉션을 UniformGrid 로 그린다.
    public ObservableCollection<HostCellViewModel> Cells { get; } = new();

    // 검은 로그 영역에 뿌릴 진행 메시지들.
    public ObservableCollection<string> Log { get; } = new();

    [ObservableProperty] private string _localIp = "";
    [ObservableProperty] private bool _isScanning;
    [ObservableProperty] private int _aliveCount;
    [ObservableProperty] private int _scannedCount;

    public MainViewModel()
    {
        // 로컬 IP 자동 감지 → 텍스트박스에 채운다(원본과 동일 동작).
        var ip = LocalNetwork.GetPrimaryIPv4();
        LocalIp = ip?.ToString() ?? "192.168.0.1";
        BuildCells();
    }

    private void BuildCells()
    {
        Cells.Clear();
        string b = SubnetBaseOrDefault();
        for (int o = 1; o <= 254; o++)
            Cells.Add(new HostCellViewModel(o, $"{b}.{o}")); // 초기: 전부 흰색(Unknown)
    }

    private string SubnetBaseOrDefault()
    {
        if (IPAddress.TryParse(LocalIp, out var ip))
        {
            var b = LocalNetwork.GetSubnetBase(ip);
            if (b != null) return b;
        }
        return "192.168.0";
    }

    // [검색 시작] 버튼
    [RelayCommand]
    private async Task StartScanAsync()
    {
        if (IsScanning) return;

        // 대역을 텍스트박스 기준으로 재설정하고, 바둑판을 흰색으로 초기화.
        string b = SubnetBaseOrDefault();
        foreach (var c in Cells)
        {
            c.Ip = $"{b}.{c.Octet}";
            c.State = HostState.Unknown;
            c.HostName = "";
            c.RttMs = 0;
        }
        Log.Clear();
        AliveCount = 0;
        ScannedCount = 0;
        AddLog("사용하고 있는 IP 의 호스트 이름을 검색하겠습니다.");
        AddLog("호스트 이름 검색은 다소 시간이 걸립니다.");

        IsScanning = true;
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        // Progress<T> 는 콜백을 UI 스레드(생성된 컨텍스트)로 마샬링한다 → 셀 갱신이 안전하다.
        var progress = new Progress<PingOutcome>(OnPing);
        try
        {
            await _scanner.ScanAsync(b, progress, ct);
            AddLog($"검색 완료 — 사용 중 {AliveCount}대 / 검사 {ScannedCount}개.");
        }
        catch (OperationCanceledException)
        {
            AddLog("검색을 중지했습니다.");
        }
        finally
        {
            IsScanning = false;
        }
    }

    // 핑 결과 하나가 도착할 때마다 호출(UI 스레드).
    private async void OnPing(PingOutcome o)
    {
        ScannedCount++;
        var cell = Cells[o.LastOctet - 1];

        if (!o.Alive)
        {
            cell.State = HostState.Free; // 연두 = 사용 가능
            return;
        }

        cell.State = HostState.Alive;    // 주황 = 사용 중
        cell.RttMs = o.RttMs;
        AliveCount++;

        // 이름은 뒤이어 비동기로 채운다(느릴 수 있어 핑 표시를 막지 않도록 분리).
        var name = await _resolver.ResolveAsync(o.Ip, _cts?.Token ?? CancellationToken.None);
        if (!string.IsNullOrWhiteSpace(name))
        {
            cell.HostName = name;
            AddLog($"IP:{o.Ip} >>>> {name} 이(가) 사용 중입니다.");
        }
        else
        {
            AddLog($"IP:{o.Ip} >>>> (이름 미확인) 사용 중입니다.");
        }
    }

    // [중지] 버튼
    [RelayCommand]
    private void StopScan() => _cts?.Cancel();

    private void AddLog(string line)
    {
        Log.Add(line);
        if (Log.Count > 500) Log.RemoveAt(0); // 무한 증가 방지
    }
}
