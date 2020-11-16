using System.Windows;
using System.Windows.Controls;

namespace AAV.WPF.View
{
  public partial class ThemeToggleIress : UserControl
  {
    public ThemeToggleIress() => InitializeComponent();

    public delegate void ApplyThemeDelegate(string v);
    public ApplyThemeDelegate? ApplyTheme { get; set; }

    public static readonly DependencyProperty CurThemeProperty = DependencyProperty.Register("CurTheme", typeof(string), typeof(ThemeToggleIress), new PropertyMetadata(null)); public string CurTheme { get => (string)GetValue(CurThemeProperty); set => SetValue(CurThemeProperty, value); }

    void onToggle(object s, RoutedEventArgs e)
    {
      CurTheme = (CurTheme == "Lite.Iress" ? "Dark.Iress" : "Lite.Iress");
      ApplyTheme?.Invoke(CurTheme);

      ((Button)s).Content = (CurTheme == "Lite.Iress" ? "Dark" : "Light");
    }
  }
}
