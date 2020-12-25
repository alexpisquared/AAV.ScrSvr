using AAV.Sys.Ext;
using AAV.WPF.AltBpr;
using AsLink;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace AlexPi.Scr.Vws
{
  public partial class ContainerH : TopmostUnCloseableWindow
  {
    bool _isTalking = false;
    readonly Brush
      _red = new SolidColorBrush(Colors.Red),
      _rng = new SolidColorBrush(Colors.Orange),
      _trn = new SolidColorBrush(Colors.Transparent);
    readonly DispatcherTimer _timer;

    public ContainerH(AlexPi.Scr.Logic.GlobalEventHandler globalEventHandler) : base(globalEventHandler)
    {
      InitializeComponent();
      _timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, new EventHandler((s, e) => updateClockOn1secTimerTick()), Dispatcher.CurrentDispatcher); //tu: prevent screensaver; //tu: one-line timer
      _timer.Start();
      cbIsSayMinOn.IsChecked = AppSettings.Instance.IsSayMinOn;
      cbIsChimesOn.IsChecked = AppSettings.Instance.IsChimesOn;
      cbIsRepeatOn.IsChecked = AppSettings.Instance.IsRepeatOn;
      //IsSaySecOn.IsChecked = AppSettings.Instance.IsSaySecOn;
    }
    async void updateClockOn1secTimerTick()
    {
      var tnow = DateTime.Now;
      var idle = ((tnow - App.StartedAt) + TimeSpan.FromSeconds(App.ScrSvrTimeoutSec));
      var left = App.StartedAt.AddMinutes(AppSettings.Instance.Min2Sleep) - tnow;
      tbOutOffL.Text = (idle.TotalMinutes < 60 ? $"{idle:m\\:ss}" : $"{idle:h\\:mm} ");

      if (AppSettings.Instance.AutoSleep)
      {
        tbOutOffR.Text = (left.TotalMinutes < 60 ? $"{left:m\\:ss}" : $"{left:h\\:mm} ");
        pbOutOff.Maximum = (idle + left).TotalMinutes;
        pbOutOff.Value = idle.TotalMinutes;
        pbOutOff.Opacity = pbOutOff.Maximum > 0 ? pbOutOff.Value / pbOutOff.Maximum : 0;
      }
      else
        tbOutOffR.Text = "";

      const int gracePeriodSec = 2; // give extra second in case of the tick delayed 1 ms over 1000 ms + wait for the grace period to pass lest speak again.
      if (AppSettings.Instance.IsSayMinOn && cbIsSayMinOn.IsChecked == true && !_isTalking && idle.TotalSeconds % 60 <= gracePeriodSec)
      {
        try
        {
          _isTalking = true;
          _timer.Stop();

          g1.Background = _red;
          await ChimerAlt.Wake(150111); // wake up monitor's audio.
          await App.SpeakAsync($"{idle.Minutes}");

          g1.Background = _rng;
          if (AppSettings.Instance.IsChimesOn)
          {
            await ChimerAlt.Chime(idle.Minutes); //nogo: .ConfigureAwait(false);
            App.SpeakFaF($"{idle.Minutes} minutes, that is.");
          }
          else if (AppSettings.Instance.IsRepeatOn)
          {
            App.SpeakFaF($"{idle.Minutes} minutes, that is.");
          }
          else
          {
            await Task.Delay(gracePeriodSec * 1000); // lest repeat the same on the next tick (2020-12-02)
          }

          g1.Background = _trn;
        }
        catch (Exception ex) { ex.Log(); }
        finally
        {
          _isTalking = false;
          _timer.Start();
        }
      }
    }

    void onSayMinChanged(object sender, System.Windows.RoutedEventArgs e)
    {
      AppSettings.Instance.IsSayMinOn = cbIsSayMinOn.IsChecked == true;
      if (AppSettings.Instance.IsSayMinOn == false)
        cbIsChimesOn.IsChecked = cbIsRepeatOn.IsChecked = false;
    }

    void TopmostUnCloseableWindow_Unloaded(object sender, RoutedEventArgs e) { }

    void onChimesChanged(object sender, System.Windows.RoutedEventArgs e) => AppSettings.Instance.IsChimesOn = cbIsChimesOn.IsChecked == true;
    void onRepeatChanged(object sender, System.Windows.RoutedEventArgs e) => AppSettings.Instance.IsRepeatOn = cbIsRepeatOn.IsChecked == true;
    async void onFreqWalk(object s, RoutedEventArgs e) => await ChimerAlt.FreqWalkUpDn();
  }
}