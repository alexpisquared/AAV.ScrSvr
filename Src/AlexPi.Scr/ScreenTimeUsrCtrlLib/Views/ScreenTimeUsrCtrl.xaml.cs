using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Db.EventLog.DbModel;
using UpTimeChart;

namespace ScreenTimeUsrCtrlLib.Views
{
  public partial class ScreenTimeUsrCtrl : UserControl
  {
    int _daysback = 2;

    public int DaysBack { get => _daysback; set => _daysback = value; }
    public int DelaySec { get; set; } = 15;

    public ScreenTimeUsrCtrl()
    {
      InitializeComponent();
      if (!DesignerProperties.GetIsInDesignMode(this)) //tu: design time mode
        Loaded += onLoaded;
    }
    async void onLoaded(object s, RoutedEventArgs e) { await Task.Delay(DelaySec * 1000); onDrawDays(s, e); }        //void onLoaded(object s, RoutedEventArgs e) { Task.Run(async () => await Task.Delay(DelaySec * 1000)).ContinueWith(_ => onXX(s, e), TaskScheduler.FromCurrentSynchronizationContext()); }
    async void onDrawDays(object s, RoutedEventArgs e) => await repopulateUsrCtrlCharts(((FrameworkElement)s).Tag?.ToString() ?? _daysback.ToString());
    async Task repopulateUsrCtrlCharts(string str) => await repopulateUsrCtrlCharts(int.TryParse(str, out _daysback) ? _daysback : 21);
    async Task repopulateUsrCtrlCharts(int daysBack)
    {
      var sw = Stopwatch.StartNew();
      tbInfo.Text = $"► going {daysBack} days back..."; await Task.Delay(100);

      var earliestRecordedDate = new DateTime(2017, 10, 20);
      if (DateTime.Today.AddDays(-daysBack) < earliestRecordedDate)
        daysBack = (int)(DateTime.Today - earliestRecordedDate).TotalDays;

      spArrayHolder.Children.Clear(); for (int i = 0; i < daysBack; i++) { spArrayHolder.Children.Add(new DailyChart(DateTime.Today.AddDays(-i))); await Task.Delay(100); }

      tbInfo.Text = $"► {sw.Elapsed:mm\\:ss} ► {1000d * daysBack / sw.ElapsedMilliseconds:N1} day/sec.";
    }
    public void RedrawOnResize(object s, RoutedEventArgs e) { }//foreach (var uc in spArrayHolder.Children) if (uc is DailyChart) ((DailyChart)uc).clearDrawAllSegmentsForAllPCsAsync(s, e); }
    public async void ReloadThisPC(PcLogic pc) { foreach (var uc in spArrayHolder.Children) if (uc is DailyChart) { await Task.Yield(); ((DailyChart)uc).ClearDrawAllSegmentsForSinglePC(pc.MachineName, pc.ColorRGB); } }
  }
}
