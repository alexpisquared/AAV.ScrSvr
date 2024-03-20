using System.Windows.Controls;
using AmbienceLib;

namespace PostScrSvr;
public partial class MainWindow
{
  List<VideoLogEntry>? entries;
  VideoLogEntry? _selectedVLE;

  public string VideoFilename
  {
    get {
      var r = dg1.SelectedItem == null
        ? OneDrive.Folder(@"Pictures\id.png")
        : OneDrive.Folder(((VideoLogEntry)dg1.SelectedItem).VideoFilename);

      Trace.WriteLine($"** {r}");
      return r;
    }
  }

  public MainWindow() => InitializeComponent();

  async void OnLoadedAsync(object s, RoutedEventArgs e)
  {
    new Bpr().AppStart();

    const int showAndKeepOnlyLastLines = 64;

    var lines = await File.ReadAllLinesAsync(OneDrive.Folder(@"Public\Logs\OleksaScrSvr.Video.log"));
    await File.WriteAllLinesAsync(OneDrive.Folder(@"Public\Logs\OleksaScrSvr.Video.log"), lines.TakeLast(showAndKeepOnlyLastLines));

    entries = lines.Where(line => line.Length > 226).Select(line => line.Split('\t')).Select(SafeCreateRecord).ToList();

    dg1.ItemsSource = entries.OrderByDescending(r => r.Displayed).Take(showAndKeepOnlyLastLines);

    lblInfo.Content = $"Last {showAndKeepOnlyLastLines} / {entries.Count} viewings";
  }

  static VideoLogEntry SafeCreateRecord(string[] x)
  {
    try
    {
      return new VideoLogEntry(DateTimeOffset.Parse(x[0]), DateTimeOffset.Parse(x[1]), int.Parse(x[2]), x[3], x[4], x[5], x[6]);
    }
    catch (Exception ex)
    {
      Trace.WriteLine($"** {ex.Message}");
      return new VideoLogEntry(DateTimeOffset.Now, DateTimeOffset.Now, 0, "·", ex.Message, "\\Pictures\\Main\\2016\\2012-05-04 - 2016-04-23 iPod\\IMG_6015.JPG", "https://dsm04pap002files.storage.live.com/y4mp2lNt4HhzhVmtpgKh-uPicy49p3x877F1fVep38LbXGcTaUMKtFE52eDbBNVfEDN1Z-nKLPtYQb8mX8N3aiOd-MvALZO8npVEgZQxyw7B_8QCbaCfezULNtl-0WsHF6ugr7wvBqb1txEsZhXDBVC0gS0UlCnyLaEyfgHyGFFx3fjmlLM_niMJ5mqC768WI6AJEm5e9PtS0jagp83HwKmi-2NZVCqWccIn3I96JTHyuU?width=800&height=598&cropmode=none");
    }
  }

  void dg1_SelectionChanged(object s, SelectionChangedEventArgs e)
  {
    _selectedVLE = ((VideoLogEntry)dg1.SelectedItem);
    mediaElement1.Source = new Uri(_selectedVLE.ThumbnailUrl);
    lblInfo.Content = $"{_selectedVLE.SizeMb} mb   {_selectedVLE.VideoFilename}";
  }
  void OnOpenPath(object s, RoutedEventArgs e) => _ = Process.Start("Explorer.exe", $"/select, \"{VideoFilename}\"");
  void OnThumbMouseUp(object s, System.Windows.Input.MouseButtonEventArgs e) { }
  void OnThumbMouseEnter(object s, System.Windows.Input.MouseEventArgs e) { lblInfo.Content = $"{((Image)s).Tag} mb   {((Image)s).ToolTip}"; ; }
  void OnPlayStart(object s, RoutedEventArgs e) { mediaElement1.Source = new Uri(VideoFilename); ; }
  void OnDblClick(object s, System.Windows.Input.MouseButtonEventArgs e) { OnPlayStart(s, e); ; }
  void OnAll(object sender, RoutedEventArgs e) => dg1.ItemsSource = entries.OrderByDescending(r => r.Displayed);
  void OnTop(object sender, RoutedEventArgs e) => dg1.ItemsSource = entries.OrderByDescending(r => r.Displayed).Take(8);
  void OnBig(object sender, RoutedEventArgs e) => dg1.ItemsSource = entries.Where(r => r.SizeMb > 300).OrderByDescending(r => r.SizeMb);
  async void OnExit(object s, RoutedEventArgs e) { Hide(); await new Bpr().AppFinishAsync(); Close(); }
  void OnCopyThumb(object sender, RoutedEventArgs e)
  {
    if (_selectedVLE is not null)
    {
      Clipboard.SetText(_selectedVLE.ThumbnailUrl);
      lblInfo.Content = $"Thumbnail URL copied!";
    }
  }
}

public record VideoLogEntry(DateTimeOffset Displayed, DateTimeOffset Created, int SizeMb, string SizeColumn, string Http, string VideoFilename, string ThumbnailUrl);

/// Thumbnail URLs are temporary and expire after a few hours.  Mar 2024.