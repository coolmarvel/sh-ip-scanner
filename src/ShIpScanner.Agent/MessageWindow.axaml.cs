using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ShIpScanner.Agent;

// 관리자가 보낸 안내 메시지를 표시하는 창.
public partial class MessageWindow : Window
{
    public MessageWindow(string text)
    {
        InitializeComponent();
        MessageText.Text = text;
    }

    private void OnClose(object? sender, RoutedEventArgs e) => Close();
}
