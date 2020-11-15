using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace AAV.WPF.Converters
{
  public class ConverterUni : MarkupExtension, IValueConverter // a port to Core3 from UniConverter - 2019-12
  {
    public bool IsInverted { get; set; }

    readonly Brush
      _b0 = new SolidColorBrush(Color.FromRgb(128, 128, 128)),
      _b1 = new SolidColorBrush(Color.FromRgb(80, 80, 210)),
      _b2 = new SolidColorBrush(Color.FromRgb(0, 160, 160)),
      _b3 = new SolidColorBrush(Color.FromRgb(80, 160, 80)),
      _b4 = new SolidColorBrush(Color.FromRgb(180, 80, 80)),
      _b5 = new SolidColorBrush(Color.FromRgb(200, 200, 011));

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is int v)
      {
        return v < 75 ? _b0 : v < 80 ? _b1 : v < 85 ? _b2 : v < 90 ? _b3 : v < 100 ? _b4 : _b5;
      }
      else if (value is string val)
      {
        switch (val)
        {
          case "offline":
            if (targetType == typeof(Brush)) return Brushes.White;
            else if (targetType == typeof(bool)) return false;
            else if (targetType == typeof(Visibility)) return Visibility.Collapsed;
            else return null;
          case "online":
            if (targetType == typeof(Brush)) return new LinearGradientBrush(Colors.Green, Colors.LightGreen, 0);//new BrushConverter().ConvertFromString("#00ff00");//
            else if (targetType == typeof(bool)) return true;
            else if (targetType == typeof(Visibility)) return Visibility.Visible;
            else return null;
          case "DND":
            if (targetType == typeof(Brush)) return new LinearGradientBrush(new GradientStopCollection() { new GradientStop(Colors.Red, 0.0), new GradientStop(Colors.Red, 0.44), new GradientStop(Colors.White, 0.45), new GradientStop(Colors.White, 0.55), new GradientStop(Colors.Red, 0.56), new GradientStop(Colors.Red, 1.0) }, 90);//
            else if (targetType == typeof(bool)) return true;
            else if (targetType == typeof(Visibility)) return Visibility.Visible;
            else return null;
          case "page-me":
            if (targetType == typeof(Brush)) return new BrushConverter().ConvertFromString("#ff8000");//new LinearGradientBrush(Colors.Green, Colors.LightGreen, 0);//
            else if (targetType == typeof(bool)) return true;
            else if (targetType == typeof(Visibility)) return Visibility.Visible;
            else return null;
          case "sign-in":
          case "sign-out":
          case "change-state":
          case "state":
          case "disconnecting":
          case "shutting-down":
          case "acquiring-network":
          case "connecting":
          case "synchronizing":
          case "no-network":
          case "no-service":
          case "quit":
          case "terminate": return Brushes.Black;
          default:
            if (targetType == typeof(Visibility)) return (string.IsNullOrEmpty(val) && IsInverted) ? Visibility.Visible : Visibility.Collapsed;
            else if (targetType == typeof(Brush)) return Brushes.Gray;
            else return Brushes.Gray;
        }
      }
      else if (value is bool bVal)
      {
        if (targetType == typeof(bool)) return bVal;
        else if (targetType == typeof(Brush))
          return bVal ? new LinearGradientBrush(Colors.Green, Colors.LightGreen, 0) : new LinearGradientBrush(Colors.DarkRed, Colors.Red, 0);//new BrushConverter().ConvertFromString("#00ff00");
        else if (targetType == typeof(Visibility))
          return bVal ? (IsInverted ? Visibility.Collapsed : Visibility.Visible) : (IsInverted ? Visibility.Visible : Visibility.Collapsed);
        else if (targetType == typeof(FontWeight))
          return bVal ? (IsInverted ? FontWeights.Normal : FontWeights.Bold) : (IsInverted ? FontWeights.Bold : FontWeights.Normal);
        else return null;
      }
      else if (value is DateTimeOffset offset)
      {
        //if (targetType == typeof(DateTimeOffset))
        {
          var va = offset;
          var dt = DateTimeOffset.Now - va;
          return
            dt.TotalSeconds < 10 ? $"Now!!!" :
            dt.TotalMinutes < 60 ? $"{va:HH:mm:ss}" :
            dt.TotalHours < 10.0 ? $"{va:HH:mm}" :
            dt.TotalDays < 3.000 ? $"{va:ddd HH:mm}" :
            dt.TotalDays < 183.0 ? $"{va:MMM-d}" :
            $"{va:yyyy-MMM}";
        }
        //else return null;
      }
      else if (value != null)
      {
        Debug.WriteLine($"**** Unprocesssed type: {value.GetType().Name} ");
        return value;
      }

      return null;

      //Unreachable code:
      //if (targetType == typeof(Visibility) && value == null) return _IsInverted ? Visibility.Visible : Visibility.Collapsed;
      //return new LinearGradientBrush(Colors.Gray, Colors.DarkGray, 45);//		return new BrushConverter().ConvertFromString("#ff0000");
    }
    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public ConverterUni() { }
  }
}
