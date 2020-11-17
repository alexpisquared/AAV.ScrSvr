using System;
using System.Threading.Tasks;
using System.Windows;

namespace RadarLib
{
  public partial class RadarAnimation : AAV.WPF.Base.WindowBase
  {
    readonly bool _isStandalole = true;

    public RadarAnimation(bool isStandalone, double alarmThreshold = .05)
    {
      InitializeComponent();

      if (Environment.GetCommandLineArgs().Length > 1 && Environment.GetCommandLineArgs()[1] == "ModeH")
        Hide();

      _isStandalole = isStandalone;

      Loaded += async (s, e) =>
            {
              if (Environment.GetCommandLineArgs().Length > 1)
              {
                if (Environment.GetCommandLineArgs()[1] == "ShowIfOnOrComingSoon")
                {
                  await Task.Delay(180000);
                  { Close(); System.Diagnostics.Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} => ::>Application.Current.Shutdown();g"); Application.Current.Shutdown(); } //App.Current.Shutdown();
                }
              }
            };

      ruc1.AlarmThreshold = alarmThreshold;
      ruc1.Focus();
    }
  }
}