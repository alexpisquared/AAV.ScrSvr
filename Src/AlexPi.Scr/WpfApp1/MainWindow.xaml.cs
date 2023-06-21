namespace WpfApp1;

public partial class MainWindow
{
  public MainWindow()
  {
    InitializeComponent();
    DataContext = string.IsNullOrEmpty(Settings.Default.VM) ? new VM() : JsonSerializer.Deserialize<VM>(Settings.Default.VM);
  }
  void Window_Closing(object sender, CancelEventArgs e)
  {
    Settings.Default.VM = JsonSerializer.Serialize(DataContext);
    Settings.Default.Save();
  }

  void OnClose(object sender, RoutedEventArgs e) => Close();
}
