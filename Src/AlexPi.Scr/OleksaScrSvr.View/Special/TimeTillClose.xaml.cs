﻿namespace OleksaScrSvr.View.Special;

public partial class TimeTillClose : UserControl
{
  TimeSpan _tsExecutionDuration;
  DateTime _finishTime;
  readonly DateTime _idleStart = DateTime.Now.AddMinutes(-4);

  public TimeTillClose() => InitializeComponent();

  async void OnLoaded(object sender, RoutedEventArgs e)
  {
    while (((Duration)Application.Current.Resources["ExecutionDuration"]).TimeSpan == TimeSpan.FromSeconds(59))
    {
      WriteLine($"     -- {DateTime.Now:ss.f} ExecutionDuration {Application.Current.Resources["ExecutionDuration"]}");
      await Task.Delay(1);
    }

    WriteLine($"     ++ {DateTime.Now:ss.f} ExecutionDuration {Application.Current.Resources["ExecutionDuration"]}");

    _tsExecutionDuration = ((Duration)Application.Current.Resources["ExecutionDuration"]).TimeSpan;
    ProgressTime.Text = $"{_tsExecutionDuration:h\\:mm}";
    _finishTime = _idleStart.AddMinutes(4) + _tsExecutionDuration;

    //*864: animations draw CPU time, ... even when paused !!!      
    //var da = (DoubleAnimation)FindResource("ExecutionDurationDA");
    //da.Duration = (Duration)Application.Current.Resources["ExecutionDuration"];
    //((Storyboard)FindResource("ExecutionDurationSB")).Stop();
    //((Storyboard)FindResource("ExecutionDurationSB")).Begin();

    new BackgroundTask(TimeSpan.FromSeconds(1), OnTimer).Start();
  }

  void OnTimer()
  {
    var now = DateTime.Now;
    ProgressTime.Text = $"{now - _idleStart:h\\:mm}:  ";
    ProgressTimB.Text = $"{(_tsExecutionDuration - (now - _idleStart)).Add(TimeSpan.FromMinutes(4)):h\\:mm}";

    progressArc2.Angle = (now - _idleStart).Seconds * 100 / 60;
    progressArc1.Angle = 100 * (1 - (_finishTime - now) / _tsExecutionDuration); //*864: animations draw CPU time, ... even when paused !!!      

    MinuteHand.Angle = (now.Minute + now.Second / 60.0) * 6;
    HourHand.Angle = (now.Hour * 360 + MinuteHand.Angle) / 12; // ProgressTimB.Text = $"{now.Second} => {MinuteHand.Angle:N0}";
  }
}
