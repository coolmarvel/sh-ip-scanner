namespace ShIpScanner.Core.Scanning;

// 바둑판 셀 하나의 상태. UI 색으로 매핑된다(원본 faIpScanner 와 동일한 색 규칙):
//   Unknown = 흰색 (아직 스캔 전)
//   Alive   = 주황 (핑 응답 있음 = 사용 중)
//   Free    = 연두 (핑 응답 없음 = 사용 가능)
public enum HostState
{
    Unknown,
    Alive,
    Free,
}
