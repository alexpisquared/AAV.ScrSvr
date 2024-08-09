using System.Runtime.InteropServices;

namespace Db.EventLog.Explorer;

public partial class MainEvLogExplr
{
  [DllImport("user32.dll")][return: MarshalAs(UnmanagedType.Bool)] static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
  static readonly IntPtr HWND_BOTTOM = new(1);
  const uint SWP_NOSIZE = 0x0001;
  const uint SWP_NOMOVE = 0x0002;
  const uint SWP_NOACTIVATE = 0x0010;

  public MainEvLogExplr()
  {
    InitializeComponent();
    KeyDown += (s, keyEventArgs) =>
    {
      Trace.Write($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.f}  KeyDown:  Key:{keyEventArgs.Key}  ");
      switch (keyEventArgs.Key)
      {
        default: Trace.WriteLine(""); break;
        case Key.Escape: { Close(); Trace.WriteLine($"=> Application.Current.Shutdown();"); Application.Current.Shutdown(); } App.Current.Shutdown(); break;
      }
    };
  }

  async void OnLoaded(object s, RoutedEventArgs e)
  {
    vizroot.IsEnabled = false;
    try
    {
      tbInfo.Text = tbCurVer.Text = $"{DateTime.Now:HH:mm}";

      if (Environment.CommandLine.Contains("Schedule") == false) return;

      await Task.Delay(1_000); // wait for showing up first.
      SendToBack();

      vizroot.IsEnabled = true;

      await Task.Delay(1_740_000); // close after 29 minutes.
      Close();
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

  void SendToBack() { SetWindowPos(new System.Windows.Interop.WindowInteropHelper(this).Handle, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE); }
}
