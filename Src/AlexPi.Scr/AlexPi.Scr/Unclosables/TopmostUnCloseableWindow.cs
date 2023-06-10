namespace AlexPi.Scr.Unclosables;

public partial class TopmostUnCloseableWindow : UnCloseableWindow
{
  public TopmostUnCloseableWindow(GlobalEventHandler globalEventHandler) : base(globalEventHandler)
  {
    Topmost = true;
    IgnoreWindowPlacement = false;
  }
}
