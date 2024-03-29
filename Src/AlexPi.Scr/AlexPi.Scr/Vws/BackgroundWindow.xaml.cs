﻿namespace AlexPi.Scr.Vws;

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

    Title = $"{trgScreen.DeviceName} - {(trgScreen.Primary ? "Primary  " : "Secondary")}     {trgScreen.Bounds.Width,5}x{trgScreen.Bounds.Height,-5} @ {trgScreen.Bounds.X,5},{trgScreen.Bounds.Y,-5}     {string.Join('·', Environment.GetCommandLineArgs().Skip(1))}     {VersionHelper.CurVerStr}";
    var k = DevOps.IsDevMachineO ? .666666666 : 1;
    Left = trgScreen.Bounds.Left * k;
    Top = trgScreen.Bounds.Top * k + 40; // task bar considerations.

    if (showMaximized)
    {
      WindowState = WindowState.Maximized;
      Topmost = true;
    }
    else
    {
      WindowState = WindowState.Normal;
      Topmost = false;
      Width = trgScreen.Bounds.Width * k;
      Height = 260;
    }
  }

  void onShow(object s, MouseEventArgs e) { try { ((Storyboard)FindResource("FadingOut")).Begin(); } catch (Exception ex) { tbk1.Text = ex.Message; } }
}