﻿namespace OleksaScrSvr.View;

public partial class UnitF1View
{
  public UnitF1View() => InitializeComponent();
  public static readonly DependencyProperty ClientIdProperty = DependencyProperty.Register("ClientId", typeof(string), typeof(UnitF1View)); public string ClientId { get => (string)GetValue(ClientIdProperty); set => SetValue(ClientIdProperty, value); }

  new void OnLoaded(object sender, RoutedEventArgs e)
  {
    base.OnLoaded(sender, e);

    MsgSlideshowUsrCtrl1.ClientId = ClientId; // new WpfUserControlLib.Helpers.ConfigRandomizer().GetRandomFromUserSection("ClientId");
    MsgSlideshowUsrCtrl1.ScaleToHalf = true;
  }
}
