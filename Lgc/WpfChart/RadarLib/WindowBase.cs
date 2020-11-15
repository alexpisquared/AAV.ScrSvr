using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace AsLink
{
  public partial class WindowBase : Window
  {
    string _filenameONLY => $"{GetType().Name}.xml";
    const int _swShowNormal = 1, _swShowMinimized = 2, _margin = 16;
    static int _left = _margin, __top = _margin;

    public WindowBase()
    {
      MouseLeftButtonDown += (s, e) => { try { DragMove(); } catch (Exception ex) { Debug.WriteLine(ex.Message); if (Debugger.IsAttached) Debugger.Break(); } };
      MouseWheel /*1*/ += (s, e) => { if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) return; ZV += (e.Delta * .001); e.Handled = true; Debug.WriteLine(Title =  $">>ZV:{ZV}"); }; //tu:
      KeyUp += (s, e) =>
      {
        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
          switch (e.Key)
          {
            default: break;
            case Key.OemPlus: ZV *= 1.1; break;
            case Key.OemMinus: ZV /= 1.1; break;
            case Key.D0: ZV = 1; break;
          }
        else
          switch (e.Key)
          {
            default: break;
            case Key.Escape: if (!_ignoreEscape) Close(); break;
          }
      };
    }

    protected bool _ignoreEscape = false;
    public static readonly DependencyProperty ZVProperty = DependencyProperty.Register("ZV", typeof(double), typeof(WindowBase), new PropertyMetadata(1d)); public double ZV { get => (double)GetValue(ZVProperty); set => SetValue(ZVProperty, value); }

    protected override void OnSourceInitialized(EventArgs e)
    {
      base.OnSourceInitialized(e);
      WindowPlacement wp;

      try
      {
        try
        {
          var wpc = XmlIsoFileSerializer.Load<WPContainer>(_filenameONLY);
          ZV = wpc.Zb == 0 ? 1 : wpc.Zb;
          wp = wpc.WindowPlacement;
        }
        catch (InvalidOperationException ex)
        {
          Debug.WriteLine(ex.Message);
          ZV = 1d;
          wp = XmlIsoFileSerializer.Load<WindowPlacement>(_filenameONLY);
        }
        catch (Exception ex) { Debug.WriteLine(ex.Message); throw; }

        wp.length = Marshal.SizeOf(typeof(WindowPlacement));
        wp.flags = 0;
        wp.showCmd = (wp.showCmd == _swShowMinimized ? _swShowNormal : wp.showCmd);

        if (wp.normalPosition.Bottom == 0 && wp.normalPosition.Top == 0 && wp.normalPosition.Left == 0 && wp.normalPosition.Right == 0)
        {
          _left = wp.normalPosition.Right = (wp.normalPosition.Left = _left) + (int)ActualWidth;
          wp.normalPosition.Bottom = (wp.normalPosition.Top = __top) + (int)ActualHeight;

          if (_left > 1700) // System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width) <== needs a ref to ..Forms.dll
          {
            _left = _margin;
            __top += (_margin + (int)ActualHeight);
          }
#if DEBUG
#else
          //wp.normalPosition.Top = 100; wp.normalPosition.Left = 200; wp.normalPosition.Bottom = 800; wp.normalPosition.Right = 1400;
#endif // DEBUG
        }

        SetWindowPlacement(new WindowInteropHelper(this).Handle, ref wp); //Note: if window was closed on a monitor that is now disconnected from the computer, SetWindowPlacement will place the window onto a visible monitor.
      }
      catch (InvalidOperationException ex) { Debug.WriteLine(ex.Message); if (Debugger.IsAttached) Debugger.Break(); } // ignored
      catch (Exception ex) { Debug.WriteLine(ex.Message); if (Debugger.IsAttached) Debugger.Break(); } // ignored
    }
    protected override void OnClosing(CancelEventArgs e) // WARNING - Not fired when Application.SessionEnding is fired
    {
      base.OnClosing(e);

      GetWindowPlacement(new WindowInteropHelper(this).Handle, out var wp);
      XmlIsoFileSerializer.Save(new WPContainer { WindowPlacement = wp, Zb = ZV }, _filenameONLY);
    }

    #region Win32 API declarations to set and get window placement
    [DllImport("user32.dll")] static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WindowPlacement lpwndpl);
    [DllImport("user32.dll")] static extern bool GetWindowPlacement(IntPtr hWnd, out WindowPlacement lpwndpl);

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct WindowPlacement
    {
      public int length;
      public int flags;
      public int showCmd;
      public Point minPosition;
      public Point maxPosition;
      public Rect normalPosition;
    }

    [Serializable] [StructLayout(LayoutKind.Sequential)] public struct Point { public int X, Y; public Point(int x, int y) { X = x; Y = y; } }
    [Serializable] [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left, Top, Right, Bottom; public Rect(int left, int top, int right, int bottom) { Left = left; Top = top; Right = right; Bottom = bottom; } } // RECT structure required by WINDOWPLACEMENT structure
    [Serializable] [StructLayout(LayoutKind.Sequential)] public struct WPContainer { public WindowPlacement WindowPlacement { get; set; } public double Zb { get; set; } }
    #endregion
  }
}