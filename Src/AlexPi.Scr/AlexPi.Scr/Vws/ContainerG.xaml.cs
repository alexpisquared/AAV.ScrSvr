namespace AlexPi.Scr.Vws;

public partial class ContainerG : TopmostUnCloseableWindow
{
  public ContainerG(GlobalEventHandler globalEventHandler) : base(globalEventHandler)
  {
    InitializeComponent();

    contentControl1.Content = new MailInfoWpfUsrCtrlLib.MailInfoUserControl { Height = 130, Width = 130 }; // new Button { Height = 130, Width = 130, Content = "No Mail for the masses :(" };
  }
}