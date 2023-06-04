namespace AlexPi.Scr.Unclosables;

public partial class TopmostUnCloseableWindow : UnCloseableWindow
{
  public TopmostUnCloseableWindow(GlobalEventHandler globalEventHandler) : base(globalEventHandler)
  {
    Topmost = true;
    IgnoreWindowPlacement = false;
  }

  protected async void onClose(object s, RoutedEventArgs e)
  {
    AppSettings.Instance.SaveIfDirty_TODO();
    new Bpr().AppFinish();
    Close();
    await App.SpeakAsync("Closing");
    await Task.Delay(512); // :Speak underestimates the time needed to speak the text.
    Application.Current.Shutdown();
  }
}
