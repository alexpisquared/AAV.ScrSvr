using WinFormsControlLib;

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
    themeSelector1.SetCurThemeToMenu(Thm); //dnf theming 2/2

    //_logger.LogInformation($"mVl{(DateTime.Now - _mvwStarted).TotalSeconds,4:N1}s  {VersionHelper.DevDbgAudit(_config)}");
  }

  void OnWindowRestoree(object s, RoutedEventArgs e) { wr.Visibility = Visibility.Collapsed; wm.Visibility = Visibility.Visible; WindowState = WindowState.Normal; }
  void OnWindowMaximize(object s, RoutedEventArgs e) { wm.Visibility = Visibility.Collapsed; wr.Visibility = Visibility.Visible; WindowState = WindowState.Maximized; }

  async void OnSave(object s, RoutedEventArgs e) => await Task.Yield();
  async void OnAudio(object s, RoutedEventArgs e) => await Task.Yield();
  async void OnFlush(object s, RoutedEventArgs e) => await Task.Yield();
  async void OnSettings(object s, RoutedEventArgs e) { await Task.Yield(); _ = MessageBox.Show("Under Construction...", "Under Construction...", MessageBoxButton.OK, MessageBoxImage.Information); }

  void OnRequestNavigate(object s, System.Windows.Navigation.RequestNavigateEventArgs e)
  {
    e.Handled = true;
    if (s.GetType() != typeof(Hyperlink)) return;

    if (Directory.Exists(e.Uri.ToString()))
      _ = Process.Start("Explorer.exe", e.Uri.ToString());
    else
      _ = MessageBox.Show($"Directory  \n\n   {e.Uri}\n\ndoes not exist...\n\n...or is unaccessible at the moment ", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
  }

  void OnGoToLink(object s, RoutedEventArgs e) => _ = Process.Start("Explorer.exe", ((MenuItem)s)?.Tag?.ToString() ?? "C:\\");
  void OnUnchecked(object s, RoutedEventArgs e) => ((CheckBox)s).IsChecked = true;
  void OnWindowMinimize(object sender, RoutedEventArgs e)  { WindowState = WindowState.Minimized; }
  void OnExit(object sender, RoutedEventArgs e)  {    Close();  }

  void OnDragMove(object sender, MouseButtonEventArgs e)
  {
    if (e.LeftButton != MouseButtonState.Pressed) return;

    DragMove();
    e.Handled = true;
  }

  void OnPrimScreens(object sender, RoutedEventArgs e) => StretchToFill(this, WinFormHelper.PrimaryScreen.Bounds);
  void OnScndScreens(object sender, RoutedEventArgs e) => StretchToFill(this, WinFormHelper.SecondaryScreen.Bounds);
  void OnBothScreens(object sender, RoutedEventArgs e) => StretchToFill(this, WinFormHelper.GetSumOfAllBounds);

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