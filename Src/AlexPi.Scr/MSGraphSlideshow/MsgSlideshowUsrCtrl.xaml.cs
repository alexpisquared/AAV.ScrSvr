namespace MSGraphSlideshow;
public partial class MsgSlideshowUsrCtrl
{
  const int _volumePerc = 16;
  const string _thumbnails = "thumbnails,children($expand=thumbnails)";
  Storyboard? _sbIntroOutro;
  readonly Random _random = new(Guid.NewGuid().GetHashCode());
  GraphServiceClient? _graphServiceClient;
  readonly LibVLC _libVLC;
  CancellationTokenSource? _cancellationTokenSource;
  readonly SizeWeightedRandomPicker _sizeWeightedRandomPicker = new(OneDrive.Folder("Pictures"));
#if DEBUG
  const int _maxMs = 9000;
#else
  const int _maxMs = 59000;
#endif
  int _currentShowTimeMS = 0;

  public MsgSlideshowUsrCtrl()
  {
    InitializeComponent();
    _libVLC = new LibVLC(enableDebugLogs: true);
    VideoView1.MediaPlayer = new MediaPlayer(_libVLC) { Volume = _volumePerc }; // percent
    VideoView1.MediaPlayer.EndReached += OnEndReached;
    _ = new DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Normal, new EventHandler(OnMoveProgressBarTimerTick), Dispatcher.CurrentDispatcher);
  }

  void SetAnimeDurationInMS(long ms)
  {
    //var showTime = (System.Windows.Duration)FindResource("showTime");
    var showTime = new System.Windows.Duration(TimeSpan.FromMilliseconds(ms));

    ((Storyboard)FindResource("_sbIntroOutro")).Duration = showTime;
    ((DoubleAnimation)FindResource("_d1IntroOutro")).Duration = showTime;
    ((DoubleAnimation)FindResource("_d2IntroOutro")).Duration = showTime;
  }
  public string ClientId { get; set; } = "9ba0619e-3091-40b5-99cb-c2aca4abd04e";
  void OnMoveProgressBarTimerTick(object? sender, EventArgs e)
  {
    //try    {
    VideoProgress.Maximum = 1;
    VideoProgress.Value = VideoView1.MediaPlayer?.Position ?? 0;
    //WriteLine($"  Psn:{VideoView1.MediaPlayer?.Position,6:N2}   timer");
    //}    catch (Exception ex)    {      WriteLine(ex);    }
  }
  async void OnLoaded(object sender, RoutedEventArgs e)
  {
    _sbIntroOutro = (Storyboard)FindResource("_sbIntroOutro");

    var (success, report, result) = await new AuthUsagePOC().LogInAsync(ClientId);
    if (!success)
    {
      ReportBC.Content = ($"{report}");
      WriteLine($"ERROR: {report}");
    }

    ArgumentNullException.ThrowIfNull(result, $"▀▄▀▄▀▄ {report}");
    ArgumentNullException.ThrowIfNull(_sbIntroOutro, $"▀▄▀▄▀▄ {nameof(_sbIntroOutro)}");

    _graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
    {
      requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
      await Task.CompletedTask;
    }));

    if (DesignerProperties.GetIsInDesignMode(this)) return; //tu: design mode for the consumers is a quiet one.

    while (true)
    {
      if (chkIsOn.IsChecked == true)
        await LoadWaitThenShowNext();
      else
        await Task.Delay(100);
    }
  }
  void OnClose(object sender, RoutedEventArgs e) => Close();
  void OnNext(object sender, RoutedEventArgs e) => _cancellationTokenSource?.Cancel();
  void OnEndReached(object? sender, EventArgs e) => _cancellationTokenSource?.Cancel();

  async Task LoadWaitThenShowNext()
  {
    string mediaType = "----", streamReport = "-- ", cancelReport = "";
    var dnldTime = TimeSpan.Zero;
    var driveItem = (DriveItem?)default;

    var pathfile = GetRandomMediaFile();

    try
    {
      ArgumentNullException.ThrowIfNull(_graphServiceClient, nameof(_graphServiceClient));
      ArgumentNullException.ThrowIfNull(_sbIntroOutro, nameof(_sbIntroOutro));

      driveItem = await _graphServiceClient.Drive.Root.ItemWithPath(pathfile).Request().Expand(_thumbnails).GetAsync();      // ~200 ms    Write($"** {.000001 * driveItem.Size,8:N1}mb   sec:{Stopwatch.GetElapsedTime(start).TotalSeconds,5:N2}");
      if (driveItem.Video is null && driveItem.Image is null && driveItem.Photo is null)
        return;

      HistoryL.Content = $"{.000001 * driveItem.Size,5:N1}mb";
      HistoryR.Content += $"{driveItem.Name}\n";

      var taskStream = TaskDownloadStream(pathfile);
      try
      {
        _cancellationTokenSource = new();
        var taskDelay = Task.Delay(_currentShowTimeMS, _cancellationTokenSource.Token);
        await Task.WhenAll(taskStream, taskDelay);
        dnldTime = taskStream.Result.dnldTime;
      }
      catch (OperationCanceledException) { cancelReport = "<CTS.Cancel>"; }
      catch (Exception ex)
      {
        ReportBC.Content = $"** ERROR: {ex.Message}\n  {pathfile}";
        WriteLine($"\nERROR inner for  {pathfile}  {ReportBC.Content}  {ex.Message}\n");
        System.Media.SystemSounds.Beep.Play();

        if (Debugger.IsAttached) Debugger.Break();        //else        //  await Task.Delay(15_000);
      }
      finally { _cancellationTokenSource?.Dispose(); _cancellationTokenSource = null; }

      ArgumentNullException.ThrowIfNull(VideoView1.MediaPlayer, "@@@@@@@@@@@@@@@@@++++++++@");
      //if (VideoView1.MediaPlayer.CanPause == true)        VideoView1.MediaPlayer.Pause();
      VideoView1.MediaPlayer.Stop();

#if DEBUG
      System.Media.SystemSounds.Hand.Play();
#endif

      ReportBR.Content = $"{driveItem.Name}";
      ReportBL.Content = $"{.000001 * driveItem.Size,5:N1}mb";

      if (driveItem.Image is not null)
      {
        mediaType = $"img";
        ReportTR.Content = $"{driveItem.Image.Width,6:N0} x {driveItem.Image.Height,-6:N0}";
        streamReport = $"{driveItem.Image.Width,29:N0}·{driveItem.Image.Height,-8:N0}";
        ImageView1.Source = (await GetBipmapFromStream(taskStream.Result.stream)).bitmapImage;
        VideoInterval.Visibility = VideoProgress.Visibility = Visibility.Hidden;        //VideoView1.Visibility =
        ImageView1.Visibility = Visibility.Visible;
        SetAnimeDurationInMS(_maxMs);
        _sbIntroOutro?.Begin();
      }
      else if (driveItem.Video is not null)
      {
        mediaType = $"Video";
        var (durationInMs, isExact, report) = await StartPlayingMediaStream(taskStream.Result.stream, driveItem);
        ReportTR.Content = $"{(isExact ? '=' : '~')}{durationInMs * .001:N0} s";
        streamReport = report;
        ImageView1.Visibility = Visibility.Hidden;
        VideoInterval.Visibility = VideoProgress.Visibility = Visibility.Visible;        //VideoView1.Visibility =
      }
      else if (driveItem.Photo is not null)
      {
        mediaType = $"■Photo■";
        ReportBC.Content = $"{.000001 * driveItem.Size,8:N1}mb  ??? What to do with Photo? ??     {driveItem.Photo.CameraMake} x {driveItem.Photo.CameraModel}    {driveItem.Name}   ▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄";
        WriteLine($"  {pathfile}  {ReportBC.Content}  ");
        ImageView1.Source = (await GetBipmapFromStream(taskStream.Result.stream)).bitmapImage;
        VideoInterval.Visibility = VideoProgress.Visibility = Visibility.Hidden;        //VideoView1.Visibility = 
        ImageView1.Visibility = Visibility.Visible;
      }
      else
      {
        mediaType = $"■ else ■";
        ReportBC.Content = $"{.000001 * driveItem.Size,8:N1}mb  !!! NOT A MEDIA FILE !!!    {driveItem.Name}   ▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐";
        WriteLine($"  {pathfile}  {ReportBC.Content}  ");
      }
    }
    catch (Exception ex)
    {
      ReportBC.Content = $"** ERROR: {ex.Message}\n  {pathfile}";
      WriteLine($"\nERROR outer for  {pathfile}  {ReportBC.Content}  {ex.Message}\n");
      System.Media.SystemSounds.Beep.Play();

      if (Debugger.IsAttached)
        Debugger.Break();
      //else
      //  await Task.Delay(15_000);
    }
    finally
    {
      WriteLine($"{DateTime.Now:HH:mm:ss.f} dl{.000001 * driveItem?.Size,4:N0}mb/{dnldTime.TotalSeconds,3:N0}s{mediaType,8}  {streamReport,-55}{driveItem?.Name,52}  {cancelReport}");
      ReportTL.Content = $"{driveItem?.CreatedDateTime:yyyy-MM-dd}";

      _currentShowTimeMS = _maxMs;
    }
  }
  async Task<(Stream stream, TimeSpan dnldTime)> TaskDownloadStream(string file)
  {
    ArgumentNullException.ThrowIfNull(_graphServiceClient, nameof(_graphServiceClient));
    var start = Stopwatch.GetTimestamp();
    var stream = await _graphServiceClient.Drive.Root.ItemWithPath(file).Content.Request().GetAsync();
    var dnldTm = Stopwatch.GetElapsedTime(start);

    return (stream, dnldTm);
  }
  string GetRandomMediaFile()
  {
    var _blackList = new string[] {
      ".aae",
      ".application",
      ".bat",
      ".db",
      ".deploy",
      ".dll",
      ".exe",
      ".ini",
      ".log",
#if DEBUG
      ".3gp",
      ".dng",
      //".jpg",
      ".mov",
      ".mp4",
      ".mpg",
      ".mpo",
      ".MPO",
      ".m2ts",
      ".mts",
      ".png",
      //".wmv",
#endif
      ".manifest",
      ".nar",
      ".oxps",
      ".pcd",
      ".modd",
      ".thumb",
      ".txt"  };

    for (var i = 0; i < _sizeWeightedRandomPicker.Count; i++)
    {
      var fileinfo = _sizeWeightedRandomPicker.PickRandomFile();
      var pathfile = fileinfo.FullName[(OneDrive.Root.Length - Environment.UserName.Length + 5)..];      //file = @"C:\Users\alexp\OneDrive\Pictures\Main\_New\2013-07-14 Lumia520\Lumia520 014.mp4"[OneDrive.Root.Length..]; //100mb      //file = @"C:\Users\alexp\OneDrive\Pictures\Camera imports\2018-07\VID_20180610_191622.mp4"[OneDrive.Root.Length..]; //700mb takes ~1min to download on WiFi and only then starts playing.
      if (_blackList.Contains(Path.GetExtension(pathfile).ToLower()) == false
#if DEBUG
        && 2_000_000 < fileinfo.Length && fileinfo.Length < 90_000_000
#endif
        )
        return pathfile;
    }

    throw new Exception("No suitable media files found ▄▀▄▀▄▀▄▀▄▀▄▀");
  }
  async Task<(long durationInMs, bool isExact, string report)> StartPlayingMediaStream(Stream stream, DriveItem driveItem)
  {
    var media = new Media(_libVLC, new StreamMediaInput(stream));

    ArgumentNullException.ThrowIfNull(VideoView1.MediaPlayer, "@@@@@@@@@@@@@@@@@@");

    //VideoView1.MediaPlayer.Media = media;
    _ = VideoView1.MediaPlayer.Play(media);
    //var report2 = $"►{(playSucces ? "+" : "-")} ";

    VideoView1.MediaPlayer.Volume = _volumePerc;
    VideoView1.MediaPlayer.Mute = VideoView1.MediaPlayer.Volume != _volumePerc;

    var (durationMs, isExact, report) = await TryGetBetterDuration(driveItem, media);
    _currentShowTimeMS = Math.Min((int)durationMs, _maxMs);
    SetAnimeDurationInMS(_currentShowTimeMS);
    _sbIntroOutro?.Begin();

    var report2 = $"{report}{durationMs * .001,4:N0}s-durn: seekTo";
    if (durationMs > _currentShowTimeMS)
    {
      var diffMs = durationMs - _currentShowTimeMS;
      var seekToMs = _random.Next((int)diffMs);
      var percd100 = (double)seekToMs / durationMs;
      VideoProgress.Value = percd100;

      //await Task.Delay(2000);      System.Media.SystemSounds.Beep.Play();

      VideoView1.MediaPlayer.SetPause(true);
      VideoView1.MediaPlayer.SeekTo(TimeSpan.FromMilliseconds(seekToMs));
      VideoView1.MediaPlayer.SetPause(false);

      //while (VideoView1.MediaPlayer.Position < percd100)      {        VideoView1.MediaPlayer.SeekTo(TimeSpan.FromMilliseconds(seekToMs));        await Task.Delay(10);        Write("!");      }

      //WriteLine($"      {percd100,6:N2} % ~~ {TimeSpan.FromMilliseconds(seekToMs):mm\\:ss}         <== new setting to !!!!!!!");
      //WriteLine($"  Psn:{VideoView1.MediaPlayer.Position,6:N2} %         after ^^ ");

      report2 += $"{seekToMs * .001,3:N0}s {(VideoView1.MediaPlayer.Position < percd100 ? ("FAILED " + Path.GetExtension(driveItem.Name).ToLower()) : "++")}";

      var k = 1000.0 / durationMs;
      rectStart.Width = seekToMs * k;
      rectMiddl.Width = _currentShowTimeMS * k;
      rectRest1.Width = (durationMs - seekToMs - _currentShowTimeMS) * k;
    }
    else if (durationMs > 0)
      report2 += ($"  °  :it's <{_maxMs * .001,3:N0}s prd");
    else
      report2 += ($"  °  Prorate this ext! ▄▀▄▀");

    if (durationMs <= _currentShowTimeMS)
    {
      rectMiddl.Width = 0;
    }

    return (durationMs, isExact, report2); // in ms
  }
  static async Task<(BitmapImage bitmapImage, string report)> GetBipmapFromStream(Stream? stream)
  {
    ArgumentNullException.ThrowIfNull(stream, nameof(stream));

    var memoryStream = new MemoryStream();
    await stream.CopyToAsync(memoryStream);
    _ = memoryStream.Seek(0, SeekOrigin.Begin); //tu: JPG images fix!!!
    stream.Close();

    var bmp = new System.Windows.Media.Imaging.BitmapImage();
    bmp.BeginInit();
    bmp.StreamSource = memoryStream;
    bmp.EndInit();

    await Task.Yield();

    return (bmp, "++");
  }
  static async Task<(long durationMs, bool isExact, string report)> TryGetBetterDuration(DriveItem driveItem, Media media)
  {
    const int maxTries = 101;
    var i = 1; for (; i < maxTries && media.Duration <= 0; i++) await Task.Delay(10);
    var rv = i < maxTries ? ($"{i,2} try") : "estimd";

    return media.Duration > 0
      ? (media.Duration, true, rv)
      : Path.GetExtension(driveItem.Name).ToLower() switch
      {
        ".m2ts" => ((driveItem.Size ?? 0) / (38208 * 1024 / 13000), false, rv),
        ".mts" => ((driveItem.Size ?? 0) / (20298 * 1024 / 13000), false, rv),
        _ => (0, false, "//todo"),
      };
  }

#pragma warning disable IDE0051 // Remove unused private members
  async Task Testingggggggg(string thm, string file)
#pragma warning restore IDE0051 // Remove unused private members
  {
    ArgumentNullException.ThrowIfNull(_graphServiceClient, nameof(_graphServiceClient));
    //var me = await graphServiceClient.Me.Request().GetAsync();
    ImageView1.Source = (await GetBipmapFromStream(await _graphServiceClient.Me.Photo.Content.Request().GetAsync())).bitmapImage;
    _ = await _graphServiceClient.Drive.Root.Request().Expand(thm).GetAsync();
    _ = await _graphServiceClient.Drive.Root.ItemWithPath("/Pictures").Request().Expand(thm).GetAsync();
    _ = await _graphServiceClient.Drive.Root.ItemWithPath(file).Request().Expand(thm).GetAsync();

    var items = await _graphServiceClient.Me.Drive.Root.Children.Request().GetAsync(); //tu: onedrive root folder items == 16 dirs.
    _ = items.ToList()[12].Folder;
  }
}