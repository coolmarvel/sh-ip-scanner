using Avalonia.Controls;
using Avalonia.Interactivity;
using ShIpScanner.App.ViewModels;

namespace ShIpScanner.App.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // 첫 실행(저장된 대역 없음)이면 안내 팝업 → 원하면 곧바로 대역 관리 모달로 이어간다.
        Opened += async (_, _) =>
        {
            if (DataContext is not MainViewModel vm || !vm.IsFirstRun) return;
            vm.IsFirstRun = false; // 재진입 방지(창 재활성화 등으로 다시 뜨지 않게)
            var notice = new FirstRunWindow { DataContext = vm };
            var openManager = await notice.ShowDialog<bool>(this);
            if (openManager)
                await new SubnetManagerWindow { DataContext = vm }.ShowDialog(this);
        };
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
