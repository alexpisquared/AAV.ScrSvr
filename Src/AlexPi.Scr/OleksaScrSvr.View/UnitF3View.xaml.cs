namespace OleksaScrSvr.View;

public partial class UnitF3View
{
  TimeSpan _ts;
  readonly DateTime _idleStart;

  public UnitF3View()
  {
    InitializeComponent();
    _idleStart = DateTime.Now.AddMinutes(-4);
  }

  async void OnLoaded(object sender, RoutedEventArgs e)
  {
    await OnLoadedAsync();

    while (((Duration)Application.Current.Resources["ExecutionDuration"]).TimeSpan == TimeSpan.FromSeconds(59))
    {
      WriteLine($"     -- {DateTime.Now:ss.f} ExecutionDuration {Application.Current.Resources["ExecutionDuration"]}");
      await Task.Delay(1);
    }

    WriteLine($"     ++ {DateTime.Now:ss.f} ExecutionDuration {Application.Current.Resources["ExecutionDuration"]}");

    _ts = ((Duration)Application.Current.Resources["ExecutionDuration"]).TimeSpan;
    ProgressTime.Text = $"{_ts:h\\:mm}";

    var da = (DoubleAnimation)FindResource("ExecutionDurationDA");
    da.Duration = (Duration)Application.Current.Resources["ExecutionDuration"];

    ((Storyboard)FindResource("ExecutionDurationSB")).Stop();
    ((Storyboard)FindResource("ExecutionDurationSB")).Begin();

    var bt = new BackgroundTask(TimeSpan.FromSeconds(3), OnTimer);
    bt.Start();
    //await Task.Delay(300);
    //await bt.StopAsync();
  }

  void OnTimer()
  {
    ProgressTime.Text = $"{DateTime.Now - _idleStart:h\\:mm}:  ";
    ProgressTimB.Text = $"{_ts - (DateTime.Now - _idleStart):h\\:mm}";
    progressArc2.Angle = (DateTime.Now - _idleStart).Seconds * 100 / 60;
  }
}