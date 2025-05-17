namespace OleksaScrSvr.View;

public partial class UnitF4View
{
  public UnitF4View() => InitializeComponent();
  void onShow(object s, MouseEventArgs e) { } // this takes 2% of CPU: must be colliding with the Begin in XAML:  try { ((Storyboard)FindResource("PeriodicFlash")).Begin(); } catch (Exception ex) { tbk1.Text = ex.Message; } }

  void OnMouseEnter(object sender, MouseEventArgs e) => WriteLine("MouseEnter"); //  Application.Current.Shutdown();     //Close();

  int count = 0;
  void OnMouseMove(object sender, MouseEventArgs e)
  {
    if (++count < 3)
    {
      WriteLine($"{DateTime.Now:HH:mm:ss.fff}  {count,2}   MouseEnter   <   3");
      Console.Beep(260 * count, 60);
      return;
    }

    WriteLine($"{DateTime.Now:HH:mm:ss.fff}  {count,2}   MouseEnter");
    Console.Beep(1260, 160);
    WriteLine($"{DateTime.Now:HH:mm:ss.fff}  {count,2}   MouseEnter   =>   Application.Current.Shutdown();     ");

    Application.Current.Shutdown();     //Close();
  }
}