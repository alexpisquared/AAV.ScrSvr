namespace OleksaScrSvr.Views;
public partial class BmsPermMgrSymtr2View : UserControl
{
  public BmsPermMgrSymtr2View() => InitializeComponent();
  void OnLoaded(object s, RoutedEventArgs e) { _ = cbxApps.Focus(); }
}