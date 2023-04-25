using AlexPi.Scr.Unclosables;

namespace AlexPi.Scr.Vws;

public partial class ContainerA : TopmostUnCloseableWindow
{
  public ContainerA(Logic.GlobalEventHandler globalEventHandler) : base(globalEventHandler) => InitializeComponent();

  async void TopmostUnCloseableWindow_Loaded(object s, System.Windows.RoutedEventArgs e) => await Task.Delay(5 * 60 * 1000);
#if SelfCloseIn5min
    Close();
#endif

  void OnUnloaded(object s, RoutedEventArgs e) => App.LogScrSvrUptimeOncePerSession("ScrSvr - Dn - ContainerA.OnUnloaded(). ");
}
