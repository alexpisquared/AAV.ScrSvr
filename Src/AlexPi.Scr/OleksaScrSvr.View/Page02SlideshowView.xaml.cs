namespace OleksaScrSvr.View;
public partial class Page02SlideshowView : UserControl
{
  public Page02SlideshowView() => InitializeComponent();
  void OnLoaded(object s, RoutedEventArgs e) { /*_ = cbxApps.Focus();*/ }
  void OnDragMove(object sender, MouseButtonEventArgs e)
  {
    if (e.LeftButton != MouseButtonState.Pressed) return;

    this.FindParentWindow().DragMove();

    e.Handled = true;
  }
}