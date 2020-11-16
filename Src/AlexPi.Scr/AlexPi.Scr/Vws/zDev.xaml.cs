using System.Windows;
using System.Windows.Input;

namespace AAV.SS
{
    /// <summary>
    /// Interaction logic for zDev.xaml
    /// </summary>
    public partial class zDev : Window
    {
        public zDev()
        {
            InitializeComponent();
            KeyDown += (s, e) => { if (e.Key == Key.Escape || e.Key == Key.Up) { { Close(); /*System.Diagnostics.Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} => ::>Application.Current.Shutdown();e");*/ Application.Current.Shutdown(); } } }; MouseLeftButtonDown += new MouseButtonEventHandler((s, e) => { DragMove(); }); //tu:
            DataContext = this;
        }
    }
}
