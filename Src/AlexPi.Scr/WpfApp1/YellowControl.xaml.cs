using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfApp1
{
  public partial class YellowControl : UserControl
  {
    public YellowControl()
    {
      InitializeComponent();
    }

    // Variables for tracking mouse movement and dragging state
    private bool isDragging;
    private Point clickPosition;

    // Event handler for mouse left button down on the border
    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      isDragging = true;
      var draggableControl = sender as Border;
      clickPosition = e.GetPosition(this);
      draggableControl.CaptureMouse();
    }

    // Event handler for mouse left button up on the border
    private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      isDragging = false;
      var draggable = sender as Border;
      draggable.ReleaseMouseCapture();
    }

    // Event handler for mouse move on the border
    private void Border_MouseMove(object sender, MouseEventArgs e)
    {
      var draggableControl = sender as Border;
      if (isDragging && draggableControl != null)
      {
        Point currentPosition = e.GetPosition(this.Parent as UIElement);

        // Get the parent canvas and its size
        var canvas = draggableControl.Parent as Canvas;
        double canvasWidth = canvas.ActualWidth;
        double canvasHeight = canvas.ActualHeight;

        // Calculate the new position of the usercontrol
        double newX = currentPosition.X - clickPosition.X;
        double newY = currentPosition.Y - clickPosition.Y;

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
  }
}
