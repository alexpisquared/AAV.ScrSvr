namespace WpfApp1;

public partial class YellowControl : UserControl
{
  public YellowControl() => InitializeComponent();

  bool isDragging, _isResizing;
  System.Windows.Point _lastMousePosition, clickPosition;

  void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
  {
    isDragging = true;
    clickPosition = e.GetPosition(this);
    _ = ((sender as Border)?.CaptureMouse());
    Panel.SetZIndex(this, 111);
  }
  void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
  {
    isDragging = false;
    (sender as Border)?.ReleaseMouseCapture();
    Panel.SetZIndex(this, 11);
  }
  void Border_MouseMove(object sender, MouseEventArgs e)
  {
    if (isDragging && sender is Border draggableControl)
    {
      var currentPosition = e.GetPosition(this.Parent as UIElement);

      var canvas = ((System.Windows.FrameworkElement)((FrameworkElement)draggableControl.Parent).Parent).Parent as Canvas;

      ArgumentNullException.ThrowIfNull(canvas, "▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄321▀▄▀▄▀▄▀▄▀▄▀▄▀▄");

      var canvasWidth = canvas.ActualWidth ;
      var canvasHeight = canvas.ActualHeight ;

      // Calculate the new position of the usercontrol
      var newX = currentPosition.X - clickPosition.X;
      var newY = currentPosition.Y - clickPosition.Y;

      // Check if the new position is within the canvas bounds
      if (newX < 0)
        newX = 0;
      if (newX + this.ActualWidth > canvasWidth)
        newX = canvasWidth - this.ActualWidth;
      if (newY < 0)
        newY = 0;
      if (newY + this.ActualHeight > canvasHeight)
        newY = canvasHeight - this.ActualHeight;

      // Set the new position of the usercontrol
      this.SetValue(Canvas.LeftProperty, newX);
      this.SetValue(Canvas.TopProperty, newY);
    }
  }


 
  private void Rectng_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
  {
    _isResizing = true;
    _lastMousePosition = e.GetPosition(this);
    Mouse.Capture((IInputElement)sender);
  }

  private void Rectng_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
  {
    _isResizing = false;
    Mouse.Capture(null);
  }

  private void Rectng_MouseMove(object sender, MouseEventArgs e)
  {
    if (_isResizing)
    {
      var currentPosition = e.GetPosition(this);
      var delta = currentPosition - _lastMousePosition;
      Width += delta.X;
      Height += delta.Y;
      _lastMousePosition = currentPosition;
    }
  }
}
