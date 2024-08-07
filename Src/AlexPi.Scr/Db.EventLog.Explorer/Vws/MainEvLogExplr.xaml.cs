﻿using System.Runtime.InteropServices;

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
    KeyDown += (s, ves) =>
    {
      switch (ves.Key)
      {
        default: Trace.WriteLine($"{DateTime.Now:HH:mm:ss.fff} KeyDown:  {ves} \t Key:{ves.Key}"); break;
        case Key.Escape: { Close(); Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} => ::>Application.Current.Shutdown();p"); Application.Current.Shutdown(); } App.Current.Shutdown(); break;
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

      await Task.Delay(1_780_000); // close after 30 minutes - 20 seconds for smooth restart.
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
