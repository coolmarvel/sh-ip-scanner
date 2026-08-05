using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ShIpScanner.App.Views;

// 대역 관리 모달. DataContext 는 여는 쪽(MainWindow)에서 MainViewModel 을 그대로 넘겨준다.
public partial class SubnetManagerWindow : Window
{
    public SubnetManagerWindow()
    {
        InitializeComponent();
    }

    private void OnClose(object? sender, RoutedEventArgs e) => Close();
}
