using System.Windows;
namespace AAV.SS { public partial class MiniScrSvrWindow : Window { public MiniScrSvrWindow() { InitializeComponent(); Closed += (s, e) => Application.Current.Shutdown(); } } }