﻿using System.Windows;

namespace OleksaScrSvr;

public partial class MainNavView : WindowBase
{
  public StandardContractsLib.IBpr Bpr { get; }

  public MainNavView(ILogger logger, IConfigurationRoot config, StandardContractsLib.IBpr bpr) : this((ILogger<Window>)logger, config, bpr) { }
  public MainNavView(ILogger<Window> logger, IConfigurationRoot config, StandardContractsLib.IBpr bpr) : base(logger)
  {
    InitializeComponent(); KeepOpenReason = "";

    //tu: ..if needed!!! MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight; // MaximumWindowTrackHeight; // 2021-11: is it for not overlapping the taskbar?

    Bpr = bpr;
    themeSelector1.ThemeApplier = ApplyTheme; //dnf theming 1/2
    Topmost = Debugger.IsAttached;

    DragMovable = false;

    btnExit.IsCancel = VersionHelper.IsDbg;
  }

  void OnLoaded(object s, RoutedEventArgs e)
  {
    themeSelector1.SetCurThemeToMenu(Thm); //dnf theming 2/2//_logger.LogInformation($"mVl{(DateTime.Now - _mvwStarted).TotalSeconds,4:N1}s  {VersionHelper.DevDbgAudit(_config)}");

    if (!VersionHelper.IsDbg)
      StretchToFill(this, WinFormHelper.PrimaryScreen.Bounds); // good for Yoga, Nuc, Razer.
  }

  void OnWindowRestoree(object s, RoutedEventArgs e) { wr.Visibility = Visibility.Collapsed; wm.Visibility = Visibility.Visible; WindowState = WindowState.Normal; }
  void OnWindowMaximize(object s, RoutedEventArgs e) { wm.Visibility = Visibility.Collapsed; wr.Visibility = Visibility.Visible; WindowState = WindowState.Maximized; }
  void OnWindowMinimize(object s, RoutedEventArgs e) => WindowState = WindowState.Minimized;
  void OnExit(object s, RoutedEventArgs e) => Close();

  void OnPrimScreens(object s, RoutedEventArgs e) => StretchToFill(this, WinFormHelper.PrimaryScreen.Bounds);
  void OnScndScreens(object s, RoutedEventArgs e) => StretchToFill(this, WinFormHelper.SecondaryScreen.Bounds);
  void OnBothScreens(object s, RoutedEventArgs e) => StretchToFill(this, WinFormHelper.GetSumOfAllBounds);

  static void StretchToFill(Window window, Rectangle rectangle, int margin = 16)
  {
    window.WindowState = WindowState.Normal;
    var k = Environment.MachineName == "LR6WV43X" ? .6666666 : 1;
    window.Top = rectangle.Top * k - margin;
    window.Left = rectangle.Left * k - margin;
    window.Width = rectangle.Width * k + margin * 2;
    window.Height = rectangle.Height * k + margin * 2;
  }
}