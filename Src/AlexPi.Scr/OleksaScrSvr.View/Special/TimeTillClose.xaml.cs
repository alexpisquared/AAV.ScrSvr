namespace OleksaScrSvr.View.Special;

public partial class TimeTillClose : UserControl
{
  TimeSpan _tsExecutionDuration;
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

    var da = (DoubleAnimation)FindResource("ExecutionDurationDA");
    da.Duration = (Duration)Application.Current.Resources["ExecutionDuration"];

    ((Storyboard)FindResource("ExecutionDurationSB")).Stop();
    ((Storyboard)FindResource("ExecutionDurationSB")).Begin();

    new BackgroundTask(TimeSpan.FromSeconds(1), OnTimer).Start();
  }

  void OnTimer()
  {
    ProgressTime.Text = $"{DateTime.Now - _idleStart:h\\:mm}:  ";
    ProgressTimB.Text = $"{_tsExecutionDuration - (DateTime.Now - _idleStart):h\\:mm}";
    progressArc2.Angle = (DateTime.Now - _idleStart).Seconds * 100 / 60;
  }
}
