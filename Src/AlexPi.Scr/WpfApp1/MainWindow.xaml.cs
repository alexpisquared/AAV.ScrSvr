using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
      // Load the saved locations of the usercontrols
      yellowControl.SetValue(Canvas.LeftProperty, Properties.Settings.Default.YellowLeft);
      yellowControl.SetValue(Canvas.TopProperty, Properties.Settings.Default.YellowTop);
      blueControl.SetValue(Canvas.LeftProperty, Properties.Settings.Default.BlueLeft);
      blueControl.SetValue(Canvas.TopProperty, Properties.Settings.Default.BlueTop);
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      // Save the current locations of the usercontrols
      Properties.Settings.Default.YellowLeft = (double)yellowControl.GetValue(Canvas.LeftProperty);
      Properties.Settings.Default.YellowTop = (double)yellowControl.GetValue(Canvas.TopProperty);
      Properties.Settings.Default.BlueLeft = (double)blueControl.GetValue(Canvas.LeftProperty);
      Properties.Settings.Default.BlueTop = (double)blueControl.GetValue(Canvas.TopProperty);
      Properties.Settings.Default.Save();
    }
  }
}
