using AAV.Sys.Helpers;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace AAV.WPF.Converters
{
  public class ConverterVisibleCollapsed : MarkupExtension, IValueConverter
  {
    public bool IsInverted { get; set; }
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (targetType != typeof(Visibility))
        return value;

      if (value == null)
        return IsInverted ? Visibility.Visible : Visibility.Collapsed;

      if (value.GetType() == typeof(bool))
      {
        return (bool)value
          ? IsInverted ? Visibility.Collapsed : Visibility.Visible
          : IsInverted ? Visibility.Visible : Visibility.Collapsed;
      }
      else if (value.GetType() == typeof(string))
      {
        return !string.IsNullOrWhiteSpace((string)value)
          ? IsInverted ? Visibility.Collapsed : Visibility.Visible
          : IsInverted ? Visibility.Visible : Visibility.Collapsed;
      }
      else // funny, eh?
      {
        return IsInverted ? Visibility.Collapsed : Visibility.Visible;
      }
    }
    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public ConverterVisibleCollapsed() { }
  }
  public class TimeAgoConverter : MarkupExtension, IValueConverter
  {
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (targetType != typeof(string) || value?.GetType() != typeof(DateTimeOffset))
        return value;

      return VerHelper.TimeAgo((DateTimeOffset)value);
    }
    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public TimeAgoConverter() { }
  }
  public class ElapsedConverter : MarkupExtension, IValueConverter
  {
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (targetType != typeof(string) || value?.GetType() != typeof(TimeSpan))
        return value;

      return VerHelper.Elapsed((TimeSpan)value);
    }
    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public ElapsedConverter() { }
  }
  public class EtaInConverter : MarkupExtension, IValueConverter
  {
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (targetType != typeof(string) || value?.GetType() != typeof(DateTimeOffset))
        return value;

      return VerHelper.EtaIn((DateTimeOffset)value);
    }
    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public EtaInConverter() { }
  }
  public class EtaAtConverter : MarkupExtension, IValueConverter
  {
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (targetType != typeof(string) || value?.GetType() != typeof(DateTimeOffset))
        return value;

      return VerHelper.EtaAt((DateTimeOffset)value);
    }
    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value;
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
    public EtaAtConverter() { }
  }
}
