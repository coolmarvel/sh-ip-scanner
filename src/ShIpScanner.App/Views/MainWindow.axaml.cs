using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ShIpScanner.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    // 좌상단 아이콘 메뉴 → 각 모달을 연다. DataContext(MainViewModel)를 그대로 넘겨 데이터를 공유한다.
    private async void OnManageSubnets(object? sender, RoutedEventArgs e)
    {
        var dialog = new SubnetManagerWindow { DataContext = DataContext };
        await dialog.ShowDialog(this);
    }

    private async void OnSettings(object? sender, RoutedEventArgs e)
    {
        var dialog = new SettingsWindow { DataContext = DataContext };
        await dialog.ShowDialog(this);
    }

    private async void OnAbout(object? sender, RoutedEventArgs e)
    {
        var dialog = new AboutWindow();
        await dialog.ShowDialog(this);
    }
}
