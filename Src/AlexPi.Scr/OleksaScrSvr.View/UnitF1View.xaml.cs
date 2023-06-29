namespace OleksaScrSvr.View;

public partial class UnitF1View
{
  public UnitF1View()
  {
    InitializeComponent();
    MsgSlideshowUsrCtrl1.ClientId = ClientId; // new WpfUserControlLib.Helpers.ConfigRandomizer().GetRandomFromUserSection("ClientId");
  }

  public string? ClientId { get; set; }
}
