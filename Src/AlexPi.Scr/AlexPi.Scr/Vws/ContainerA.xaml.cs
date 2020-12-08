using System.Threading.Tasks;

namespace AlexPi.Scr.Vws
{
  public partial class ContainerA : TopmostUnCloseableWindow
  {
    public ContainerA(Logic.GlobalEventHandler globalEventHandler) : base(globalEventHandler) => InitializeComponent();

    async void TopmostUnCloseableWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
#if DEBUG
      await Task.Delay(10000);
#else
      await Task.Delay(5 * 60 * 1000);
#endif
      Close();
      await AltBpr.ChimerAlt.NoteA();
    }
  }
}
