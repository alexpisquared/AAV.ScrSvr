using AAV.SS.AltBpr;
using AsLink;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace AAV.SS.Vws
{
  public partial class ContainerH : TopmostUnCloseableWindow
  {
    bool _isTalking = false;
    readonly Brush
      _red = new SolidColorBrush(Colors.Red),
      _rng = new SolidColorBrush(Colors.Orange),
      _trn = new SolidColorBrush(Colors.Transparent);
    readonly DispatcherTimer _timer;

    public ContainerH(AAV.SS.Logic.GlobalEventHandler globalEventHandler) : base(globalEventHandler)
    {
      InitializeComponent();
      _timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, new EventHandler((s, e) => updateClock()), Dispatcher.CurrentDispatcher); //tu: prevent screensaver; //tu: one-line timer
      _timer.Start();
      cbIsSayMinOn.IsChecked = AppSettings.Instance.IsSayMinOn;
      cbIsChimesOn.IsChecked = AppSettings.Instance.IsChimesOn;
    }
    async void updateClock()
    {
      var tnow = DateTime.Now;
      var idle = ((tnow - App.Started) + TimeSpan.FromSeconds(App.ScrSvrTimeoutSec));
      var left = App.Started.AddMinutes(AppSettings.Instance.Min2Sleep) - tnow;
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

      const int gracePeriodSec = 2; // give extra second in case of the tick delayed 1 over 1000 ms + wait for the grace period to pass lest speak again.
      if (cbIsSayMinOn.IsChecked == true && !_isTalking && idle.TotalSeconds % 60 <= gracePeriodSec)
      {
        if (AppSettings.Instance.IsSayMinOn)
        {
          _timer.Stop();
          {
            g1.Background = _red;
            await Task.Delay(100);
            g1.Background = _rng;
            await Task.Delay(100);          //await Task.Delay(gracePeriodSec * 1000);
            g1.Background = _trn;

            _isTalking = true;
            {
              App.SpeakSynch($"{idle.Minutes}");
              if (AppSettings.Instance.IsChimesOn)
              {
                await AltBpr.ChimerAlt.Chime(idle.Minutes); //nogo: .ConfigureAwait(false);
                App.SpeakAsync($"{idle.Minutes} minutes, that is.");
              }
            }
            _isTalking = false;

          }
          _timer.Start();
        }
      }
    }

    void onSayMinChanged(object sender, System.Windows.RoutedEventArgs e)
    {
      AppSettings.Instance.IsSayMinOn = cbIsSayMinOn.IsChecked == true;
      if (AppSettings.Instance.IsSayMinOn == false)
        cbIsChimesOn.IsChecked = false;
    }
    void onChimesChanged(object sender, System.Windows.RoutedEventArgs e) => AppSettings.Instance.IsChimesOn = cbIsChimesOn.IsChecked == true;
    async void onFreqWalk(object s, RoutedEventArgs e) => await ChimerAlt.FreqWalkUpDn();

  }
}