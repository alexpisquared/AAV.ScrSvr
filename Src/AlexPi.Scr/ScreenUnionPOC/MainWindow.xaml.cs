namespace ScreenUnionPOC;

public partial class MainWindow
{
  public MainWindow()
  {
    InitializeComponent();

    try
    {
      //DataContext = string.IsNullOrEmpty(Settings.Default.VM) ? new VM() : JsonSerializer.Deserialize<VM>(Settings.Default.VM);
      //DataContext = new VM();
    }
    catch (Exception ex)
    {
      Trace.WriteLine(Title = $"{ex.Message}:  {Settings.Default.VM}");
      //DataContext = new VM();
    }
    canvas.Width = ScreenHelper.GetSumOfAllBounds.Width;
    canvas.Height = ScreenHelper.GetSumOfAllBounds.Height;
    Trace.WriteLine(Title = $"{Environment.MachineName}:   canvas.Width: {canvas.Width}, canvas.Height: {canvas.Height}");
  }

  void OnDragMove(object sender, MouseButtonEventArgs e)
  {
    if (e.LeftButton != MouseButtonState.Pressed) return;

    DragMove();
    e.Handled = true;
  }

  void Window_Closing(object sender, CancelEventArgs e)
  {
    //Settings.Default.VM = JsonSerializer.Serialize(DataContext);
    //Settings.Default.Save();
  }

  void OnClose(object sender, RoutedEventArgs e) => Close();
  void OnPrimScreens(object sender, RoutedEventArgs e) => StretchToFill(this, ScreenHelper.PrimaryScreen.Bounds);
  void OnScndScreens(object sender, RoutedEventArgs e) => StretchToFill(this, ScreenHelper.SecondaryScreen.Bounds);
  void OnBothScreens(object sender, RoutedEventArgs e) => StretchToFill(this, ScreenHelper.GetSumOfAllBounds);

  void StretchToFill(Window window, Rectangle rectangle)
  {
    window.WindowState = WindowState.Normal;
    var k = Environment.MachineName == "LR6WV43X" ? .6666666 : 1;
    window.Top = rectangle.Top * k;
    window.Left = rectangle.Left * k;
    window.Width = rectangle.Width * k;
    window.Height = rectangle.Height * k;
  }
}