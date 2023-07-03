namespace AlexPi.Scr.Vws;
public partial class MiniScrSvrWindow
{
  public MiniScrSvrWindow()
  {
    InitializeComponent();
    Closed += (s, e) => Application.Current.Shutdown(99);
  }
}