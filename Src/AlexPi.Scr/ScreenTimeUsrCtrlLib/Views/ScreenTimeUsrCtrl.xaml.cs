using WpfUserControlLib.Base;

namespace ScreenTimeUsrCtrlLib.Views;

public partial class ScreenTimeUsrCtrl
{
  int _daysback = 2;
  Bpr _bpr = new Bpr();

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
      await Task.Delay(11); // time to show up.
      ctrlpnl.Visibility = Visibility.Collapsed;
      _bpr.Click();
      //tmi: tbInfo.Text = $"► going {daysBack} days back...";      //var sw = Stopwatch.StartNew();

      spArrayHolder.Children.Clear();
      var eois =new  EvLogHelper().GetAllUpDnEvents(DateTime.Today.AddDays(-daysBack), DateTime.Today.AddDays(.9999999));
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
      _bpr.Tick();
      await Task.Delay(100);
    }
  }
  public void RedrawOnResize(object s, RoutedEventArgs e) { }//foreach (var uc in spArrayHolder.Children) if (uc is DailyChart) ((DailyChart)uc).clearDrawAllSegmentsForAllPCsAsync(s, e); }

  public static readonly DependencyProperty ZVProperty = DependencyProperty.Register("ZV", typeof(double), typeof(ScreenTimeUsrCtrl), new PropertyMetadata(1.5)); public double ZV { get => (double)GetValue(ZVProperty); set => SetValue(ZVProperty, value); }
}