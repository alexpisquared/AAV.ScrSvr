using AlexPi.Scr.Unclosables;
using AsLink;
using System.Windows;
using System.Windows.Controls;

namespace AlexPi.Scr.Vws
{
  public partial class ContainerJ : TopmostUnCloseableWindow
  {
    public ContainerJ(AlexPi.Scr.Logic.GlobalEventHandler globalEventHandler) : base(globalEventHandler) => InitializeComponent();
    async void onManualUpdateRequested(object s, RoutedEventArgs e) { ((Button)s).Visibility = Visibility.Collapsed; await App.SpeakAsync($"{await EvLogHelper.UpdateEvLogToDb(15, "ContainerJ.onManualUpdateRequested()")} rows saved"); }
  }
}