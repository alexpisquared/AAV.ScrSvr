using AAV.Sys.Ext;
using AAV.WPF.Ext;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AlexPi.Scr.Vws
{
  public partial class ContainerB : TopmostUnCloseableWindow
  {
    PerformanceCounter
            //_perfCountAA = new PerformanceCounter("Memory", "Available MBytes"), // new PerformanceCounter(categoryName: "Process", counterName: "Private Bytes", instanceName: "AlexPi.Scr.scr"),
            //_perfCountEx = new PerformanceCounter(categoryName: "Process", counterName: "Private Bytes", instanceName: "Explorer"),
            //_perfCounGPU = new PerformanceCounter("GPU", "% GPU Time", "_Total"), // % GPU Time-Base - https://github.com/Alexey-Kamenev/GpuPerfCounters/blob/master/src/GpuPerfCounters/PerfCounterService.cs
            //_ramCounterr = new PerformanceCounter("Memory", "Available MBytes")
            _perfCounCPU;// = new PerformanceCounter("Processor", "% Processor Time", "_Total");
    double _sum;
    int _cnt;
    bool _alreadyRunning = false;

    public ContainerB(AlexPi.Scr.Logic.GlobalEventHandler globalEventHandler) : base(globalEventHandler)
    {
      InitializeComponent();

      //_perfCountAA = new PerformanceCounter("Memory", "Available MBytes"), // new PerformanceCounter(categoryName: "Process", counterName: "Private Bytes", instanceName: "AlexPi.Scr.scr"),
      //_perfCountEx = new PerformanceCounter(categoryName: "Process", counterName: "Private Bytes", instanceName: "Explorer"),
      ////_perfCounGPU = new PerformanceCounter("GPU", "% GPU Time", "_Total"), // % GPU Time-Base - https://github.com/Alexey-Kamenev/GpuPerfCounters/blob/master/src/GpuPerfCounters/PerfCounterService.cs
      //_ramCounterr = new PerformanceCounter("Memory", "Available MBytes");

      Task<PerformanceCounter>.Run(() => new PerformanceCounter("Processor", "% Processor Time", "_Total")).ContinueWith(_ => // new (...) takes forever => using Task to unfreeze (Aug 2019)
      {
        _perfCounCPU = _.Result;
        new DispatcherTimer(TimeSpan.FromSeconds(5), DispatcherPriority.Background, new EventHandler((s, e) => measure_Cpu_for_arrows_only()), Dispatcher.CurrentDispatcher).Start();
      }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    void measure_Cpu_for_arrows_only()
    {
      try
      {
        var k = 1.8;
        var p = _perfCounCPU.NextValue();
        var v = k * p - 90;

        try
        {
          gaugeTorCPU.InnerValAnim = (_sum += v) / (++_cnt);
        }
        catch (Exception ex) { ex.Log("1111"); }
        try
        {
          gaugeTorCPU.MiddlValAnim = v;
        }
        catch (Exception ex) { ex.Log("2222-Height made Abs()."); }
        try
        {

          if (gaugeTorCPU.OuterVal < v)
            gaugeTorCPU.OuterValAnim = v;
        }
        catch (Exception ex) { ex.Log("3333"); }
        try
        {
          gaugeTorCPU.GaugeText = $"{p:N0}\r\n{((gaugeTorCPU.InnerVal + 90) / k):N0} - {((gaugeTorCPU.OuterVal + 90) / k):N0}";
        }
        catch (Exception ex) { ex.Log("4444"); }


        if (p > 75 && !_alreadyRunning)
        {
          App.SpeakFaF($"Measured {p:N0} percent of CPU usage. Launching task manager...");
          _alreadyRunning = true;
          try { Process.Start(new ProcessStartInfo("TaskMgr.exe") { UseShellExecute = true }); } catch (Exception ex) { ex.Pop(); }
        }

        //todo: move to the proper UC:
        //p = _perfCountEx.NextValue() / 1000000; // ~100 Mb on Asus2
        //ao1.GaugeText = $"{p:N0}";
        //ao1.MiddlValAnim = 2.5 * p - 150;

        //p = _perfCountAA.NextValue() / 1000; // ~6,000 Mb on Asus2
        //ao2.GaugeText = $"{p:N2}";
        //ao2.MiddlValAnim = 37.5 * p - 150;

        ////p = _perfCounGPU.NextValue(); // exception - Category does not exist.
        ////ao2.GaugeText = $"{p}";
        ////ao2.MiddlValAnim = 37.5 * p - 150;
      }
      catch (Exception ex) { ex.Log($"_perfCounCPU.InstanceName: {_perfCounCPU.InstanceName}"); }
    }
  }
}