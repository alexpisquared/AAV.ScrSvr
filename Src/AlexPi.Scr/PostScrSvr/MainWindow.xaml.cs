﻿using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PostScrSvr;
public partial class MainWindow : Window
{
  public string videoFilename
  {
    get {
      Trace.WriteLine($"** {((VideoLogEntry)dg1.SelectedItem).VideoFilename}");
      return dg1.SelectedItem == null
        ? @"C:\Users\alexp\OneDrive\Pictures\id.png"
        : @"C:\Users\alexp\OneDrive" + ((VideoLogEntry)dg1.SelectedItem).VideoFilename;
    }
  }

  public MainWindow() => InitializeComponent();

  async void OnLoadedAsync(object s, RoutedEventArgs e)
  {
    var list = await File.ReadAllLinesAsync("""C:\Users\alexp\OneDrive\Public\Logs\OleksaScrSvr.Video.log""");
    var entries = list.Select(x => x.Split('\t')).Select(x => SafeCreateRecord(x)
    ).ToList();

    dg1.ItemsSource = entries.OrderByDescending(r => r.Displayed);
  }

  static VideoLogEntry SafeCreateRecord(string[] x)
  {
    try
    {
      return new VideoLogEntry(DateTimeOffset.Parse(x[0]), DateTimeOffset.Parse(x[1]), int.Parse(x[2]), x[3], x[4], x[5], x[6]);
    }
    catch (Exception)
    {
      return new VideoLogEntry(DateTimeOffset.Now, DateTimeOffset.Now, 0, "·", "5qGo7te", "\\Pictures\\Main\\2016\\2012-05-04 - 2016-04-23 iPod\\IMG_6015.JPG", "https://dsm04pap002files.storage.live.com/y4mp2lNt4HhzhVmtpgKh-uPicy49p3x877F1fVep38LbXGcTaUMKtFE52eDbBNVfEDN1Z-nKLPtYQb8mX8N3aiOd-MvALZO8npVEgZQxyw7B_8QCbaCfezULNtl-0WsHF6ugr7wvBqb1txEsZhXDBVC0gS0UlCnyLaEyfgHyGFFx3fjmlLM_niMJ5mqC768WI6AJEm5e9PtS0jagp83HwKmi-2NZVCqWccIn3I96JTHyuU?width=800&height=598&cropmode=none");
    }
  }

  void dg1_SelectionChanged(object s, SelectionChangedEventArgs e) { mediaElement1.Source = new Uri(((VideoLogEntry)dg1.SelectedItem).ThumbnailUrl); ; }
  void OnOpenPath(object s, RoutedEventArgs e) => _ = Process.Start("Explorer.exe", $"/select, \"{videoFilename}\"");
  void OnExit(object s, RoutedEventArgs e) => Close();
  void OnThumbMouseUp(object s, System.Windows.Input.MouseButtonEventArgs e) { }
  void OnThumbMouseEnter(object s, System.Windows.Input.MouseEventArgs e) { Title = $"{((Image)s).Tag} mb   {((Image)s).ToolTip}"; ; }
  void OnPlayStart(object s, RoutedEventArgs e) { mediaElement1.Source = new Uri(videoFilename); ; }
  void OnDblClick(object s, System.Windows.Input.MouseButtonEventArgs e) { OnPlayStart(s, e); ; }
}

public record VideoLogEntry(DateTimeOffset Displayed, DateTimeOffset Created, int SizeMb, string SizeColumn, string Http, string VideoFilename, string ThumbnailUrl);
