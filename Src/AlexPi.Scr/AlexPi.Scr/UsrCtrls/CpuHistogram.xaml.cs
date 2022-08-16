using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GaugeUserControlLibrary
{
  public partial class CpuHistogram : UserControl
  {
    //987 PerformanceCounter _perfCounCPU = new PerformanceCounter("Processor", "% Processor Time", "_Total");
    //777 Process myProcess = Process.GetCurrentProcess();
#if DEBUG
    readonly double _minute = .5, _dbg = 100;
#else
    readonly double _minute = 5, _dbg = 0;
#endif
    public CpuHistogram()
    {
      InitializeComponent();
      sp1.Children.Clear();

      for (var i = 0; i < _dbg; i++) sp1.Children.Add(new Rectangle());

      var red = new SolidColorBrush(Colors.Red);
      new DispatcherTimer(TimeSpan.FromMinutes(_minute), DispatcherPriority.Background, new EventHandler((s, e) => sp1.Children.Add(new Rectangle { Fill = red, Height = 100 })), Dispatcher.CurrentDispatcher).Start(); //tu: one-line timer

      //987 new DispatcherTimer(TimeSpan.FromSeconds(_second), DispatcherPriority.Background, new EventHandler((s, e) => measure_Cpu(_perfCounCPU.NextValue())), Dispatcher.CurrentDispatcher).Start(); //tu: one-line timer
    }

    double _avg; public double Avg
    {
      get => _avg; internal set
      {
        brAvg.Height = Math.Abs(50 * Math.Log10(_avg = value)); // t1.Text = $"{value:N1} ==> {brAvg.Height:N1}";
      }
    }

    public void Measure_Cpu(float percCpuLoad)
    {
      dov(sp1, percCpuLoad);
      t0.Text =
        percCpuLoad < 10 ?
        $"CPU    {percCpuLoad,5:N1}  %" :
        $"CPU    {percCpuLoad,5:N0}  %";

      //777 myProcess.Refresh(); // Refresh the current process property values.
      //777 if (AppSettings.Instance.PeakWorkingSet64 < myProcess.PeakWorkingSet64 ||
      //777     AppSettings.Instance.PeakPagMemSize64 < myProcess.PeakPagedMemorySize64 ||
      //777     AppSettings.Instance.PeakVirMemSize64 < myProcess.PagedSystemMemorySize64)
      //777 {
      //777   AppSettings.Instance.PeakWorkingSet64 = myProcess.PeakWorkingSet64;
      //777   AppSettings.Instance.PeakPagMemSize64 = myProcess.PeakPagedMemorySize64;
      //777   AppSettings.Instance.PeakVirMemSize64 = myProcess.PagedSystemMemorySize64;
      //777   AppSettings.Instance.Save();
      //777 }

      // double[] da = new double[4];
      //dov(c2, da[1] = 100.000 * myProcess.WorkingSet64              /**/ / AppSettings.Instance.PeakWorkingSet64);
      //dov(c3, da[2] = 100.000 * myProcess.PagedMemorySize64         /**/ / AppSettings.Instance.PeakPagMemSize64);
      //dov(c4, da[3] = 100.000 * myProcess.PagedSystemMemorySize64   /**/ / AppSettings.Instance.PeakVirMemSize64);

      //t1.Text = $"Work Set: \t{da[1],5:N0}   %    /     {AppSettings.Instance.PeakWorkingSet64,10:e2}";
      //t2.Text = $"PagedMem: \t{da[2],5:N0}   %    /     {AppSettings.Instance.PeakPagMemSize64,10:e2}";
      //t3.Text = $"PagedSys: \t{da[3],5:N0}   %    /     {AppSettings.Instance.PeakVirMemSize64,10:e2}";
    }

    void dov(StackPanel stkPnl, double percCpuLoad)
    {
      if (stkPnl.Children.Count > ActualWidth) stkPnl.Children.RemoveAt(0); // measure per pixel ...plus scaling...

      stkPnl.Children.Add(new Rectangle { Height = Math.Abs(50 * Math.Log10(percCpuLoad) - percCpuLoad), Margin = new Thickness(0, 0, 0, percCpuLoad) });

      //Debug.WriteLine($"*** WxH: {ActualWidth}x{stkPnl.ActualHeight}   v={percCpuLoad,3:N0}");
    }
  }
}
