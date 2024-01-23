using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PostScrSvr;
public partial class MainWindow : Window
{
  public string videoFilename => @"C:\Users\alexp\OneDrive" + ((VideoLogEntry)dg1.SelectedItem).VideoFilename;

  public MainWindow() => InitializeComponent();

  async void OnLoadedAsync(object s, RoutedEventArgs e)
  {
    var list = await File.ReadAllLinesAsync("""C:\Users\alexp\OneDrive\Public\Logs\OleksaScrSvr.Video.log""");
    var entries = list.Select(x => x.Split('\t')).Select(x => new VideoLogEntry(
      DateTimeOffset.Parse(x[0]),
      DateTimeOffset.Parse(x[1]),
      int.Parse(x[2]),
      x[3],
      x[4],
      x[5],
      x[6])).ToList();

    dg1.ItemsSource = entries.OrderByDescending(r => r.Displayed);
  }

  void dg1_SelectionChanged(object s, SelectionChangedEventArgs e)
  {
    mediaElement1.Source = new Uri(videoFilename); // mediaElement1.Source = new Uri(((VideoLogEntry)dg1.SelectedItem).ThumbnailUrl);
  }

  void OnOpenPath(object s, RoutedEventArgs e) => _ = Process.Start("Explorer.exe", $"/select, \"{videoFilename}\"");

  void OnExit(object s, RoutedEventArgs e) => Close();

  void OnThumbMouseUp(object s, System.Windows.Input.MouseButtonEventArgs e) { }

  void OnThumbMouseEnter(object s, System.Windows.Input.MouseEventArgs e) { Title = $"{((Image)s).Tag} mb   {((Image)s).ToolTip}"; ; }
}

public record VideoLogEntry(DateTimeOffset Displayed, DateTimeOffset Created, int SizeMb, string SizeColumn, string Http, string VideoFilename, string ThumbnailUrl);
