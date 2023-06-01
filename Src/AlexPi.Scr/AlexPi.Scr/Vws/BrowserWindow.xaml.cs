namespace AlexPi.Scr;

public partial class BrowserWindow 
{
  public BrowserWindow()
  {
    InitializeComponent();
    Closed += (s, e) => Application.Current.Shutdown(88);
    KeyUp += (s, e) =>
    {
      if (e.Key is Key.Escape or Key.Up)
      {
        Close();
        /*System.Diagnostics.Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} => ::>Application.Current.Shutdown();5");*/
        Application.Current.Shutdown();
      }
    };
  }
}
