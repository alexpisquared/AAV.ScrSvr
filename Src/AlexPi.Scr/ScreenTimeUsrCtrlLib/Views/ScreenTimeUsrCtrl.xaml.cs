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
      var sw = Stopwatch.StartNew();
      tbInfo.Text = $"► going {daysBack} days back...";

      var earliestRecordedDate = new DateTime(2017, 10, 20);
      if (DateTime.Today.AddDays(-daysBack) < earliestRecordedDate)
        daysBack = (int)(DateTime.Today - earliestRecordedDate).TotalDays;
            
      var eois = EvLogHelper.GetAllUpDnEvents(DateTime.Today.AddDays(-daysBack), DateTime.Today.AddDays(.9999999));

      spArrayHolder.Children.Clear(); 
      for (var i = 0; i < daysBack; i++)
      {
        var thisDay = DateTime.Today.AddDays(-i);
        var sortedList = new SortedList<DateTime, int>();

        eois.Where(r => thisDay < r.Key && r.Key < thisDay.AddDays(.9999999)).ToList().ForEach(r => sortedList.Add(r.Key, r.Value));

        _ = spArrayHolder.Children.Add(new DailyChart(thisDay, sortedList)); 
        //await Task.Delay(1000 / daysBack);
      }

      tbInfo.Text = $"► {sw.Elapsed:mm\\:ss} ► {1000d * daysBack / sw.ElapsedMilliseconds:N1} day/sec.";
    }
    finally
    {
      ctrlpnl.Visibility = Visibility.Visible;
      Bpr.BeepClk();
      await Task.Delay(1);
    }
  }
  public void RedrawOnResize(object s, RoutedEventArgs e) { }//foreach (var uc in spArrayHolder.Children) if (uc is DailyChart) ((DailyChart)uc).clearDrawAllSegmentsForAllPCsAsync(s, e); }
}
