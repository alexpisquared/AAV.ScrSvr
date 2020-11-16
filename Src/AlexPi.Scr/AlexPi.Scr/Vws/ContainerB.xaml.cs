using AAV.Sys.Ext;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace AAV.SS.Vws
{
  public partial class ContainerB : TopmostUnCloseableWindow
  {
    PerformanceCounter
            //_perfCountAA = new PerformanceCounter("Memory", "Available MBytes"), // new PerformanceCounter(categoryName: "Process", counterName: "Private Bytes", instanceName: "AAV.SS.scr"),
            //_perfCountEx = new PerformanceCounter(categoryName: "Process", counterName: "Private Bytes", instanceName: "Explorer"),
            //_perfCounGPU = new PerformanceCounter("GPU", "% GPU Time", "_Total"), // % GPU Time-Base - https://github.com/Alexey-Kamenev/GpuPerfCounters/blob/master/src/GpuPerfCounters/PerfCounterService.cs
            //_ramCounterr = new PerformanceCounter("Memory", "Available MBytes")
            _perfCounCPU;// = new PerformanceCounter("Processor", "% Processor Time", "_Total");
    double sum;
    int cnt;

    public ContainerB(AAV.SS.Logic.GlobalEventHandler globalEventHandler) : base(globalEventHandler)
    {
      InitializeComponent();

      //_perfCountAA = new PerformanceCounter("Memory", "Available MBytes"), // new PerformanceCounter(categoryName: "Process", counterName: "Private Bytes", instanceName: "AAV.SS.scr"),
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

        gaugeTorCPU.InnerValAnim = (sum += v) / (++cnt);
        gaugeTorCPU.MiddlValAnim = v;

        if (gaugeTorCPU.OuterVal < v)
          gaugeTorCPU.OuterValAnim = v;

        gaugeTorCPU.GaugeText = $"{p:N0}\r\n{((gaugeTorCPU.InnerVal + 90) / k):N0} - {((gaugeTorCPU.OuterVal + 90) / k):N0}";

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