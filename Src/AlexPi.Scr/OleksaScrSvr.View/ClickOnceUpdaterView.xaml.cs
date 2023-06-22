namespace OleksaScrSvr.Views;

public partial class ClickOnceUpdaterView : UserControl
{
  public ClickOnceUpdaterView()
  {
    InitializeComponent();

    //Loaded += async (s, e) => { await Task.Delay(1500)/*!!.ConfigureAwait(false)*/; _ = S.Focus(); };
  }
}
