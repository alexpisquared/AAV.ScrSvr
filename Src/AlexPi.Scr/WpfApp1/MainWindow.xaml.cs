using System.Windows;

namespace WpfApp1;
public partial class MainWindow : Window
{
  const int periodInSeconds = 5;
  public MainWindow() => InitializeComponent();

  void OnLoaded(object sender, RoutedEventArgs e)
  {
    TimeSpan targetWait = GetNextPeriodWaitTime();

    DateTime now = DateTime.Now;
    _ = Task.Run(async () => await Task.Delay(targetWait)
     ).ContinueWith(_ => Dispatcher.Invoke(() => UpdateTextBlockPeriodicallyAsync()), TaskScheduler.FromCurrentSynchronizationContext());
  }

  private async Task UpdateTextBlockPeriodicallyAsync()
  {
    while (true)
    {
      // update tbk1.Text periodically every periodInSeconds minutes but such that each update happens exactly on 0 second, like:  at 3:00:00.000 3:10:00.000 3:20:00.000 3:20:00.000, etc.
      TimeSpan targetWait = GetNextPeriodWaitTime();
      await Task.Delay(targetWait);
    }
  }

  private TimeSpan GetNextPeriodWaitTime()
  {
    DateTime now = DateTime.Now;
    TimeSpan targetWait = TimeSpan.FromSeconds(periodInSeconds - (now.Second % periodInSeconds)).Subtract(now - now.AddSeconds(-(0.001 * now.Millisecond) - (0.000001 * now.Microsecond)));
    DateTime targetTime = now.Add(targetWait);
    tbk1.Text = $"{now:H:mm:ss.ffffff}\n{targetWait}\n{targetTime:H:mm:ss.ffffff}";
    return targetWait;
  }

  void OnToggleTopmost(object sender, RoutedEventArgs e) => Topmost = !Topmost;
  void OnClose(object sender, RoutedEventArgs e) => Close();
}