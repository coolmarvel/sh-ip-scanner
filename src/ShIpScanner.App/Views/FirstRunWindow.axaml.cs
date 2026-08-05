using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ShIpScanner.App.Views;

// 첫 실행(저장된 대역 없음) 때 한 번 뜨는 안내 모달.
// ShowDialog<bool> 결과: true = "대역 관리 열기" 선택(호출 쪽이 이어서 대역 관리 모달을 연다).
public partial class FirstRunWindow : Window
{
    public FirstRunWindow()
    {
        InitializeComponent();
    }

    private void OnOpenManager(object? sender, RoutedEventArgs e) => Close(true);

    private void OnLater(object? sender, RoutedEventArgs e) => Close(false);
}
