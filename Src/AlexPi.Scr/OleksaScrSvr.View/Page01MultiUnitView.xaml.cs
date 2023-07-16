﻿namespace OleksaScrSvr.View;
public partial class Page01MultiUnitView : UserControl
{
  public Page01MultiUnitView()
  {
    InitializeComponent();

    try
    {
      if (Debugger.IsAttached || DevOps.IsDbg)
      {
        canvas.Width = WinFormsControlLib.WinFormHelper.PrimaryScreen.Bounds.Width;
        canvas.Height = WinFormsControlLib.WinFormHelper.PrimaryScreen.Bounds.Height;
      }
      else
      {
        canvas.Width = WinFormsControlLib.WinFormHelper.GetSumOfAllBounds.Width;
        canvas.Height = WinFormsControlLib.WinFormHelper.GetSumOfAllBounds.Height;
      }
      WriteLine(tbkTitle.Text = $"{Environment.MachineName}:   canvas.Width: {canvas.Width}, canvas.Height: {canvas.Height}");
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