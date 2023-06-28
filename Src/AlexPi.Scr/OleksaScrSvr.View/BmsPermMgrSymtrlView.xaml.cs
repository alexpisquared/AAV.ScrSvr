using WinFormsControlLib;

namespace OleksaScrSvr.View;
public partial class BmsPermMgrSymtrlView : UserControl
{
  public BmsPermMgrSymtrlView()
  {
    InitializeComponent();

    try
    {
      canvas.Width = WinFormHelper.GetSumOfAllBounds.Width;
      canvas.Height = WinFormHelper.GetSumOfAllBounds.Height;
      WriteLine(tbkTitle.Text = $"{Environment.MachineName}:   canvas.Width: {canvas.Width}, canvas.Height: {canvas.Height}");
    }
    catch (Exception ex)
    {
      WriteLine(tbkTitle.Text = $"{ex.Message}");
    }
  }
}