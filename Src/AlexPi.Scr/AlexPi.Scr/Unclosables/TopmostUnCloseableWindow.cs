using AlexPi.Scr.Logic;
using AlexPi.Scr.Unclosables;
using AsLink;
using System.Windows;

namespace AlexPi.Scr.Vws
{
  public partial class TopmostUnCloseableWindow : UnCloseableWindow 
  {
    public TopmostUnCloseableWindow(GlobalEventHandler globalEventHandler) : base(globalEventHandler)
    {
      Topmost = true;
      IgnoreWindowPlacement = false;
    }

    protected void onClose(object s, RoutedEventArgs e)
    {
      AppSettings.Instance.SaveIfDirty_TODO();
      Close();
    }
  }
}
