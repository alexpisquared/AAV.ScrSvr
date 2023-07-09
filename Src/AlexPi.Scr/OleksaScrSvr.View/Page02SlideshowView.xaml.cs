namespace OleksaScrSvr.View;
public partial class Page02SlideshowView : UserControl
{
  public Page02SlideshowView() => InitializeComponent();

  void OnDragMove(object s, MouseButtonEventArgs e)
  {
    if (e.LeftButton != MouseButtonState.Pressed) return;

    this.FindParentWindow().DragMove();

    e.Handled = true;
  }
}