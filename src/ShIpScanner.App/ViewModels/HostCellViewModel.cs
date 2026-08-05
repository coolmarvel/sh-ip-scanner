using CommunityToolkit.Mvvm.ComponentModel;
using ShIpScanner.Core.Scanning;

namespace ShIpScanner.App.ViewModels;

// 바둑판의 셀 하나. [ObservableProperty] 로 State/HostName 이 바뀌면 UI 의 색·글자가 자동 갱신된다.
public partial class HostCellViewModel : ObservableObject
{
    public int Octet { get; }        // 마지막 옥텟(1~254). 셀에 크게 표시되는 번호.

    public HostCellViewModel(int octet, string ip)
    {
        Octet = octet;
        _ip = ip;
    }

    [ObservableProperty] private string _ip;                        // "192.168.80.12"
    [ObservableProperty] private HostState _state = HostState.Unknown; // 색(흰/주황/연두)을 결정
    [ObservableProperty] private string _hostName = "";             // 예: "관리부-PC01"
    [ObservableProperty] private long _rttMs;                       // 왕복 시간(툴팁용)
}
