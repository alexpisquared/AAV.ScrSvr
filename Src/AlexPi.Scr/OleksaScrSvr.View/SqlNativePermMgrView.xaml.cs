namespace OleksaScrSvr.Views;

public partial class SqlNativePermMgrView : UserControl
{
  public SqlNativePermMgrView()
  {
    InitializeComponent();

    Loaded += async (s, e) => { await Task.Delay(1500)/*!!.ConfigureAwait(false)*/; _ = S.Focus(); };
  }
}
