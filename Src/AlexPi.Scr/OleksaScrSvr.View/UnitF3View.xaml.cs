using System.Windows.Media.Animation;

namespace OleksaScrSvr.View;

public partial class UnitF3View
{
  public UnitF3View()
  {
    InitializeComponent();
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

    ProgressTime.Text = $"{((Duration)Application.Current.Resources["ExecutionDuration"]).TimeSpan:h\\:mm}";

    var da = (DoubleAnimation)FindResource("ExecutionDurationDA");
    da.Duration = ((Duration)Application.Current.Resources["ExecutionDuration"]);

    ((Storyboard)FindResource("ExecutionDurationSB")).Stop();
    ((Storyboard)FindResource("ExecutionDurationSB")).Begin();
  }
}