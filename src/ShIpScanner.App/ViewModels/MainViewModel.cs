using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShIpScanner.Core.Config;
using ShIpScanner.Core.Naming;
using ShIpScanner.Core.Net;
using ShIpScanner.Core.Scanning;

namespace ShIpScanner.App.ViewModels;

// [개념: MVVM 의 VM] 화면(View)과 로직 사이의 상태 + 명령 계층.
public partial class MainViewModel : ViewModelBase
{
    private readonly SubnetScanner _scanner = new(maxParallel: 128, timeoutMs: 1000);
    private readonly IHostNameResolver _resolver =
        new CompositeHostNameResolver(new NetBiosNameResolver(), new ReverseDnsResolver());
    private readonly SubnetStore _store = new();
    private CancellationTokenSource? _cts;

    // 스캔 대상 대역 목록(드롭다운) — 관리자가 여러 대역을 오가며 고른다.
    public ObservableCollection<SubnetDefinition> Subnets { get; } = new();

    public ObservableCollection<HostCellViewModel> Cells { get; } = new();
    public ObservableCollection<string> Log { get; } = new();

    [ObservableProperty] private SubnetDefinition? _selectedSubnet;
    [ObservableProperty] private string _newSubnet = "";   // "새 대역 추가" 입력칸
    [ObservableProperty] private string _localIpText = "";  // 자동 감지된 내 IP(안내용)
    [ObservableProperty] private bool _isScanning;
    [ObservableProperty] private int _aliveCount;
    [ObservableProperty] private int _scannedCount;

    public MainViewModel()
    {
        // 저장된 대역 목록 로드(없으면 기본 3개).
        foreach (var d in _store.Load()) Subnets.Add(d);

        // 내 IP 자동 감지 → 내 대역이 목록에 없으면 맨 앞에 추가하고, 그걸 기본 선택.
        var ip = LocalNetwork.GetPrimaryIPv4();
        LocalIpText = ip?.ToString() ?? "";
        var localBase = ip != null ? LocalNetwork.GetSubnetBase(ip) : null;
        if (localBase != null && !Subnets.Any(s => s.Base == localBase))
        {
            Subnets.Insert(0, new SubnetDefinition { Base = localBase, Label = "내 대역" });
            Persist();
        }
        SelectedSubnet = Subnets.FirstOrDefault(s => s.Base == localBase) ?? Subnets.FirstOrDefault();

        BuildCells();
    }

    private string CurrentBase => SelectedSubnet?.Base ?? "192.168.0";

    private void BuildCells()
    {
        Cells.Clear();
        for (int o = 1; o <= 254; o++)
            Cells.Add(new HostCellViewModel(o, $"{CurrentBase}.{o}"));
    }

    // 드롭다운에서 대역을 바꾸면 바둑판을 그 대역 기준으로 초기화(흰색).
    partial void OnSelectedSubnetChanged(SubnetDefinition? value)
    {
        if (Cells.Count == 0) return;
        foreach (var c in Cells)
        {
            c.Ip = $"{CurrentBase}.{c.Octet}";
            c.State = HostState.Unknown;
            c.HostName = "";
            c.RttMs = 0;
        }
    }

    // [검색 시작]
    [RelayCommand]
    private async Task StartScanAsync()
    {
        if (IsScanning || SelectedSubnet is null) return;

        string b = CurrentBase;
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
        AddLog($"[{b}.x] 대역을 검색합니다.");
        AddLog("사용 중인 IP 의 호스트 이름(PC명)을 함께 조회합니다.");

        IsScanning = true;
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

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

    private async void OnPing(PingOutcome o)
    {
        ScannedCount++;
        var cell = Cells[o.LastOctet - 1];

        if (!o.Alive)
        {
            cell.State = HostState.Free;
            return;
        }

        cell.State = HostState.Alive;
        cell.RttMs = o.RttMs;
        AliveCount++;

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

    [RelayCommand]
    private void StopScan() => _cts?.Cancel();

    // 대역 추가(관리자) — "192.168.85" 처럼 앞 3옥텟만 입력.
    [RelayCommand]
    private void AddSubnet()
    {
        var b = (NewSubnet ?? "").Trim();
        // 사용자가 "192.168.85.1" 처럼 4옥텟을 넣어도 앞 3옥텟으로 정규화.
        var parts = b.Split('.');
        if (parts.Length == 4) b = string.Join('.', parts.Take(3));

        if (!SubnetStore.IsValidBase(b))
        {
            AddLog($"대역 추가 실패: '{NewSubnet}' 는 올바른 형식이 아닙니다 (예: 192.168.85).");
            return;
        }
        if (Subnets.Any(s => s.Base == b))
        {
            AddLog($"이미 목록에 있는 대역입니다: {b}.x");
            SelectedSubnet = Subnets.First(s => s.Base == b);
            NewSubnet = "";
            return;
        }
        var def = new SubnetDefinition { Base = b };
        Subnets.Add(def);
        SelectedSubnet = def;
        NewSubnet = "";
        Persist();
        AddLog($"대역을 추가했습니다: {b}.x");
    }

    // 선택 대역 삭제(최소 1개는 남긴다).
    [RelayCommand]
    private void RemoveSubnet()
    {
        if (SelectedSubnet is null || Subnets.Count <= 1) return;
        var removed = SelectedSubnet;
        int idx = Subnets.IndexOf(removed);
        Subnets.Remove(removed);
        SelectedSubnet = Subnets[Math.Max(0, idx - 1)];
        Persist();
        AddLog($"대역을 삭제했습니다: {removed.Base}.x");
    }

    private void Persist() => _store.Save(Subnets);

    private void AddLog(string line)
    {
        Log.Add(line);
        if (Log.Count > 500) Log.RemoveAt(0);
    }
}
