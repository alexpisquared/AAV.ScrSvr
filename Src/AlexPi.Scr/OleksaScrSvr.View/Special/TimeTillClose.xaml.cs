using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OleksaScrSvr.View.Special
{
    public partial class TimeTillClose : UserControl
  {
    TimeSpan _ts;
    readonly DateTime _idleStart;

    public TimeTillClose()
    {
      InitializeComponent();
      _idleStart = DateTime.Now.AddMinutes(-4);
    }

    async void OnLoaded(object sender, RoutedEventArgs e)
    {
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
}
