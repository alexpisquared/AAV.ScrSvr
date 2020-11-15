using AsLink;
using System.Windows;
using System.Windows.Controls;

namespace AAV.SS.Vws
{
  public partial class ContainerJ : TopmostUnCloseableWindow
  {
    public ContainerJ(AAV.SS.Logic.GlobalEventHandler globalEventHandler) : base(globalEventHandler) => InitializeComponent();
    async void onManualUpdateRequested(object s, RoutedEventArgs e) { ((Button)s).Visibility = Visibility.Collapsed; App.SpeakAsync($"{await EvLogHelper.UpdateEvLogToDb(15, "ContainerJ.onManualUpdateRequested()")} rows saved"); }
  }
}