using AsLink;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AAV.SS
{
  public partial class GadgetsBinWindow :TopmostUnCloseableWindow
  {
    public GadgetsBinWindow()
    {
      try
      {
        InitializeComponent();
        
        Closed += (s, e) => { App.LogScrSvrUptime("ScrSvr - Dn - GadgetsBinWindow.Closed!"); Application.Current.Shutdown(); };
        KeyUp += (s, e) =>
        {
          switch (e.Key)
          {
            default: e.Handled = false; return;
            case Key.Up: case Key.Escape: { Close(); /*System.Diagnostics.Trace.WriteLine($"{DateTime.Now:HH:mm:ss.f} => ::>Application.Current.Shutdown();6");*/ Application.Current.Shutdown(); } break;
            case Key.F4: onSettings(); break;
          }

          e.Handled = true;
        };
      }
      catch (Exception ex) { Debug.WriteLine(ex, "public GadgetsBinWindow()"); }
    }

    void onSettings(object sender = null, RoutedEventArgs e = null) => new SettingsWindow().ShowDialog();

    void onManualUpdateRequested(object s, RoutedEventArgs e) { ((Button)s).Visibility = Visibility.Collapsed; App.Synth.SpeakAsync($"{Vws.EvDbLogSsvrHelper.UpdateEvLogToDb(15)} rows saved"); }

    void Button_Click(object sender, RoutedEventArgs e) => Close();
  }
}
