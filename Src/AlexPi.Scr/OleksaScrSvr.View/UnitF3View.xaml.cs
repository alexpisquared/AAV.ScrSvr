namespace OleksaScrSvr.View;

public partial class UnitF3View
{
  //TimeSpan _ts;
  readonly DateTime _idleStart;

  public UnitF3View()
  {
    InitializeComponent();
    _idleStart = DateTime.Now.AddMinutes(-4);
  }

  async void OnLoaded(object sender, RoutedEventArgs e)
  {
    await OnLoadedAsync();

  }
}