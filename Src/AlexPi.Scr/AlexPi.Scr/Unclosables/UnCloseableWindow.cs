﻿using AlexPi.Scr.Logic;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace AlexPi.Scr.Vws
{
  public partial class UnCloseableWindow : AAV.WPF.Base.WindowBase // <- black bacground bottommost window to save on heating.
  {
    protected readonly GlobalEventHandler _GlobalEventHandler;

    public UnCloseableWindow(GlobalEventHandler globalEventHandler) : base()
    {
      IgnoreWindowPlacement = true;
      var minMouseMovePoints = 99;

      _GlobalEventHandler = globalEventHandler;

      PreviewMouseMove += async (s, e) => minMouseMovePoints = await ExitStrategy.CloseIfBigMoveBoforeGracePeriod(minMouseMovePoints, this, GetType().Name);

      //Closing += UnCloseableWindow_Closing;
      Closed += UnCloseableWindow_Closed;

      PreviewKeyUp += async (s, e) =>
      {
        e.Handled = await ExitStrategy.CloseBasedOnPCName(e.Key, this);
        if (e.Handled)
        {
          //todo: is it redundant Application.Current.Shutdown(77);          Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - App.StartedAt):mm\\:ss\\.ff}   App.Shutdown(77) in PreviewKeyUp(). ");
        }
        else
        {
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
            default:
              App.StopSpeakingAsync();
              App.SpeakFaF($"{e.Key} not handled!"); return;
          }
        }
      };
    }

    //void UnCloseableWindow_Closing(object s, CancelEventArgs e) => Trace.WriteLine($"Closing..  {GetType().FullName}");
    void UnCloseableWindow_Closed(object s, EventArgs eee) { } // Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - App.StartedAt):mm\\:ss\\.ff}   Closed ..  {GetType().FullName}");
  }
}
