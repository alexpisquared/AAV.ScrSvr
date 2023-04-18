using AAV.Sys.Helpers;
using AAV.WPF.Ext;
using AsLink;
using Db.EventLog.DbModel;
using Db.EventLog.Main;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Db.EventLog.Explorer;

public partial class MainEvLogExplr : AAV.WPF.Base.WindowBase
{
  public MainEvLogExplr()
  {
    InitializeComponent();
    KeyDown += (s, ves) =>
    {
      switch (ves.Key)
      {
        default: System.Diagnostics.Trace.WriteLine($"::>>{ves} \t {ves.Key}"); break;
        case Key.Escape: { Close(); System.Diagnostics.Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} => ::>Application.Current.Shutdown();p"); Application.Current.Shutdown(); } App.Current.Shutdown(); break;
      }
    };
  }

  async void onLoaded(object s, RoutedEventArgs e)
  {
    vizroot.IsEnabled = false;
    try
    {
      tbInfo.Text = $"Loading ...  ";
      tbCurVer.Text = $"{VerHelper.CurVerStr()}";

      IEnumerable<EvOfInt>? eoi = null;
      var trgDate = DateTime.Today;
      var t1 = trgDate;
      var t2 = trgDate.AddDays(.99999);
      await Task.Run(() => EvLogHelper.GetAllUpDnEvents(t1, t2)).ContinueWith(_ =>
      {
        try
        {
          var de = _.Result;
          if (de.Count < 1) { }
          else
          {
            eoi = de.Select(e => new EvOfInt { TimeID = e.Key, EvOfIntFlag = e.Value, MachineName = Environment.MachineName });
          }
        }
        catch (Exception ex) { ex.Pop(); }
      }, TaskScheduler.FromCurrentSynchronizationContext());
      
      ArgumentNullException.ThrowIfNull(eoi);
      ((CollectionViewSource)FindResource("evOfIntViewSource")).Source = eoi;

      tbInfo.Text = $"{eoi.Count()} events.";
    }
    catch (Exception ex) { ex.Pop(); ; }
    finally { vizroot.IsEnabled = true; }
  }
  void pcChanged(object s, System.Windows.Controls.SelectionChangedEventArgs e)
  {
    if (e.AddedItems?.Count < 1) return;

    var pc = ((PcLogic)e.AddedItems[0]);
    var dt = pc.LogReadAt == null ? TimeSpan.Zero : (DateTime.Now - pc.LogReadAt);

    tbInfo.Text = $"{pc.MachineName}. Log read {dt:h\\:mm} ago."; //tbInfo.Text = $"{pc.MachineName} - {await _db.EvOfInts.CountAsync(r => r.MachineName.Equals(pc.MachineName, StringComparison.OrdinalIgnoreCase))} events. Log read {dt:h\\:mm} ago.";

    if (((DataGrid)s).Name == "pcLogicDataGridRO")
    {
      stuc.ReloadThisPC(pc);
    }
  }
  async void Window_SizeChanged(object s, SizeChangedEventArgs e)
  {
    var lastSize = e.NewSize;
    await Task.Delay(9999);
    if (lastSize.Width == e.NewSize.Width) // if finished resizing.
      stuc.RedrawOnResize(s, null);
  }
}
