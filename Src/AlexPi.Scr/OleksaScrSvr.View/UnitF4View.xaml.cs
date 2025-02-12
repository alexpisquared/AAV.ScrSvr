namespace OleksaScrSvr.View;

public partial class UnitF4View
{
  public UnitF4View() => InitializeComponent();
  void onShow(object s, MouseEventArgs e) { } // this takes 2% of CPU: must be colliding with the Begin in XAML:  try { ((Storyboard)FindResource("PeriodicFlash")).Begin(); } catch (Exception ex) { tbk1.Text = ex.Message; } }
}