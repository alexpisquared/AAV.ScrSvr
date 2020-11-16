using AAV.Sys.Ext;
using AsLink;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AAV.SS.Vws
{
  public partial class UpTimeReview2 : AAV.WPF.Base.WindowBase
  {
    readonly bool requirePasskey;

    public UpTimeReview2(bool requirePasskey)
    {
      try { InitializeComponent(); } catch (Exception ex) { ex.Log(); }

      this.requirePasskey = requirePasskey;

#if DEBUG
      jjj();
#endif
    }

    async void onUpdateMdb(object s, System.Windows.RoutedEventArgs e)
    {
      var evNo = await EvLogHelper.UpdateEvLogToDb(15, $"");
      var rprt = $"{(evNo < -3 ? "No" : evNo.ToString())} new events found/stored to MDB file.";
      App.SpeakSynch(rprt);
      await Task.Delay(750);
    }

    void jjj()
    {
      var h00 = DateTime.Today;
      var h24 = h00.AddDays(.9999999);
      var lst = EvLogHelper.GetAllUpDnEvents(h00, h24);
      foreach (var item in lst)
      {
        Debug.WriteLine($"---- {item.Key},  {item.Value}");
      }
    }
  }
}
