using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1;
public partial class MainWindow : Window
{
  public MainWindow()
  {
    InitializeComponent();
  }

  async void OnLoaded(object sender, RoutedEventArgs e)
  {

    tbk1.Text = $"{DateTime.Now:H:mm:ss.fffffff}";

    while (true)
    {
      // update tbk1.Text periodically every 2 minutes but such that each update happens exactly on 0 second, like:  at 3:00:00.000 3:02:00.000 3:02:04.000 3:06:00.000, etc.
      var currentTime = DateTime.Now;
      var targetWait = TimeSpan.FromMinutes(2 - currentTime.Minute % 2).Subtract(currentTime- currentTime.AddSeconds(-currentTime.Second));
      var targetTime = currentTime.Add(targetWait);
      tbk1.Text = $"{currentTime:H:mm:ss.fffffff}\n{targetTime:H:mm:ss.fffffff}";
      await Task.Delay(targetWait);
    }


  }
}