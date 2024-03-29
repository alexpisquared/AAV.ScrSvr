﻿namespace OleksaScrSvr.View;

public partial class NavigationBar : UserControl
{
  public NavigationBar() => InitializeComponent();

  void OnDragMove(object s, MouseButtonEventArgs e)
  {
    if (e.LeftButton != MouseButtonState.Pressed) return;

    this.FindParentWindow().DragMove();

    e.Handled = true;
  }
}