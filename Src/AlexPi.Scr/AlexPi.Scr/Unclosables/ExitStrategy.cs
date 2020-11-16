using AlexPi.Scr.Logic;
using AsLink;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace AlexPi.Scr.Vws
{
  public static class ExitStrategy
  {
    public static int CloseIfBigMoveBoforeGracePeriod(int minMaouseMovePoints, Window wdw, string typeName)
    {
      if ((DateTime.Now - App.Started).TotalSeconds < App.GraceEvLogAndLockPeriodSec) // ignore mouse moves after the grace period (to adjust layout of windows and such).
      {
        Trace.WriteLineIf(App.CurTraceLevel.TraceWarning, $"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - App.Started):mm\\:ss\\.ff}    MouseMove #{minMaouseMovePoints,4} in {typeName}.");
        if (--minMaouseMovePoints < 0)
        {
          ExitStrategy.CloseBasedOnPCName(Key.Escape, wdw);
        }
      }

      return minMaouseMovePoints;
    }

    public static bool CloseBasedOnPCName(Key key, Window window)
    {
      Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - App.Started):mm\\:ss\\.ff}    PcLocationBased(key:{key}, window:{window.Name})");

      if (Key.F1 <= key && key < Key.DeadCharProcessed || // all special keys, like: alt, ctrl, shift, oem*
        key == Key.Tab ||
        key == Key.Left || key == Key.Right)
        return false;                                     // keep scrsvr on.

      switch (Environment.MachineName)
      {
        case "RAZER1":  
        case "ASUS2":       /**/ App.SpeakAsync($"Home PCs.");                  /**/return closeScrSvrSansLocking(window);  // mainPC - assuming always at home: no need to lock.
        case "CA03-APIGID": /**/ App.SpeakAsync($"Office.");                    /**/return BackDoor_Minuted(key, window);   // office - locking
        default:            /**/ App.SpeakAsync($"{Environment.MachineName}");  /**/return SpaceUpEscapOnly(key, window);   // others - space + escape + up
      }
    }
    public static bool BackDoor_Minuted(Key key, Window window)
    {
      var min10 = DateTime.Now.Minute % 10;
      var kmn10 = key - (key < Key.NumPad0 ? Key.D0 : Key.NumPad0);
      if (Math.Abs(min10 - kmn10) <= 1)
        closeScrSvrSansLocking(window);
      else
        lockPc_ThenCloseScrSvr(window);

      return true;
    }
    public static void BackDoor_Houred_(Key key, Window window)
    {
      var h = DateTime.Now.Hour;
      h = (h <= 12 ? h : h - 12) % 10;
      if (key - Key.D0 == h || key - Key.NumPad0 == h)
        closeScrSvrSansLocking(window);
      else
        lockPc_ThenCloseScrSvr(window);
    }
    public static void BackDoor_Fixed__(Key key, Window window)
    {
      switch (key)
      {
        default: lockPc_ThenCloseScrSvr(window); break;
        case Key.I:
        case Key.O:
        case Key.P:
        case Key.K:
        case Key.L:
        case Key.OemSemicolon:
        case Key.OemComma:
        case Key.OemPeriod: closeScrSvrSansLocking(window); break;
      }
    }
    public static bool SpaceUpEscapOnly(Key key, Window window)
    {
      switch (key)
      {
        case Key.Space:
        case Key.Up:
        case Key.Escape: closeScrSvrSansLocking(window); break;
        default: App.SpeakAsync($"Nice try."); break;
      }

      return true;
    }

    static bool lockPc_ThenCloseScrSvr(Window window)
    {
      bool keyUpHandled;
      if ((DateTime.Now - App.Started).TotalSeconds < App.GraceEvLogAndLockPeriodSec)
      {
        App.SpeakAsync($"Right on time.");
        keyUpHandled = false;
      }
      else
      {
        App.LockWorkStation();
        App.SpeakAsync($"Nice try.");
        keyUpHandled = true;
      }

      closeScrSvrSansLocking(window);

      return keyUpHandled;
    }
    static bool closeScrSvrSansLocking(Window window) { window.Close(); Thread.Sleep(333); Application.Current.Shutdown(); return true; }
  }
}
