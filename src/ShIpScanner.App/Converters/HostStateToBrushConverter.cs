using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using ShIpScanner.Core.Scanning;

namespace ShIpScanner.App.Converters;

// [개념: IValueConverter] 바인딩 값(HostState)을 화면용 값(Brush 색)으로 변환한다.
// XAML 에서 Background="{Binding State, Converter={StaticResource StateToBrush}}" 처럼 쓴다.
// 원본 색 규칙: Unknown=흰색 / Alive=주황(사용중) / Free=연두(사용가능).
public sealed class HostStateToBrushConverter : IValueConverter
{
    private static readonly IBrush White = new SolidColorBrush(Color.Parse("#FFFFFF"));
    private static readonly IBrush Orange = new SolidColorBrush(Color.Parse("#F08C28")); // 사용 중
    private static readonly IBrush Green = new SolidColorBrush(Color.Parse("#B7E08A"));  // 사용 가능

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value switch
        {
            HostState.Alive => Orange,
            HostState.Free => Green,
            _ => White,
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
