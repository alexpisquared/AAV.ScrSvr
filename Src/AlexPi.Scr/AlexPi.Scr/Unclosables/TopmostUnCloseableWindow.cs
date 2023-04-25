namespace AlexPi.Scr.Unclosables;

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
