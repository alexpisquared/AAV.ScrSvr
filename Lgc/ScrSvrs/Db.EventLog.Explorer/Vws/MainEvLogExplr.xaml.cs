using AAV.Sys.Helpers;
using AAV.WPF.Ext;
using Db.EventLog.DbModel;
using Db.EventLog.Main;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Db.EventLog.Explorer
{
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
        tbCurVer.Text = $"{VerHelper.CurVerStr(".NET 4.8")}";

        var itemsSrc = await DbLogHelper.AllPCsAsync();
        tbInfo.Text = $"Loading {itemsSrc.Count} PCs...  ";

        var _localdb = OneDrive.Folder($@"{DbLogHelper._dbSubP}LocalDb({Environment.MachineName}).mdf");
        FileAttributeHelper.RmvAttribute(_localdb, FileAttributes.ReadOnly);
        var _db = A0DbModel.GetLclFl(_localdb);
        await _db.PcLogics.LoadAsync();
        await _db.EvOfInts.LoadAsync();

        //foreach (PcLogic item in ((List<PcLogic>)pcLogicDataGridRO.ItemsSource).Where(r => !r.MachineName.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase)))                    _db.PcLogics.Local.Add(item);

        ((CollectionViewSource)(FindResource("pcLogicViewSource"))).Source = _db.PcLogics.Local;
        ((CollectionViewSource)(FindResource("evOfIntViewSource"))).Source = _db.EvOfInts.Local;

        tbInfo.Text = $"{_db.PcLogics.Local.Count} PCs with {_db.EvOfInts.Local.Count} events.";

        pcLogicDataGridRO.ItemsSource = itemsSrc;
        foreach (var item in itemsSrc)
        {
          if (item.MachineName.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase))
            pcLogicDataGridRO.SelectedItem = item;
        }
        pcLogicDataGridRO.Focus();
      }
      catch (Exception ex) { ex.Pop(); ; }
      finally { vizroot.IsEnabled = true; }
    }
    void onEditWin(object s, RoutedEventArgs e)
    {
      if (pcLogicDataGridRO.SelectedItems.Count > 0 &&
          pcLogicDataGridRO.SelectedItems[0] is PcLogic)
      {
        var localdb = OneDrive.Folder($@"{DbLogHelper._dbSubP}LocalDb({(pcLogicDataGridRO.SelectedItems[0] as PcLogic).MachineName}).mdf");
        if (File.Exists(localdb))
          new RODBView(localdb).ShowDialog();
      }
      else
        MessageBox.Show($"{pcLogicDataGridRO.SelectedItems[0]}", "Not there");
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
}
