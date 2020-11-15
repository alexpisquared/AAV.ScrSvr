using System.Windows;
using System.Windows.Input;

namespace AAV.SS
{
    public partial class BrowserWindow : Window
    {
        public BrowserWindow()
        {
            InitializeComponent();
            Closed += (s, e) => Application.Current.Shutdown();
            KeyUp += (s, e) =>
            {
                if (e.Key == Key.Escape || e.Key == Key.Up)
                { Close (); /*System.Diagnostics.Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} => ::>Application.Current.Shutdown();5");*/ Application.Current.Shutdown(); }
            };
        }
    }
}
