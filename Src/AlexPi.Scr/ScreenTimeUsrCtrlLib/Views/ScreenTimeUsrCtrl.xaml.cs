using AAV.Sys.Helpers;

namespace ScreenTimeUsrCtrlLib.Views;

public partial class ScreenTimeUsrCtrl : UserControl
{
  int _daysback = 2;

  public int DaysBack { get => _daysback; set => _daysback = value; }
  public int DelaySec { get; set; } = 15;

  public ScreenTimeUsrCtrl()
  {
    InitializeComponent();
    if (!DesignerProperties.GetIsInDesignMode(this)) //tu: design time mode
      Loaded += onDrawDays;
  }
  async void onDrawDays(object s, RoutedEventArgs e) => await repopulateUsrCtrlCharts(((FrameworkElement)s).Tag?.ToString() ?? _daysback.ToString());
  async Task repopulateUsrCtrlCharts(string str) => await repopulateUsrCtrlCharts(int.TryParse(str, out _daysback) ? _daysback : 21);
  async Task repopulateUsrCtrlCharts(int daysBack)
  {
    try
    {
      Bpr.Begin2FaF();

      ctrlpnl.Visibility = Visibility.Collapsed;
      var sw = Stopwatch.StartNew();
      tbInfo.Text = $"► going {daysBack} days back...";

      var earliestRecordedDate = new DateTime(2017, 10, 20);
      if (DateTime.Today.AddDays(-daysBack) < earliestRecordedDate)
        daysBack = (int)(DateTime.Today - earliestRecordedDate).TotalDays;

      spArrayHolder.Children.Clear(); for (var i = 0; i < daysBack; i++) { _ = spArrayHolder.Children.Add(new DailyChart(DateTime.Today.AddDays(-i))); await Task.Delay(1); }

      tbInfo.Text = $"► {sw.Elapsed:mm\\:ss} ► {1000d * daysBack / sw.ElapsedMilliseconds:N1} day/sec.";

      Bpr.BeepEnd2();
    }
    finally
    {
      ctrlpnl.Visibility = Visibility.Visible;
    }
  }
  public void RedrawOnResize(object s, RoutedEventArgs e) { }//foreach (var uc in spArrayHolder.Children) if (uc is DailyChart) ((DailyChart)uc).clearDrawAllSegmentsForAllPCsAsync(s, e); }
  public async void ReloadThisPC(PcLogic pc) { foreach (var uc in spArrayHolder.Children) if (uc is DailyChart) { await Task.Yield(); ((DailyChart)uc).ClearDrawAllSegmentsForSinglePC(pc.MachineName); } }
}
