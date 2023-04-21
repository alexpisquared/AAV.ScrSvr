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
      ctrlpnl.Visibility = Visibility.Collapsed;
      Bpr.Beep1of2();
      //tmi: tbInfo.Text = $"► going {daysBack} days back...";      //var sw = Stopwatch.StartNew();

      spArrayHolder.Children.Clear();
      var eois = EvLogHelper.GetAllUpDnEvents(DateTime.Today.AddDays(-daysBack), DateTime.Today.AddDays(.9999999));
      for (var i = 0; i < daysBack; i++)
      {
        var day_i = DateTime.Today.AddDays(-i);
        var sortedEois = new SortedList<DateTime, int>();
        eois.Where(r => day_i < r.Key && r.Key < day_i.AddDays(.9999999)).ToList().ForEach(r => sortedEois.Add(r.Key, r.Value));
        if (sortedEois.Count > 0)
        {
          _ = spArrayHolder.Children.Add(new DailyChart(day_i, sortedEois));
        }
      }

      //tmi: tbInfo.Text = $"► {sw.Elapsed:mm\\:ss} ► {1000d * daysBack / sw.ElapsedMilliseconds:N1} day/sec.";
    }
    finally
    {
      ctrlpnl.Visibility = Visibility.Visible;
      await Task.Delay(100);
      Bpr.Beep2of2();
    }
  }
  public void RedrawOnResize(object s, RoutedEventArgs e) { }//foreach (var uc in spArrayHolder.Children) if (uc is DailyChart) ((DailyChart)uc).clearDrawAllSegmentsForAllPCsAsync(s, e); }
}
