using System.Linq;
using OleksaScrSvr.View.Base;

namespace OleksaScrSvr.View;
public partial class Page01MultiUnitView : UserControl
{
  public Page01MultiUnitView()
  {
    InitializeComponent();

    try
    {
      if ((Debugger.IsAttached || DevOps.IsDbg))
      {
        Canvas1.Width = ScreenHelper.PrimaryScreen.Bounds.Width;
        Canvas1.Height = ScreenHelper.PrimaryScreen.Bounds.Height;
        WriteLine(tbkTitle.Text = $"Dbg mode: using PrimaryScreen with canvas {Canvas1.Width}x{Canvas1.Height}");
      }
      else
      {
        Canvas1.Width = ScreenHelper.GetSumOfAllBounds.Width;
        Canvas1.Height = ScreenHelper.GetSumOfAllBounds.Height;
        WriteLine(tbkTitle.Text = $"Rls mode: using GetSumOfAllBounds with canvas {Canvas1.Width}x{Canvas1.Height}");
      }
    }
    catch (Exception ex) { WriteLine(tbkTitle.Text = $"{ex.Message}"); }
  }

  void OnDragMove(object sender, MouseButtonEventArgs e)
  {
    if (e.LeftButton != MouseButtonState.Pressed) return;

    this.FindParentWindow().DragMove();

    e.Handled = true;
  }

  void OnResetPlacement(object sender, RoutedEventArgs e)
  {
    int i = 0, width = 260;
    foreach (var unitF in Canvas1.Children.OfType<UnitContainerBase>())
    {
      unitF.IsOpenState = false;
      Canvas.SetTop(unitF, 88);
      Canvas.SetLeft(unitF, width * i + 52);
      i++;
    }
  }
}