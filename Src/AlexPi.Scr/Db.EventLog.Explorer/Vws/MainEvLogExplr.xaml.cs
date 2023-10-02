namespace Db.EventLog.Explorer;

public partial class MainEvLogExplr 
{
  public MainEvLogExplr()
  {
    InitializeComponent();
    KeyDown += (s, ves) =>
    {
      switch (ves.Key)
      {
        default: System.Diagnostics.Trace.WriteLine($"::>>{ves} \t {ves.Key}"); break;
        case Key.Escape: { Close(); System.Diagnostics.Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} => ::>Application.Current.Shutdown();p"); Application.Current.Shutdown(); } App.Current.Shutdown(); break;
      }
    };
  }

  void onLoaded(object s, RoutedEventArgs e)
  {
    vizroot.IsEnabled = false;
    try
    {
      //tbCurVer.Text = $"{VersionHelper.CurVerStr("")}";
    }
    catch (Exception ex) { ex.Pop(); ; }
    finally { vizroot.IsEnabled = true; }
  }
  async void Window_SizeChanged(object s, SizeChangedEventArgs e)
  {
    var lastSize = e.NewSize;
    await Task.Delay(333);
    if (lastSize.Width == e.NewSize.Width) // if finished resizing.
      stuc.RedrawOnResize_todo(s, null);
  }
}
