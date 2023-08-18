using Db.EventLog.Ext;
using StandardLib.Helpers;

namespace Db.EventLog.Explorer;

public partial class RODBView 
{
  readonly A0DbModel _db;
  readonly string _localdb;
  public RODBView(string dbfile)
  {
    InitializeComponent(); KeyDown += (s, ves) => { switch (ves.Key) { case Key.Escape: Close(); break; } };
    _localdb = dbfile;
    FileAttributeHelper.RmvAttribute(_localdb, FileAttributes.ReadOnly);
    _db = A0DbModel.GetLclFl(dbfile);
    Loaded += onLoaded;
    samePC.IsEnabled = _localdb.Contains(Environment.MachineName);
  } // using AsLink.UI;
  protected override void OnClosing(System.ComponentModel.CancelEventArgs e) { base.OnClosing(e); FileAttributeHelper.AddAttribute(_localdb, FileAttributes.ReadOnly); }
  //public static readonly DependencyProperty ZVProperty = DependencyProperty.Register("ZV", typeof(double), typeof(RODBView), new PropertyMetadata(1d)); public double ZV { get { return (double)GetValue(ZVProperty); } set { SetValue(ZVProperty, value); } }

  async void onLoaded(object s, RoutedEventArgs e)
  {
    ZoomablePanel.IsEnabled = false;
    try
    {
      tbInfo.Text = $"Loading ... {_localdb}";
      tbCurVer.Text = $"{VersionHelper.CurVerStr("")}";

      await _db.PcLogics.LoadAsync();
      await _db.EvOfInts.OrderByDescending(r => r.TimeID).LoadAsync(); //tu: error "'EditItem' is not allowed for this view." if Order is done on Local !!!!!!!!!!!!!!

      ((CollectionViewSource)(this.FindResource("pcLogicViewSource"))).Source = _db.PcLogics.Local;
      ((CollectionViewSource)(this.FindResource("pcLogicEvOfIntsViewSource"))).Source = _db.EvOfInts.Local;
      ((CollectionViewSource)(this.FindResource("pcLogicEvOfIntsViewSourRO"))).Source = _db.EvOfInts.Local;

      tbInfo.Text = $"{_db.PcLogics.Local.Count} PCs, {_db.EvOfInts.Local.Count} events in \r\n{_localdb}.";
    }
    catch (Exception ex) { ex.Pop(); ; }
    finally { ZoomablePanel.IsEnabled = true; }
  }
  async void onDbSave(object s, RoutedEventArgs e)
  {
    ZoomablePanel.IsEnabled = false;
    try { tbInfo.ToolTip = tbInfo.Text = (await _db.TrySaveReportAsync()).report; }
    catch (Exception ex) { ex.Pop(); ; }
    finally { ZoomablePanel.IsEnabled = true; }
  }
  async void onLoadEventsForToday(object s, RoutedEventArgs e) => await evLogToDb_days(1);
  async void onLoadEventsForAWeek(object s, RoutedEventArgs e) => await evLogToDb_days(7);
  async void onLoadEventsForMonth(object s, RoutedEventArgs e) => await evLogToDb_days(32);
  async void onLoadEventsFor1Year(object s, RoutedEventArgs e) => await evLogToDb_days(365);

  async Task evLogToDb_days(int daysBack)
  {
    ZoomablePanel.IsEnabled = false;
    try
    {
      var before = _db.EvOfInts.Local.Count;
      var afterr = await evLogToDb(daysBack);

      ((CollectionViewSource)(this.FindResource("pcLogicViewSource"))).View.Refresh();
      ((CollectionViewSource)(this.FindResource("pcLogicEvOfIntsViewSource"))).View.Refresh();

      tbInfo.Text = $"Events: {before} before, {afterr}/{(await _db.TrySaveReportAsync()).rowsSavedCnt} found/saved, {_db.EvOfInts.Local.Count} after.";
    }
    catch (Exception ex) { ex.Pop(); ; }
    finally { ZoomablePanel.IsEnabled = true; }
  }
  async Task<int> evLogToDb(int daysBack)
  {
    var n = 0;
    try
    {
      tbInfo.Text = $"Loading...";
      for (var i = 0; i < daysBack; i++)
      {
        n += await UpdateDbWithNewLogEvents(DateTime.Today.AddDays(-i), _db);
        tbInfo.Text = $"daysBack:{(daysBack - i),4} - rows added/saved:{n,5}.";
      }
    }
    catch (Exception ex) { ex.Pop(); ; }
    finally { ZoomablePanel.IsEnabled = true; }

    return n;
  }

  [Obsolete]
  public static async Task<int> UpdateDbWithNewLogEvents(DateTime trgDate, A0DbModel db) //todo: remove the dupes into central space
  {
    var rowsAddedSaved = 0;
    try
    {
      var t1 = trgDate;
      var t2 = trgDate.AddDays(.99999);
      await Task.Run(() => EvLogHelper.GetAllUpDnEvents(t1, t2)).ContinueWith(_ =>
      {
        try
        {
          var dailyEvents = _.Result;
          if (dailyEvents.Count < 1)
            Debug.WriteLine($"==>{trgDate:MMM-dd, ddd}:   No events registered for the day.");
          else
            rowsAddedSaved = DbLogHelper.FindNewEventsToSaveToDb(_.Result, Environment.MachineName, $"Db.EvLog.Explr {trgDate}", db);
        }
        catch (Exception ex) { ex.Pop(); rowsAddedSaved = -5; }
      }, TaskScheduler.FromCurrentSynchronizationContext());
    }
    catch (Exception ex) { _ = ex.Log(); rowsAddedSaved = -1; }

    return rowsAddedSaved;
  }

  void onSyncFromLocalFileToCentralDb(object s, RoutedEventArgs e)
  {
    ((Button)s).IsEnabled = false;
    try
    {
      var (newRows, report, swstopwatch) = new DataTransfer().CopyChunkyAzureSync(_db, A0DbModel.GetExprs); // .GetAzure);
      tbInfo.Text = $"{report}";
    }
    catch (Exception ex) { ex.Pop(); ; }
    finally { ((Button)s).IsEnabled = true; }
  }
}
//todo: check SSVR for properly saving new events to db.