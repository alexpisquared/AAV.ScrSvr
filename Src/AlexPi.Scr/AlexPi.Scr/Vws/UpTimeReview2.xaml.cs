using AAV.Sys.Ext;
using AsLink;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AlexPi.Scr.Vws
{
  public partial class UpTimeReview2 : AAV.WPF.Base.WindowBase
  {
    public UpTimeReview2()
    {
      try { InitializeComponent(); } catch (Exception ex) { ex.Log(); }

#if __DEBUG
      jjj();
#endif
    }

    async void onLoaded_UpdateMdb(object s, System.Windows.RoutedEventArgs e)
    {
      var evNo = await EvLogHelper.UpdateEvLogToDb(15, $"");
      var rprt = $"{(evNo < -3 ? "No" : evNo.ToString())} new events found/stored to MDB file.";
      App.SpeakFaF(rprt);
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
