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

    Title = $" {trgScreen.DeviceName} - {(trgScreen.Primary ? "Primary  " : "Secondary")}   XY: {trgScreen.Bounds.X,5} x {trgScreen.Bounds.Y,-5} \t {VersionHelper.CurVerStr("")}"; // always NaN / 0: ► Left-Top: {(double.IsNaN(window.Left) ? -1.0 .Left)}-{(double.IsNaN(window.Top) ? -1.0 .Top)}   Actual W x H: {window.ActualWidth}x{window.ActualHeight}" +

    if (showMaximized)
    {
      WindowState = WindowState.Maximized;
      Topmost = true;
      Background = Brushes.Black;
    }
    else
    {
      WindowState = WindowState.Normal;
      Topmost = false;
      Width = trgScreen.Bounds.Width;
      Left = trgScreen.Bounds.Left;
      Top = trgScreen.Bounds.Top + 40; // task bar considerations.
    }
  }

  void onShow(object s, MouseEventArgs e) { try { ((Storyboard)FindResource("FadingOut")).Begin(); } catch (Exception ex) { tbk1.Text = ex.Message; } }
}