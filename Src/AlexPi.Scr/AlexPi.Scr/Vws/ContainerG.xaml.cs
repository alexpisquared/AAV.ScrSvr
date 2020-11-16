using AAV.Sys.Helpers;
using MailInfoWpfUsrCtrlLib;
using System.Drawing;
using System.Windows.Controls;

namespace AAV.SS.Vws
{
  public partial class ContainerG : TopmostUnCloseableWindow
  {
    public ContainerG(AAV.SS.Logic.GlobalEventHandler globalEventHandler) : base(globalEventHandler)
    {
      InitializeComponent();

      if (VerHelper.IsVIP)
        contentControl1.Content = new MailInfoUserControl { Height = 130, Width = 130 };
      else
        contentControl1.Content = new Button { Content = "Hello" };
    }
  }
}