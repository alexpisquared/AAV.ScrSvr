﻿namespace AlexPi.Scr.UsrCtrls;

public partial class SlideShowUsrCtrl
{
#if DEBUG
  readonly int _showtimeMs = 4_000;
  readonly int _initlDelay = 250;
  readonly int _inAndOutMs = 2_000;
  int _take = 5;
  readonly string _wildcard = "*.jpg";
#else
  readonly string _wildcard = "*.*";
  readonly int _showtimeMs = 60_000;
  readonly int _initlDelay = 2_500;
  readonly int _inAndOutMs = 5_000;
  int _take = 100;
#endif
  readonly Random _rand = new(DateTime.Now.Second);
  Storyboard _sbin, _sbou, _time;
  string[] _allFiles;
  string _curHistFile;
  bool _isPlaying = true;
  int _back = 0, _randIdx;
  public int HistSlct { get; set; }
  public int HistIndx { get; set; }
  public ObservableCollection<int> HistList { get; set; } = new ObservableCollection<int>();

  public SlideShowUsrCtrl()
  {
    try
    {
      InitializeComponent();
      Loaded += onLoadedRand;
      PreviewKeyUp += onPreviewKeyUp;

      deleteUsrCtrl1.ClosedA += async (s, e) => await continueWithTheShow(e?.ToString(), -321, false);
      deleteUsrCtrl1.ClosedB += async (m, i, b) => await continueWithTheShow(m, i, b);
      deleteUsrCtrl1.ClosedC += async (m, i, b) => await continueWithTheShow(m, i, b);

      me1.Volume = .01;
    }
    catch (Exception ex) { _ = ex.Log(); }
  }

  async Task continueWithTheShow(string msg, int i, bool b) { Debug.WriteLine($"{msg} {i} {b}"); ctrlPanel.IsEnabled = true; await runMainLoop(_allFiles.Length); }

  void onPreviewKeyUp(object s, System.Windows.Input.KeyEventArgs e)
  {
    switch (e.Key)
    {
      case Key.Up: case Key.Left: if (e.SystemKey is Key.LeftAlt or Key.RightAlt) { btnPrev_Click(s, e); e.Handled = true; } break;
      case Key.Down: case Key.Right: if (e.SystemKey is Key.LeftAlt or Key.RightAlt) { btnNext_Click(s, e); e.Handled = true; } break;
      default: Debug.WriteLine(tbtl.Text = $"-- keyUp : {e.Key}   {e.SystemKey}"); break;
    }
  }
  async void onLoadedSeql(object s, RoutedEventArgs e)
  {
    List<string> filesToShow;
    int ttlAvail, skip;

    try
    {
      _sbin = FindResource("sbIn") as Storyboard;
      _sbou = FindResource("sbOu") as Storyboard;
      _time = FindResource("sb15") as Storyboard;

      tbtr.Text = $"Delay starting...";
      if (true)
      {
        await Task.Delay(_initlDelay);

        var sw = Stopwatch.StartNew();
        var allFiles = await OneDrive.GetFileNamesAsync(_wildcard);
        sw.Stop();

        ttlAvail = allFiles.Length;
        if (ttlAvail <= _take)
        {
          _take = ttlAvail / 5;
        }

        skip = _rand.Next(ttlAvail - _take);
        filesToShow = allFiles.Skip(skip).Take(_take).ToList();
      }
      else
      {
#if false
        using (var db = new A0DbContext())
                  {
                      tbbl.Text = $"{db.Database.Connection.ConnectionString}";
                      await Task.Delay(_initlDelay);

                      var lst = db.FileDetails.//Where(r => !r.FileExtn.Equals(".jpg")).
                          OrderBy(r => r.ID).Select(r => r.FullPath);

                      ttlAvail = await lst.CountAsync();
                      skip = _rand.Next(ttlAvail - _take);
                      filesToShow = lst.Skip(skip).Take(_take).ToList();
                  }
#endif
      }

      tbbr.Text = $"Finished loading {filesToShow.Count} files for the show starting from {skip} / {ttlAvail:N0}.";

      btnPlay.Visibility = Visibility.Hidden;

      var currentIdx = 0;
      foreach (var file in filesToShow)
      {
        tbtr.Text = $"{System.IO.Path.GetDirectoryName(file)} \r\n{System.IO.Path.GetFileName(file)} \r\n";                    //tbtl.Text = $"{f.FileCreated:yyyy-MM-dd  HH}";

        if (!File.Exists(file))
        {
          tbbr.Text = "!Exists!";
          continue;
        }

        tbbl.Text = $"{(File.Exists(file) ? "" : "!Exists!")}";
        tbbl.Text = $"{++currentIdx} / {filesToShow.Count}";

        me1.Source = new Uri(file);
        _sbin.Begin();

        var showtimeMs = TimeSpan.FromMilliseconds(_showtimeMs);
        if (MediaHelper.IsVideo(file))
        {
          for (var i = 0; i < 10; i++)
          {
            await Task.Delay(250);
            if (me1.NaturalDuration.HasTimeSpan)
            {
              break;
            }

            tbbl.Text += ($"  ({i} tries) ");
          }

          tbbl.Text += tbtl.Text = $" {me1.NaturalDuration.TimeSpan:m\\:ss}";

          showtimeMs += me1.NaturalDuration.TimeSpan;
        }
        else
        {
          Debug.WriteLine($"  not video.");
        }

        me1.Play();
        await Task.Delay(showtimeMs);
        _sbou.Begin();
        await Task.Delay(_inAndOutMs);
        me1.Pause();

        //Visibility = Visibility.Hidden;
        //await Task.Delay(12000); //2022-01: let see other controls
        //Visibility = Visibility.Visible;
      }

      btnPlay.Visibility = Visibility.Visible;

      tbbr.Text = tbtr.Text = $"Finished {filesToShow.Count} file show starting from {skip} / {ttlAvail:N0}.";
    }
    catch (Exception ex) { tbtl.Foreground = new SolidColorBrush(Colors.Orange); tbtl.Text = ex.Log(); }
  }
  async void onLoadedRand(object s, RoutedEventArgs e)
  {
    try
    {
      _sbin = FindResource("sbIn") as Storyboard;
      _sbou = FindResource("sbOu") as Storyboard;
      _time = FindResource("sb15") as Storyboard;

      tbtr.Text = $"Delay starting...";
      await Task.Delay(_initlDelay);

      var window = Window.GetWindow(this);
      while (window == null)
      {
        await Task.Delay(100);
        window = Window.GetWindow(this);
      }
      window.PreviewKeyUp += onPreviewKeyUp;

      var sw = Stopwatch.StartNew();

      _allFiles = await OneDrive.GetFileNamesAsync(_wildcard);                //GetAllFiles(@"C:\", "*").ToArray();

      sw.Stop();

      var ttlAvail = _allFiles.Length;

      tbtl.Text = $"Loaded  {ttlAvail:N0}  files in  {sw.Elapsed:m\\:ss\\.f}";

      if (ttlAvail < 1)
      {
        tbtl.Text += " ... No show  :(";
      }
      else
      {
        await runMainLoop(ttlAvail);
      }
    }
    catch (Exception ex) { tbtl.Foreground = new SolidColorBrush(Colors.Orange); tbtl.Text = ex.Log(); }
  }
  async Task runMainLoop(int ttlAvail)
  {
    _isPlaying = true;
    btnPlay.Visibility = Visibility.Hidden;

    do
    {
      _randIdx = _rand.Next(ttlAvail);
      var file = _allFiles[_randIdx];
      if (_blackList.Contains(Path.GetExtension(file).ToLower()))
      {
        continue;
      }

      tbbr.Text = $"{_randIdx,6:N0} / {ttlAvail:N0}";
      tbtr.Text = $"{Path.GetDirectoryName(file)} \r\n{Path.GetFileName(file)} \r\n";                    //tbtl.Text = $"{f.FileCreated:yyyy-MM-dd  HH}";

      tbtl.Text = $"{new FileInfo(file).LastWriteTime:MMM dd, yyyy  ddd}";
      tbbl.Text = $"{new FileInfo(file).LastWriteTime:HH:mm}";

      HistList.Add(HistSlct = _randIdx);
      lbxHist.SelectedIndex = HistIndx = HistList.Count - 1;

      await showFile(file);
    }
    while (_isPlaying);
    tbtr.Text = $"_isPlaying == {_isPlaying}.";

    btnPlay.Visibility = Visibility.Visible;
  }
  async Task showFile(string file, bool autohide = true)
  {
    Debug.WriteLine($"---");
    try
    {
      me1.Source = new Uri(file);
      //me1.Play(); Debug.WriteLine($"--- Play()");
      me1.Pause(); Debug.WriteLine($"--- Pause()");
      me1.Volume = .01;

      //tbbl.Text = "";

      var showtimeTs = TimeSpan.FromMilliseconds(_showtimeMs - _inAndOutMs);
      if (MediaHelper.IsVideo(file))
      {
        for (var i = 0; i < 100; i++)
        {
          tbbl.Text += ($" (got duration in {i + 1} tries) ");
          await Task.Delay(100);
          if (me1.NaturalDuration.HasTimeSpan)
          {
            break;
          }
        }

        Debug.WriteLine($"--- {tbbl.Text}");

        if (false) { tbbl.Text += $" {me1.NaturalDuration.TimeSpan:m\\:ss}"; showtimeTs += (me1.NaturalDuration.HasTimeSpan ? me1.NaturalDuration.TimeSpan : showtimeTs); }
        else
        if (me1.NaturalDuration.HasTimeSpan && me1.NaturalDuration != Duration.Automatic && me1.NaturalDuration.TimeSpan.TotalMilliseconds > _showtimeMs)
        {
          me1.Position = TimeSpan.FromMilliseconds(_rand.Next((int)me1.NaturalDuration.TimeSpan.TotalMilliseconds - _showtimeMs));
          tbbl.Text += $" ··· {me1.Position:m\\:ss} / {me1.NaturalDuration.TimeSpan:m\\:ss}";
        }
      }
      else
      {
        Debug.WriteLine($"  not video.");
      }

      me1.Play(); Debug.WriteLine($"--- Play()");
      _sbin.Begin();
      _time.Begin();

      if (autohide)
      {
        await Task.Delay(showtimeTs);
        _sbou.Begin(); await Task.Delay(_inAndOutMs);
        me1.Pause();

        Visibility = Visibility.Hidden;
        await Task.Delay(_inAndOutMs * 2); //2022-01: let see other controls
        Visibility = Visibility.Visible;
      }
    }
    catch (InvalidOperationException ex) { tbtl.Foreground = new SolidColorBrush(Colors.Green); tbtl.Text = ex.Log($"\r\n {file}"); }
    catch (Exception ex) { tbtl.Foreground = new SolidColorBrush(Colors.Orange); tbtl.Text = ex.Log($"\r\n {file}"); }
  }

  async void btnPrev_Click(object s, RoutedEventArgs e)
  {
    var histIdx = 0;
    try
    {
      _isPlaying = false;
      btnPrev.Visibility = Visibility.Hidden;

      lbxHist.SelectedIndex = histIdx = HistList.Count - (++_back) - 1;
      _curHistFile = _allFiles[HistSlct = _randIdx = HistList[histIdx]];
      Clipboard.SetText(_curHistFile);
      await showFile(_curHistFile, false);
      btnNext.Visibility = btnEdit.Visibility = Visibility.Visible;
    }
    catch (Exception ex) { tbtl.Foreground = new SolidColorBrush(Colors.Orange); tbtl.Text = ex.Log(); }
    finally
    {
      btnPrev.Visibility = histIdx > 0 ? Visibility.Visible : Visibility.Hidden;
      btnNext.Visibility = histIdx < HistList.Count - 1 ? Visibility.Visible : Visibility.Hidden;
    }
  }
  async void btnNext_Click(object s, RoutedEventArgs e)
  {
    var histIdx = 0;
    try
    {
      _isPlaying = false;
      btnPrev.Visibility = Visibility.Hidden;

      lbxHist.SelectedIndex = histIdx = HistList.Count - (--_back) - 1;
      var file = _allFiles[HistList[histIdx]];
      Clipboard.SetText(file);
      await showFile(file, false);
      btnNext.Visibility = Visibility.Visible;
    }
    catch (Exception ex) { tbtl.Foreground = new SolidColorBrush(Colors.Orange); tbtl.Text = ex.Log(); }
    finally
    {
      btnPrev.Visibility = histIdx > 0 ? Visibility.Visible : Visibility.Hidden;
      btnNext.Visibility = histIdx < HistList.Count - 1 ? Visibility.Visible : Visibility.Hidden;
      if (btnNext.Visibility == Visibility.Hidden)
      {
        await runMainLoop(_allFiles.Length);
      }
    }
  }
  async void btnPlay_Click(object s, RoutedEventArgs e) => await runMainLoop(_allFiles.Length);

  async void lbxHist_SelectionChanged(object s, SelectionChangedEventArgs e)
  {
    if (e.AddedItems.Count < 1)
    {
      return;
    } ((ListBox)s).ScrollIntoView(e.AddedItems[0]);

    if (_randIdx != (int)e.AddedItems[0])
    {
      _isPlaying = false;
      _randIdx = (int)e.AddedItems[0];
      await showFile(_allFiles[_randIdx], false);
    }
  }

  async void btnEdit_Click(object s, RoutedEventArgs e) { ctrlPanel.IsEnabled = false; deleteUsrCtrl1.ShowEditorPanel(_allFiles, _randIdx); await Task.Yield(); }
  async void btnDele_Click(object s, RoutedEventArgs e) { if (!EvLogHelper.IsVIP) return; ctrlPanel.IsEnabled = false; _ = new DeleteMePopup(_allFiles[_randIdx]).ShowDialog(); await continueWithTheShow("onDele", -123, false); }



  IEnumerable<string> GetAllFiles(string path, string searchPattern) => System.IO.Directory.EnumerateFiles(path, searchPattern).Union(
        System.IO.Directory.EnumerateDirectories(path).SelectMany(d =>
        {
          try
          {
            return GetAllFiles(d, searchPattern);
          }
          catch (UnauthorizedAccessException)
          {
            return Enumerable.Empty<string>();
          }
        }));

  void on_MediaOpened(object s, /**/     RoutedEventArgs e) => Debug.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss}> Media Loaded : {e}");
  void on_MediaEnded(object s,  /**/     RoutedEventArgs e) => Debug.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss}> Media Ended  : {e}");
  void on_MediaFailed(object s, ExceptionRoutedEventArgs e) => Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss}> Media Failed : {e.ErrorException.Message} \t{((MediaElement)s)?.Source?.LocalPath}");

  readonly string[] _blackList = new string[]
  {
    ".aae",
    ".application",
    ".bat",
    ".db",
    ".deploy",
    ".dll",
    ".exe",
    ".ini",
    ".log",
    ".manifest",
    ".nar",
    ".oxps",
    ".pcd",
    ".modd",
    ".thumb",
    ".txt"  };
}
///todo: Delete popup window
///todo: timeline near
///todo: C# Advanced Tutorials. Delegates, Lambdas, Action T, and Func T (720-Aac192)
///