namespace AlexPi.Scr.Vws;

public partial class BackgroundWindow : UnCloseableWindow
{
  public BackgroundWindow() : base(null) { }
  public BackgroundWindow(GlobalEventHandler globalEventHandler) : base(globalEventHandler)
  {
    InitializeComponent();
    DataContext = this;
  }
  public void ShowOnTargetScreen(System.Windows.Forms.Screen trgScreen, bool showMaximized)
  {
    Show();

    Title = $"{trgScreen.DeviceName} - {(trgScreen.Primary ? "Primary  " : "Secondary")}     {trgScreen.Bounds.Width,5}x{trgScreen.Bounds.Height,-5} @ {trgScreen.Bounds.X,5},{trgScreen.Bounds.Y,-5}     {VersionHelper.CurVerStr("")}"; // always NaN / 0: ► Left-Top: {(double.IsNaN(window.Left) ? -1.0 .Left)}-{(double.IsNaN(window.Top) ? -1.0 .Top)}   Actual W x H: {window.ActualWidth}x{window.ActualHeight}" +

    if (showMaximized)
    {
      WindowState = WindowState.Maximized;
      Topmost = true;
      Background = Brushes.Black;
    }
    else
    {
      var k = DevOps.IsDevMachineH ? 1 : .666666666;
      WindowState = WindowState.Normal;
      Topmost = false;
      Width = trgScreen.Bounds.Width * k;
      Left = trgScreen.Bounds.Left * k;
      Top = trgScreen.Bounds.Top * k + 40; // task bar considerations.
    }
  }

  void onShow(object s, MouseEventArgs e) { try { ((Storyboard)FindResource("FadingOut")).Begin(); } catch (Exception ex) { tbk1.Text = ex.Message; } }
}