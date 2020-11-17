using AlexPi.Scr.Logic;
using System.Windows.Input;

namespace AlexPi.Scr.Vws
{
  public partial class UnCloseableWindow : AAV.WPF.Base.WindowBase // <- black bacground bottommost window to save on heating.
  {
    protected readonly GlobalEventHandler _GlobalEventHandler;

    public UnCloseableWindow(GlobalEventHandler globalEventHandler) : base()
    {
      IgnoreWindowPlacement = true;

      _GlobalEventHandler = globalEventHandler;

      var minMouseMovePoints = 99;
      PreviewMouseMove += (s, e) => minMouseMovePoints = ExitStrategy.CloseIfBigMoveBoforeGracePeriod(minMouseMovePoints, this, GetType().Name);

      PreviewKeyUp += (s, e) =>
      {
        e.Handled = ExitStrategy.CloseBasedOnPCName(e.Key, this);
        if (e.Handled)
          return;

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
          default: App.SpeakAsync($"{e.Key} not handled!"); return;
        }
      };
    }
  }
}
