namespace ShIpScanner.Core.Scanning;

// [개념: readonly record struct] 값 하나의 결과를 담는 불변 데이터.
// record 는 값 기반 동등성(==) 과 간결한 선언을, struct 는 힙 할당 없는 값 타입을 준다.
// 핑 한 번의 결과(마지막 옥텟, IP, 살아있음, 왕복시간ms).
public readonly record struct PingOutcome(int LastOctet, string Ip, bool Alive, long RttMs);
