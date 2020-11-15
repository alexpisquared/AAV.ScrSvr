using System;
using System.Windows;
using System.Windows.Controls;

namespace AAV.WPF.View
{
  public partial class ThemeSelectorUsrCtrl : UserControl
  {
    public ThemeSelectorUsrCtrl() => InitializeComponent();

    public delegate void ApplyThemeDelegate(string v);
    public ApplyThemeDelegate? ApplyTheme { get; set; }

    public void SetCurTheme(string theme)
    {
      foreach (MenuItem? item in ((ItemsControl)menu1.Items[0]).Items)
        if (item != null)
          item.IsChecked = theme?.Equals(item.Tag.ToString(), StringComparison.OrdinalIgnoreCase) ?? false;
    }
    public static readonly DependencyProperty CurThemeProperty = DependencyProperty.Register("CurTheme", typeof(string), typeof(ThemeSelectorUsrCtrl), new PropertyMetadata(null)); public string CurTheme { get => (string)GetValue(CurThemeProperty); set => SetValue(CurThemeProperty, value); }

    void onChangeTheme(object s, RoutedEventArgs e) => ApplyTheme?.Invoke(((Button)s)?.Tag?.ToString() ?? "No Theme");
    void onSelectionChanged(object s, SelectionChangedEventArgs e)
    {
      if (e?.AddedItems?.Count > 0 && ApplyTheme != null)
        ApplyTheme(CurTheme = ((FrameworkElement)((object[])e.AddedItems)[0]).Tag?.ToString() ?? "No Theme?");
    }
    void onMenuClick(object s, RoutedEventArgs e)
    {
      ApplyTheme?.Invoke(CurTheme = ((FrameworkElement)s).Tag?.ToString() ?? "No Theme");

      SetCurTheme(CurTheme);
    }
  }
}
