using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ShIpScanner.App.Views;

// 설정 모달. DataContext = MainViewModel(공유). [저장]은 SaveSettingsCommand 실행 후 창을 닫는다.
public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void OnSaveClose(object? sender, RoutedEventArgs e) => Close();
    private void OnClose(object? sender, RoutedEventArgs e) => Close();
}
