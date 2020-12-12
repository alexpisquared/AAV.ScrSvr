using AAV.Sys.Helpers;
using MailInfoWpfUsrCtrlLib;
using System.Drawing;
using System.Windows.Controls;

namespace AlexPi.Scr.Vws
{
  public partial class ContainerG : TopmostUnCloseableWindow
  {
    public ContainerG(AlexPi.Scr.Logic.GlobalEventHandler globalEventHandler) : base(globalEventHandler)
    {
      InitializeComponent();

      contentControl1.Content = VerHelper.IsVIP
          ? new MailInfoUserControl { Height = 130, Width = 130 }
          : (object)new Button { Height = 130, Width = 130, Content = "No Mail for the masses :(" };
    }
  }
}