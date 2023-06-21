using System.Drawing;
using System.Windows.Controls;

namespace WpfApp1;

public partial class MainWindow
{
  public MainWindow()
  {
    InitializeComponent();
    DataContext = string.IsNullOrEmpty(Settings.Default.VM) ? new VM() : JsonSerializer.Deserialize<VM>(Settings.Default.VM);
    canvas.Width = Width;
    canvas.Height = Height;
  }
  void Window_Closing(object sender, CancelEventArgs e)
  {
    Settings.Default.VM = JsonSerializer.Serialize(DataContext);
    Settings.Default.Save();
  }

  void OnClose(object sender, RoutedEventArgs e) => Close();
  void OnPrimScreens(object sender, RoutedEventArgs e) => StretchToFill(this, WinFormHelper.PrimaryScreen.Bounds);
  void OnScndScreens(object sender, RoutedEventArgs e) => StretchToFill(this, WinFormHelper.SecondaryScreen.Bounds);
  void OnBothScreens(object sender, RoutedEventArgs e) => StretchToFill(this, WinFormHelper.GetSumOfAllBounds);

  void StretchToFill(Window window, Rectangle rectangle)
  {
    window.WindowStartupLocation = WindowStartupLocation.Manual;
    window.Top = rectangle.Top;
    window.Left = rectangle.Left;
    window.Width = canvas.Width = rectangle.Width;
    window.Height = canvas.Height = rectangle.Height;
  }
}