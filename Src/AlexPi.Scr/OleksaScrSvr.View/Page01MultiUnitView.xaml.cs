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
        canvas.Width = ScreenHelper.PrimaryScreen.Bounds.Width;
        canvas.Height = ScreenHelper.PrimaryScreen.Bounds.Height;
        WriteLine(tbkTitle.Text = $" PrimaryScreen:       canvas {canvas.Width} * {canvas.Height}");
      }
      else
      {
        canvas.Width = ScreenHelper.GetSumOfAllBounds.Width;
        canvas.Height = ScreenHelper.GetSumOfAllBounds.Height;
        WriteLine(tbkTitle.Text = $" GetSumOfAllBounds:   canvas {canvas.Width} * {canvas.Height}");
      }
    }
    catch (Exception ex)
    {
      WriteLine(tbkTitle.Text = $"{ex.Message}");
    }
  }

  private void OnDragMove(object sender, MouseButtonEventArgs e)
  {
    if (e.LeftButton != MouseButtonState.Pressed) return;

    this.FindParentWindow().DragMove();

    e.Handled = true;
  }
}