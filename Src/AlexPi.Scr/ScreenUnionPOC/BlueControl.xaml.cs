namespace ScreenUnionPOC;

public partial class BlueControl : UserControl
{
  public BlueControl() => InitializeComponent();

  // Variables for tracking mouse movement and dragging state
  bool isDragging;
  System.Windows.Point clickPosition;

  // Event handler for mouse left button down on the border
  void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
  {
    isDragging = true;
    var draggableControl = sender as Border;
    clickPosition = e.GetPosition(this);
    _ = (draggableControl?.CaptureMouse());
    Panel.SetZIndex(this, 111);
  }

  // Event handler for mouse left button up on the border
  void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
  {
    isDragging = false;
    var draggable = sender as Border;
    draggable?.ReleaseMouseCapture();
    Panel.SetZIndex(this, 11);
  }

  // Event handler for mouse move on the border
  void Border_MouseMove(object sender, MouseEventArgs e)
  {
    if (isDragging && sender is Border draggableControl)
    {
      var currentPosition = e.GetPosition(Parent as UIElement);

      var canvas = ((FrameworkElement)draggableControl.Parent).Parent as Canvas;
      ArgumentNullException.ThrowIfNull(canvas, "▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄321▀▄▀▄▀▄▀▄▀▄▀▄▀▄");
      var canvasWidth = canvas.ActualWidth;
      var canvasHeight = canvas.ActualHeight;

      // Calculate the new position of the usercontrol
      var newX = currentPosition.X - clickPosition.X;
      var newY = currentPosition.Y - clickPosition.Y;

      // Check if the new position is within the canvas bounds
      if (newX < 0)
        newX = 0;
      if (newX + ActualWidth > canvasWidth)
        newX = canvasWidth - ActualWidth;
      if (newY < 0)
        newY = 0;
      if (newY + ActualHeight > canvasHeight)
        newY = canvasHeight - ActualHeight;

      // Set the new position of the usercontrol
      SetValue(Canvas.LeftProperty, newX);
      SetValue(Canvas.TopProperty, newY);
    }
  }
}
