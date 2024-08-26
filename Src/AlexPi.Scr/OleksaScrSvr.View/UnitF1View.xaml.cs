namespace OleksaScrSvr.View;

public partial class UnitF1View
{
  public UnitF1View() => InitializeComponent();
  public static readonly DependencyProperty ClientIdProperty = DependencyProperty.Register("ClientId", typeof(string), typeof(UnitF1View)); public string ClientId { get => (string)GetValue(ClientIdProperty); set => SetValue(ClientIdProperty, value); }
  public static readonly DependencyProperty ClientSecretProperty = DependencyProperty.Register("ClientSecret", typeof(string), typeof(UnitF1View)); public string ClientSecret { get => (string)GetValue(ClientSecretProperty); set => SetValue(ClientSecretProperty, value); }

  new void OnLoaded(object s, RoutedEventArgs e)
  {
    base.OnLoadedBase(s, e);
  }
}
