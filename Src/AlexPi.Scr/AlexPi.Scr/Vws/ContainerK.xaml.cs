using AsLink;
using System.Windows;
using System.Windows.Controls;

namespace AlexPi.Scr.Vws
{
  public partial class ContainerK : TopmostUnCloseableWindow
  {
    public ContainerK(AlexPi.Scr.Logic.GlobalEventHandler globalEventHandler) : base(globalEventHandler) => InitializeComponent();
    async void onManualUpdateRequested(object s, RoutedEventArgs e) { ((Button)s).Visibility = Visibility.Collapsed; await App.SpeakAsync($"{await EvLogHelper.UpdateEvLogToDb(15, "ContainerK.onManualUpdateRequested()")} rows saved"); }
  }
}