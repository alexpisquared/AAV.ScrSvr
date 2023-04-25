using AlexPi.Scr.Unclosables;

namespace AlexPi.Scr.Vws;
public partial class ContainerH : TopmostUnCloseableWindow
{
  bool _isTalking = false;
  readonly Brush
    _red = new SolidColorBrush(Colors.Red),
    _rng = new SolidColorBrush(Colors.Orange),
    _trn = new SolidColorBrush(Colors.Transparent);
  readonly DispatcherTimer _timer;
  const double _gracePeriodSec = 0.1; // overruled with pausing the timer =>  give extra second in case of the tick delayed 1 ms over 1000 ms + wait for the grace period to pass lest speak again.
  const int _timerePeriodSec = 1;

  public ContainerH(AlexPi.Scr.Logic.GlobalEventHandler globalEventHandler) : base(globalEventHandler)
  {
    InitializeComponent();
    _timer = new DispatcherTimer(TimeSpan.FromSeconds(_timerePeriodSec), DispatcherPriority.Background, new EventHandler((s, e) => updateGuiOn1secTimerTick()), Dispatcher.CurrentDispatcher); //tu: prevent screensaver; //tu: one-line timer
    _timer.Start();
    cbIsSayMinOn.IsChecked = AppSettings.Instance.IsSayMinOn;
    cbIsChimesOn.IsChecked = AppSettings.Instance.IsChimesOn;
    cbIsRepeatOn.IsChecked = AppSettings.Instance.IsRepeatOn;
    //IsSaySecOn.IsChecked = AppSettings.Instance.IsSaySecOn;
  }
  async void updateGuiOn1secTimerTick()
  {
    var tnow = DateTime.Now;
    var idle = tnow - App.StartedAt + TimeSpan.FromSeconds(App.IdleTimeoutSec);
    var left = App.StartedAt.AddMinutes(AppSettings.Instance.Min2Sleep) - tnow;
    tbOutOffL.Text = idle.TotalMinutes < 60 ? $"{idle:m\\:ss}" : $"{idle:h\\:mm\\:ss} ";

    if (AppSettings.Instance.AutoSleep)
    {
      tbOutOffR.Text = left.TotalMinutes < 60 ? $"    {left:m\\:ss}" : $"{left:h\\:mm\\:ss} ";
      pbOutOfF.Maximum = pbOutOff.Maximum = (idle + left).TotalMinutes;
      pbOutOfF.Value = pbOutOff.Value = idle.TotalMinutes;
      pbOutOff.Opacity = pbOutOff.Maximum > 0 ? pbOutOff.Value / pbOutOff.Maximum : 0;
    }
    else
      tbOutOffR.Text = "";

    if (!_isTalking && idle.TotalSeconds % 60 <= (_timerePeriodSec + _gracePeriodSec))
    {
      try
      {
        _isTalking = true; _timer.Stop();

        //g1.Background = _red;

        if (AppSettings.Instance.IsSayMinOn || AppSettings.Instance.IsChimesOn || AppSettings.Instance.IsRepeatOn)
        {
          //await ChimerAlt.WakeAudio(); // wake up monitor's audio.
          if (AppSettings.Instance.IsSayMinOn)
          {
            if (idle.Minutes % 5 == 0 && DateTime.Now.Minute % 10 == 0)
              App.SayExe($"{idle.Minutes} as well as {DateTime.Now:H:mm}");
            else if (idle.Minutes % 5 == 0)
              await App.SpeakAsync($"{idle.Minutes}");
            else if (DateTime.Now.Minute % 10 == 0)
              await App.SpeakAsync($"{DateTime.Now:H:mm}");
            //await App.SpeakAsync(DateTime.Now.Minute % 10 == 0 ? $"{idle.Minutes} as well as {DateTime.Now:H:mm}" : $"{idle.Minutes}"); } //2022-06-01
          }

          if (AppSettings.Instance.IsChimesOn) { /*await ChimerAlt.Chime(idle.Minutes);*/ } //nogo: .ConfigureAwait(false);}

          if (AppSettings.Instance.IsRepeatOn) { App.SpeakFaF($"{idle.Minutes} minutes, that is."); }

          //g1.Background = _rng;
        }

        await Task.Delay((int)(_gracePeriodSec * 1000)); // lest repeat the same on the next tick (2020-12-02)

        //g1.Background = _trn;
      }
      catch (Exception ex) { _ = ex.Log(); }
      finally
      {
        _isTalking = false; _timer.Start();
      }
    }
  }

  void onSayMinChanged(object s, RoutedEventArgs e)
  {
    AppSettings.Instance.IsSayMinOn = cbIsSayMinOn.IsChecked == true;
    if (AppSettings.Instance.IsSayMinOn == false)
      cbIsChimesOn.IsChecked = cbIsRepeatOn.IsChecked = false;
  }
  void onChimesChanged(object s, RoutedEventArgs e) => AppSettings.Instance.IsChimesOn = cbIsChimesOn.IsChecked == true;
  void onRepeatChanged(object s, RoutedEventArgs e) => AppSettings.Instance.IsRepeatOn = cbIsRepeatOn.IsChecked == true;
  async void onFreqWalk(object s, RoutedEventArgs e) { } //  => await ChimerAlt.FreqWalkUpDn();
  void onGreetFaf(object s, RoutedEventArgs e) { var sj = new ConfigRandomizer(); App.SpeakFaF($"Hey, {sj.GetRandomFromUserSection("FirstName")}!", sj.GetRandomFromUserSection("VoiceF")); }
  void onGreetAlt(object s, RoutedEventArgs e) { var sj = new ConfigRandomizer(); App.SpeakFaF($"Hey, {sj.GetRandomFromUserSection("FirstName")}!", sj.GetRandomFromUserSection("VoiceM")); }
}