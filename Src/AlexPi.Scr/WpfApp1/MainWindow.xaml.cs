using System;
using System.ComponentModel;
using System.Configuration;
using System.Windows;

namespace WpfApp1
{
  public partial class MainWindow : Window, INotifyPropertyChanged
  {
    public MainWindow()
    {
      InitializeComponent();
      DataContext = this;
      // Load the saved locations from app settings
      YellowX = Convert.ToDouble(ConfigurationManager.AppSettings["YellowX"]);
      YellowY = Convert.ToDouble(ConfigurationManager.AppSettings["YellowY"]);
      BlueX = Convert.ToDouble(ConfigurationManager.AppSettings["BlueX"]);
      BlueY = Convert.ToDouble(ConfigurationManager.AppSettings["BlueY"]);
    }

    // Properties for binding the usercontrols' locations
    private double _yellowX;
    public double YellowX
    {
      get { return _yellowX; }
      set { _yellowX = value; OnPropertyChanged("YellowX"); }
    }

    private double _yellowY;
    public double YellowY
    {
      get { return _yellowY; }
      set { _yellowY = value; OnPropertyChanged("YellowY"); }
    }

    private double _blueX;
    public double BlueX
    {
      get { return _blueX; }
      set { _blueX = value; OnPropertyChanged("BlueX"); }
    }

    private double _blueY;
    public double BlueY
    {
      get { return _blueY; }
      set { _blueY = value; OnPropertyChanged("BlueY"); }
    }

    // Event handler for saving the locations to app settings on window closing
    private void Window_Closing(object sender, CancelEventArgs e)
    {
      Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
      config.AppSettings.Settings["YellowX"].Value = YellowX.ToString();
      config.AppSettings.Settings["YellowY"].Value = YellowY.ToString();
      config.AppSettings.Settings["BlueX"].Value = BlueX.ToString();
      config.AppSettings.Settings["BlueY"].Value = BlueY.ToString();
      config.Save(ConfigurationSaveMode.Modified);
      ConfigurationManager.RefreshSection("appSettings");
    }

    // Implementation of INotifyPropertyChanged interface
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
  }
}
