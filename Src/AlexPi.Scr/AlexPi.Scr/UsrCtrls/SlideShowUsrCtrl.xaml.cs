namespace AlexPi.Scr.UsrCtrls;

public partial class SlideShowUsrCtrl
{
#if DEBUG
  readonly int showTime = 4_000;
  readonly int _initlDelay = 250;
  readonly int _inAndOutMs = 2_000;
  int _take = 5;
  readonly string _wildcard = "*.jpg";
#else
  readonly string _wildcard = "*.*";
  readonly int showTime = 60_000;
  readonly int _initlDelay = 2_500;
  readonly int _inAndOutMs = 5_000;
  int _take = 100;
#endif
  readonly Random _rand = new(DateTime.Now.Second);
  Storyboard _sbIntroOutro;
  readonly SizeWeightedRandomPicker _sizeWeightedRandomPicker = new(OneDrive.Folder("Pictures"));
  readonly string _curHistFile;
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

      mediaElmnt.Volume = .01;
    }
    catch (Exception ex) { _ = ex.Log(); }
  }

  async Task continueWithTheShow(string msg, int i, bool b) { Debug.WriteLine($"{msg} {i} {b}"); ctrlPanel.IsEnabled = true; await runMainLoop(); }

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
      _sbIntroOutro = (Storyboard)FindResource("_sbIntroOutro");

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

        mediaElmnt.Source = new Uri(file);
        _sbIntroOutro.Begin();

        var showtimeMs = TimeSpan.FromMilliseconds(showTime);
        if (MediaHelper.IsVideo(file))
        {
          for (var i = 0; i < 10; i++)
          {
            await Task.Delay(250);
            if (mediaElmnt.NaturalDuration.HasTimeSpan)
            {
              break;
            }

            tbbl.Text += ($"  ({i} tries) ");
          }

          tbbl.Text += tbtl.Text = $" {mediaElmnt.NaturalDuration.TimeSpan:m\\:ss}";

          showtimeMs += mediaElmnt.NaturalDuration.TimeSpan;
        }
        else
        {
          Debug.WriteLine($"  not video.");
        }

        mediaElmnt.Play();
        await Task.Delay(showtimeMs);
        await Task.Delay(_inAndOutMs);
        mediaElmnt.Pause();

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
      _sbIntroOutro = FindResource("_sbIntroOutro") as Storyboard;

      tbtr.Text = $"Delay starting...";
      await Task.Delay(_initlDelay);

      var window = Window.GetWindow(this);
      while (window == null)
      {
        await Task.Delay(100);
        window = Window.GetWindow(this);
      }
      window.PreviewKeyUp += onPreviewKeyUp;

      await runMainLoop();
    }
    catch (Exception ex) { tbtl.Foreground = new SolidColorBrush(Colors.Orange); tbtl.Text = ex.Log(); }
  }
  async Task runMainLoop()
  {
    _isPlaying = true;
    btnPlay.Visibility = Visibility.Hidden;

    do
    {
      var file = _sizeWeightedRandomPicker.PickRandomFile().FullName;
      if (_blackList.Contains(Path.GetExtension(file).ToLower()))
      {
        continue;
      }

      //tbbr.Text = $"{_randIdx,6:N0} / {ttlAvail:N0}";
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

      WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss} xx {file}");

      mediaElmnt.Source = new Uri(file);
      //mediaElmnt.Play(); Debug.WriteLine($"--- Play()");
      mediaElmnt.Pause(); Debug.WriteLine($"--- Pause()");
      mediaElmnt.Volume = .01;

      //tbbl.Text = "";

      var showtimeTs = TimeSpan.FromMilliseconds(showTime - _inAndOutMs);
      if (MediaHelper.IsVideo(file))
      {
        tbbl.Text += $" got duration in tries: ";
        for (var i = 1; i < 88; i++)
        {
          tbbl.Text += "·";
          await Task.Delay(100);
          if (mediaElmnt.NaturalDuration.HasTimeSpan)
          {
            tbbl.Text += $" {i}";
            break;
          }
        }

        var fallbackDuration = 0L;
        if (mediaElmnt.NaturalDuration.HasTimeSpan == false)
          fallbackDuration = (long)mediaElmnt.NaturalDuration.TimeSpan.TotalMilliseconds;
        else
          fallbackDuration = Path.GetExtension(file).ToLower() switch
          {
            ".mts" => (new FileInfo(file).Length) / (20298 * 1024 / 13000),
            _ => 0,
          };
        Debug.WriteLine($"--- {tbbl.Text}");

        if (false) { tbbl.Text += $" {mediaElmnt.NaturalDuration.TimeSpan:m\\:ss}"; showtimeTs += (mediaElmnt.NaturalDuration.HasTimeSpan ? mediaElmnt.NaturalDuration.TimeSpan : showtimeTs); }
        else
        if (mediaElmnt.NaturalDuration.HasTimeSpan && mediaElmnt.NaturalDuration != Duration.Automatic && mediaElmnt.NaturalDuration.TimeSpan.TotalMilliseconds > showTime)
        {
          mediaElmnt.Position = TimeSpan.FromMilliseconds(_rand.Next((int)mediaElmnt.NaturalDuration.TimeSpan.TotalMilliseconds - showTime));
          tbbl.Text += $" ··· {mediaElmnt.Position:m\\:ss} / {mediaElmnt.NaturalDuration.TimeSpan:m\\:ss}";
        }
      }
      else
      {
        Debug.WriteLine($"  not video.");
      }

      mediaElmnt.Play(); Debug.WriteLine($"--- Play()");
      _sbIntroOutro.Begin();

      if (autohide)
      {
        await Task.Delay(showtimeTs);
        await Task.Delay(_inAndOutMs);
        mediaElmnt.Pause();

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
      //_curHistFile = _allFiles[HistSlct = _randIdx = HistList[histIdx]];
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
      //var file = _allFiles[HistList[histIdx]];
      //Clipboard.SetText(file);
      //await showFile(file, false);
      btnNext.Visibility = Visibility.Visible;
    }
    catch (Exception ex) { tbtl.Foreground = new SolidColorBrush(Colors.Orange); tbtl.Text = ex.Log(); }
    finally
    {
      btnPrev.Visibility = histIdx > 0 ? Visibility.Visible : Visibility.Hidden;
      btnNext.Visibility = histIdx < HistList.Count - 1 ? Visibility.Visible : Visibility.Hidden;
      if (btnNext.Visibility == Visibility.Hidden)
      {
        await runMainLoop();
      }
    }
  }
  async void btnPlay_Click(object s, RoutedEventArgs e) => await runMainLoop();

  void lbxHist_SelectionChanged(object s, SelectionChangedEventArgs e)
  {
    if (e.AddedItems.Count < 1)
    {
      return;
    } ((ListBox)s).ScrollIntoView(e.AddedItems[0]);

    if (_randIdx != (int)e.AddedItems[0])
    {
      _isPlaying = false;
      _randIdx = (int)e.AddedItems[0];
      //await showFile(_allFiles[_randIdx], false);
    }
  }

  async void btnEdit_Click(object s, RoutedEventArgs e) { ctrlPanel.IsEnabled = false; /*deleteUsrCtrl1.ShowEditorPanel(_allFiles, _randIdx); */await Task.Yield(); }
  async void btnDele_Click(object s, RoutedEventArgs e) { if (!EvLogHelper.IsVIP) return; ctrlPanel.IsEnabled = false; /*_ = new DeleteMePopup(_allFiles[_randIdx]).ShowDialog(); */await continueWithTheShow("onDele", -123, false); }

  //IEnumerable<string> GetAllFiles__(string path, string searchPattern) => System.IO.Directory.EnumerateFiles(path, searchPattern).Union(
  //      System.IO.Directory.EnumerateDirectories(path).SelectMany(d =>
  //      {
  //        try
  //        {
  //          return GetAllFiles__(d, searchPattern);
  //        }
  //        catch (UnauthorizedAccessException)
  //        {
  //          return Enumerable.Empty<string>();
  //        }
  //      }));

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