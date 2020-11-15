using AAV.Sys.Helpers;
using System;
using System.Diagnostics;
using System.Windows;

namespace AsLink.UI
{
    [Obsolete(@"Instead use C:\c\AsLink\Win32\AAV.WPF.Base.WindowBase.cs", true)]
    public static class WindowExt
    {
        [Obsolete(@"Instead use C:\c\AsLink\Win32\AAV.WPF.Base.WindowBase.cs", true)]
        public static void SaveToIsoStore(this Window w, double zoom = 1) { if (w.WindowState == WindowState.Normal) XmlIsoFileSerializer.Save<BoundsRestorer>(new BoundsRestorer(w, zoom), w.Name); }
        [Obsolete(@"Instead use C:\c\AsLink\Win32\AAV.WPF.Base.WindowBase.cs", true)]
        public static BoundsRestorer LoadFromIsoStore(this Window w) { try { return XmlIsoFileSerializer.Load<BoundsRestorer>(w.Name) as BoundsRestorer; } catch { return new BoundsRestorer(); } }
    }

    [Obsolete(@"Instead use C:\c\AsLink\Win32\AAV.WPF.Base.WindowBase.cs", true)]
    public class BoundsRestorer
    {
        public BoundsRestorer() { }
        public BoundsRestorer(Window w) : this(w, 1.0) { }
        public BoundsRestorer(Window w, double zoom)
        {
            WindowState = w.WindowState;
            Zoom = zoom;
            if (w.WindowState == System.Windows.WindowState.Normal)
            {
                Top = w.Top;
                Left = w.Left;
                Width = w.Width;
                Height = w.Height;
            }
        }

        public void SetWindowToThis(Window w)
        {
            w.Top = Top;
            w.Left = Left;
            w.Width = Width;
            w.Height = Height;
            w.WindowState = WindowState;
            try { ((dynamic)w).ZV = Zoom; }
            catch { Trace.WriteLine("ex:> ignore missing ZV propg."); } // ignore
        }

        public double Top = 240, Left = 474, Width = 971, Height = 600, Zoom = 1.15;
        public WindowState WindowState = WindowState.Normal;
    }
}
