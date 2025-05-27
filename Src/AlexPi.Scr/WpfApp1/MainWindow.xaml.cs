using System.Windows;

namespace WpfApp1;
public partial class MainWindow : Window
{
  public MainWindow() => InitializeComponent();

  async void OnLoaded(object sender, RoutedEventArgs e)
  {

    tbk1.Text = $"{DateTime.Now:H:mm:ss.fffffff}";

    const int periodInMinutes = 10;
    while (true)
    {
      // update tbk1.Text periodically every periodInMinutes minutes but such that each update happens exactly on 0 second, like:  at 3:00:00.000 3:10:00.000 3:20:00.000 3:20:00.000, etc.
      DateTime currentTime = DateTime.Now;
      TimeSpan targetWait = TimeSpan.FromMinutes(periodInMinutes - (currentTime.Minute % periodInMinutes)).Subtract(currentTime - currentTime.AddSeconds(-currentTime.Second - 0.001 * currentTime.Millisecond - 0.000001 * currentTime.Microsecond));
      DateTime targetTime = currentTime.Add(targetWait);
      tbk1.Text = $"{currentTime:H:mm:ss.ffffff}\n{targetTime:H:mm:ss.ffffff}";
      await Task.Delay(targetWait);
    }
  }
}