using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ShIpScanner.App.ViewModels;

namespace ShIpScanner.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    // 바둑판 셀 더블클릭 → 해당 PC 제어 창(살아있는 IP 만).
    private void OnCellDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Control { DataContext: HostCellViewModel cell }) return;
        if (DataContext is not MainViewModel vm) return;
        if (cell.State != Core.Scanning.HostState.Alive) return;

        var dlg = new PcControlWindow(cell.Ip, cell.HostName, cell.AgentInstalled, cell.AgentVersion,
            vm.AgentToken, vm.AgentPort);
        dlg.ShowDialog(this);
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
