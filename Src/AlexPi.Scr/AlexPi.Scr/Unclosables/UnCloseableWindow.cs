namespace AlexPi.Scr.Unclosables;
[System.Runtime.Versioning.SupportedOSPlatform("windows")]

public partial class UnCloseableWindow : WpfUserControlLib.Base.WindowBase // <- black bacground bottommost window to save on heating.
{
  protected readonly GlobalEventHandler? _GlobalEventHandler;

  public IConfigurationRoot ConfigurationRoot { get; }
  public IBpr Bpr { get; }

  public UnCloseableWindow(GlobalEventHandler? globalEventHandler) : base()
  {
    IgnoreWindowPlacement = true;
    var minMouseMovePoints = 99;

    _GlobalEventHandler = globalEventHandler;

    PreviewMouseMove += async (s, e) => minMouseMovePoints = await ExitStrategy.CloseIfBigMoveBoforeGracePeriod(minMouseMovePoints, this, GetType().Name);

    Closed += UnCloseableWindow_Closed;
    Unloaded += UnCloseableWindow_Unloaded;

    PreviewKeyUp += async (s, e) =>
    {
      e.Handled = await ExitStrategy.CloseBasedOnPCName(e.Key, this);
      if (e.Handled)
      {
        //todo: is it redundant Application.Current.Shutdown(77);          Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - App.StartedAt):mm\\:ss\\.ff}   App.Shutdown(77) in PreviewKeyUp(). ");
      }
      else if(_GlobalEventHandler is not null)
        switch (e.Key)
        {
          case Key.F1: _GlobalEventHandler.TglContainerVis("ContainerA"); break;
          case Key.F2: _GlobalEventHandler.TglContainerVis("ContainerB"); break;
          case Key.F3: _GlobalEventHandler.TglContainerVis("ContainerC"); break;
          case Key.F4: _GlobalEventHandler.TglContainerVis("ContainerD"); break;
          case Key.F5: _GlobalEventHandler.TglContainerVis("ContainerE"); break;
          case Key.F6: _GlobalEventHandler.TglContainerVis("ContainerF"); break;
          case Key.F7: _GlobalEventHandler.TglContainerVis("ContainerG"); break;
          case Key.F8: _GlobalEventHandler.TglContainerVis("ContainerH"); break;
          case Key.F9: _GlobalEventHandler.TglContainerVis("ContainerI"); break;
          case Key.F10: _GlobalEventHandler.TglContainerVis("ContainerJ"); break;
          case Key.F11: _GlobalEventHandler.TglContainerVis("ContainerK"); break;
          case Key.F12: _GlobalEventHandler.TglContainerVis("ContainerL"); break;
          default: await App.SpeakAsync($"{e.Key} not handled!", ignoreBann: true); return;
        }
    };
  }

  public UnCloseableWindow(ILogger logger, IConfigurationRoot configurationRoot, IBpr bpr) : base(logger)
  {
    ConfigurationRoot = configurationRoot;
    Bpr = bpr;
  }

  protected async void onClose(object s, RoutedEventArgs e)
  {
    AppSettings.Instance.SaveIfDirty_TODO();
    Close();
    await new Bpr().AppFinishAsync();
    Application.Current.Shutdown();
  }

  void UnCloseableWindow_Unloaded(object s, RoutedEventArgs e) { }// => App.LogScrSvrUptimeOncePerSession($"ScrSvr - Dn - {GetType().Name}.UnCloseableWindow_Unloaded(). ");
  void UnCloseableWindow_Closed(object? s,  /**/  EventArgs e) /**/  => App.LogScrSvrUptimeOncePerSession($"ScrSvr - Dn - {GetType().Name}.UnCloseableWindow_Closed(). ");
}
