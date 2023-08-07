namespace OleksaScrSvr.View;

public partial class UnitF4View
{
  public UnitF4View() => InitializeComponent();
  void onShow(object s, MouseEventArgs e) { try { ((Storyboard)FindResource("PeriodicFlash")).Begin(); } catch (Exception ex) { tbk1.Text = ex.Message; } }
}