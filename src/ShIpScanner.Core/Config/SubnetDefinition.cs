using System.Text.Json.Serialization;

namespace ShIpScanner.Core.Config;

// 스캔할 서브넷 하나의 정의(설정 파일에 저장되는 단위).
// Base = 앞 3옥텟("192.168.0"), Label = 관리자용 별칭(선택, 예: "본원").
public sealed class SubnetDefinition
{
    public string Base { get; set; } = "";
    public string Label { get; set; } = "";

    // 드롭다운에 보일 문구. JsonIgnore = 저장 파일엔 안 씀(파생 값이라).
    [JsonIgnore]
    public string Display => string.IsNullOrWhiteSpace(Label) ? $"{Base}.x" : $"{Label} ({Base}.x)";
}
