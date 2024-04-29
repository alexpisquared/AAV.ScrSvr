using System.Speech.Recognition;
using AmbienceLib;

namespace OleksaScrSvr;

public partial class MainNavView : WindowBase
{
  public StandardContractsLib.IBpr Bpr { get; }
  public SpeechSynth Speech { get; }

  public MainNavView(ILogger logger, IConfigurationRoot config, StandardContractsLib.IBpr bpr, SpeechSynth speech) : this((ILogger<Window>)logger, config, bpr, speech) { }
  public MainNavView(ILogger<Window> logger, IConfigurationRoot config, StandardContractsLib.IBpr bpr, SpeechSynth speech) : base(logger)
  {
    InitializeComponent(); KeepOpenReason = "";

    //tu: ..if needed!!! MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight; // MaximumWindowTrackHeight; // 2021-11: is it for not overlapping the taskbar?

    Bpr = bpr;
    Speech = speech;
    themeSelector1.ThemeApplier = ApplyTheme; //dnf theming 1/2
    //if (Debugger.IsAttached) Topmost = true;

    DragMovable = false;
    IgnoreEscape = false;

    btnExit.IsCancel = VersionHelper.IsDbg;
  }

  async void OnLoaded(object s, RoutedEventArgs e)
  {
    themeSelector1.SetCurThemeToMenu(Thm); //dnf theming 2/2//_logger.LogInformation($"mVl{(DateTime.Now - _mvwStarted).TotalSeconds,4:N1}s  {VersionHelper.DevDbgAudit(_config)}");

    //Speech.SpeakFAF("Loading...");

    var max = 16;

    if (DevOps.IsDbg)
    {
      //for (int i = 1; i <= max; i++) { await Bpr.BeepAsync(100 * i, 0.333); Opacity = (double)i / max; }
      return;
    }

    StretchToFill(this, Environment.MachineName == "ASUS2" ? ScreenHelper.PrimaryScreen.Bounds : ScreenHelper.GetSumOfAllBounds);
    Topmost = Environment.CommandLine.Contains("Topmost");

    //for (int i = 1; i <= max; i++) { await Task.Delay(60_000); Opacity = (double)i / max; }
  }

  void OnWindowRestoree(object s, RoutedEventArgs e) { wr.Visibility = Visibility.Collapsed; wm.Visibility = Visibility.Visible; WindowState = WindowState.Normal; }
  void OnWindowMaximize(object s, RoutedEventArgs e) { wm.Visibility = Visibility.Collapsed; wr.Visibility = Visibility.Visible; WindowState = WindowState.Maximized; }
  void OnWindowMinimize(object s, RoutedEventArgs e) => WindowState = WindowState.Minimized;
  void OnExit(object s, RoutedEventArgs e) => Close();

  void OnPrimScreens(object s, RoutedEventArgs e) => StretchToFill(this, ScreenHelper.PrimaryScreen.Bounds);
  void OnScndScreens(object s, RoutedEventArgs e) => StretchToFill(this, ScreenHelper.SecondaryScreen.Bounds);
  void OnBothScreens(object s, RoutedEventArgs e) => StretchToFill(this, ScreenHelper.GetSumOfAllBounds);

  static void StretchToFill(Window window, Rectangle rectangle, int margin = 2)
  {
    window.WindowState = WindowState.Normal;
    var k = Environment.MachineName == "LR6WV43X" ? .6666666 : 1;
    window.Top = rectangle.Top * k - margin;
    window.Left = rectangle.Left * k - margin;
    window.Width = rectangle.Width * k + margin * 2;
    window.Height = rectangle.Height * k + margin * 2;
  }
}