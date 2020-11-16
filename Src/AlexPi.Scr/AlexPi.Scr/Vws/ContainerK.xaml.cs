using AsLink;
using System.Windows;
using System.Windows.Controls;

namespace AAV.SS.Vws
{
  public partial class ContainerK : TopmostUnCloseableWindow
  {
    public ContainerK(AAV.SS.Logic.GlobalEventHandler globalEventHandler) : base(globalEventHandler) => InitializeComponent();
    async void onManualUpdateRequested(object s, RoutedEventArgs e) { ((Button)s).Visibility = Visibility.Collapsed; App.SpeakAsync($"{await EvLogHelper.UpdateEvLogToDb(15, "ContainerK.onManualUpdateRequested()")} rows saved"); }
  }
}