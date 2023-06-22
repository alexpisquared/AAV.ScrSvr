namespace WpfApp1;

public partial class MainWindow
{
  public MainWindow()
  {
    InitializeComponent();

    try
    {
      DataContext = string.IsNullOrEmpty(Settings.Default.VM) ? new VM() : JsonSerializer.Deserialize<VM>(Settings.Default.VM);
      //DataContext = new VM();
    }
    catch (Exception ex)
    {
      Trace.WriteLine(Title = $"{ex.Message}:  {Settings.Default.VM}");
      DataContext = new VM();
    }
    canvas.Width = WinFormHelper.GetSumOfAllBounds.Width;
    canvas.Height = WinFormHelper.GetSumOfAllBounds.Height;
    Trace.WriteLine(Title = $"{Environment.MachineName}:   canvas.Width: {canvas.Width}, canvas.Height: {canvas.Height}");
  }

  void OnDragMove(object sender, MouseButtonEventArgs e)
  {
    if (e.LeftButton != MouseButtonState.Pressed) return;
    DragMove();
    //handled = true;
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
    window.Width = rectangle.Width;
    window.Height = rectangle.Height;
  }
}