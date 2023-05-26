namespace MSGraphSlideshow;
public partial class MsgSlideshowUsrCtrl
{
  const int _volumePerc = 16;
  const string _thumbnails = "thumbnails,children($expand=thumbnails)";
  Storyboard? _sbIntroOutro;
  readonly Random _random = new(Guid.NewGuid().GetHashCode());
  GraphServiceClient? _graphServiceClient;
  readonly LibVLC _libVLC;
  TimeSpan _maxShowTime = TimeSpan.FromMicroseconds(22);
  CancellationTokenSource? _cancellationTokenSource;
  readonly SizeWeightedRandomPicker _sizeWeightedRandomPicker = new(OneDrive.Folder("Pictures"));
#if DEBUG
  const int _maxMs = 9000;
#else
  const int _maxMs = 59000;
#endif
  int _currentShowTimeMS = _maxMs;

  public MsgSlideshowUsrCtrl()
  {
    InitializeComponent();
    _libVLC = new LibVLC(enableDebugLogs: true);
    VideoView1.MediaPlayer = new MediaPlayer(_libVLC) { Volume = _volumePerc }; // percent
    VideoView1.MediaPlayer.EndReached += OnEndReached;
    _ = new DispatcherTimer(TimeSpan.FromMicroseconds(50), DispatcherPriority.Background, new EventHandler(OnTimerTick), Dispatcher.CurrentDispatcher);
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
  void OnTimerTick(object? sender, EventArgs e)
  {
    //try    {
    SeekBar.Maximum = 1;
    SeekBar.Value = VideoView1.MediaPlayer?.Position ?? 0;
    //}    catch (Exception ex)    {      WriteLine(ex);    }
  }
  async void OnLoaded(object sender, RoutedEventArgs e)
  {
    _sbIntroOutro = (Storyboard)FindResource("_sbIntroOutro");

    var (success, report, result) = await new AuthUsagePOC().LogInAsync(ClientId);
    if (!success)
    {
      ReportBC.Text = ($"{report}");
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
    string adtn = "----", streamReport = "-- ", cnacReport = "";
    var dnldTime = TimeSpan.Zero;
    DriveItem? driveItem = default;

    var file = GetRandomMediaFile();

    try
    {
      ArgumentNullException.ThrowIfNull(_graphServiceClient, nameof(_graphServiceClient));
      ArgumentNullException.ThrowIfNull(_sbIntroOutro, nameof(_sbIntroOutro));

      driveItem = await _graphServiceClient.Drive.Root.ItemWithPath(file).Request().Expand(_thumbnails).GetAsync();      // ~200 ms    Write($"** {.000001 * driveItem.Size,8:N1}mb   sec:{Stopwatch.GetElapsedTime(start).TotalSeconds,5:N2}");
      if (driveItem.Video is null && driveItem.Image is null && driveItem.Photo is null)
        return;

      ReportTR.Content += $"{driveItem.Name}  {.000001 * driveItem.Size,8:N1}mb ...\n";

      var taskStream = TaskDownloadStream(file);
      try
      {
        _cancellationTokenSource = new();
        var taskDelay = Task.Delay(_currentShowTimeMS, _cancellationTokenSource.Token);
        await Task.WhenAll(taskStream, taskDelay);
        dnldTime = taskStream.Result.dnldTime;
      }
      catch (OperationCanceledException) { cnacReport = ($"<<<Cancel>>"); }
      catch (Exception ex)
      {
        ReportBC.Text = $"** ERROR: {ex.Message}\n  {file}";
        WriteLine($"\nERROR inner for  {file}  {ReportTC.Text}  {ex.Message}\n");
        System.Media.SystemSounds.Hand.Play();

        if (Debugger.IsAttached) Debugger.Break();        //else        //  await Task.Delay(15_000);
      }
      finally { _cancellationTokenSource?.Dispose(); }

      _maxShowTime = TimeSpan.FromMilliseconds(_maxMs);

      if (VideoView1.MediaPlayer?.CanPause == true)
        VideoView1.MediaPlayer?.Pause();

#if DEBUG
      System.Media.SystemSounds.Beep.Play();
#endif

      if (driveItem.Image is not null)
      {
        adtn = $"img";
        ReportTC.Text = $"{.000001 * driveItem.Size,8:N1}mb    {driveItem.Image.Width,8:N0}x{driveItem.Image.Height,-8:N0}    {driveItem.Name}";
        streamReport = $"{driveItem.Image.Width,29:N0}·{driveItem.Image.Height,-8:N0}";
        ImageView1.Source = (await GetBipmapFromStream(taskStream.Result.stream)).bitmapImage;
        VideoView1.Visibility = Visibility.Hidden;
        ImageView1.Visibility = Visibility.Visible;
      }
      else if (driveItem.Video is not null)
      {
        adtn = $"Video";
        var (durationInMs, report) = await StartPlayingMediaStream(taskStream.Result.stream, driveItem);
        ReportTC.Text = $"{.000001 * driveItem.Size,8:N1}mb    {durationInMs * .001:N0}s-durn    {driveItem.Name,-52}";
        streamReport = report;
        ImageView1.Visibility = Visibility.Hidden;
        VideoView1.Visibility = Visibility.Visible;
      }
      else if (driveItem.Photo is not null)
      {
        adtn = $"Photo■■";
        ReportTC.Text = $"{.000001 * driveItem.Size,8:N1}mb  ??? What to do with Photo? ??     {driveItem.Photo.CameraMake}x{driveItem.Photo.CameraModel}    {driveItem.Name}   ▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄";
        ImageView1.Source = (await GetBipmapFromStream(taskStream.Result.stream)).bitmapImage;
        VideoView1.Visibility = Visibility.Hidden;
        ImageView1.Visibility = Visibility.Visible;
      }
      else
      {
        adtn = $"■ else ■";
        ReportTC.Text = $"{.000001 * driveItem.Size,8:N1}mb  !!! NOT A MEDIA FILE !!!    {driveItem.Name}   ▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐";
      }
    }
    catch (Exception ex)
    {
      ReportBC.Text = $"** ERROR: {ex.Message}\n  {file}";
      WriteLine($"\nERROR outer for  {file}  {ReportTC.Text}  {ex.Message}\n");
      System.Media.SystemSounds.Hand.Play();

      if (Debugger.IsAttached)
        Debugger.Break();
      else
        await Task.Delay(15_000);
    }
    finally
    {
      WriteLine($"{DateTime.Now:HH:mm:ss.f}  dnld{.000001 * driveItem?.Size,6:N1}mb in{dnldTime.TotalSeconds,3:N0}s{adtn,8}  {streamReport,-55}{driveItem?.Name,52}  {cnacReport}");
      ReportBC.Text = "";
      ReportTL.Text = $"{driveItem?.CreatedDateTime:yyyy-MM-dd}";
      //ReportTR.Text = $"{driveItem.LastModifiedDateTime:yyyy-MM-dd}";
      //ReportBL.Text = $"{driveItem.CreatedBy}    {driveItem.CreatedByUser}  ";
      //ReportBR.Text = $"{driveItem.LastModifiedBy}    {driveItem.LastModifiedByUser}  ";

      _currentShowTimeMS = _maxMs;
    }
  }

  async Task<(Stream stream, TimeSpan dnldTime)> TaskDownloadStream(string file)
  {
    ArgumentNullException.ThrowIfNull(_graphServiceClient, nameof(_graphServiceClient));
    var start = Stopwatch.GetTimestamp();
    var stream =  await _graphServiceClient.Drive.Root.ItemWithPath(file).Content.Request().GetAsync();
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
      ".jpg",
      ".mov",
      ".mp4",
      ".mpg",
      ".mpo",
      ".MPO",
      ".mts",
      ".png",
      ".wmv",
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
      var file = fileinfo.FullName[(OneDrive.Root.Length - Environment.UserName.Length + 5)..];      //file = @"C:\Users\alexp\OneDrive\Pictures\Main\_New\2013-07-14 Lumia520\Lumia520 014.mp4"[OneDrive.Root.Length..]; //100mb      //file = @"C:\Users\alexp\OneDrive\Pictures\Camera imports\2018-07\VID_20180610_191622.mp4"[OneDrive.Root.Length..]; //700mb takes ~1min to download on WiFi and only then starts playing.
      if (_blackList.Contains(Path.GetExtension(file).ToLower()) == false
#if DEBUG
        && 5_000_000 < fileinfo.Length && fileinfo.Length < 20_000_000
#endif
        )
        return file;
    }

    throw new Exception("No suitable media files found ▄▀▄▀▄▀▄▀▄▀▄▀");
  }
  async Task<(long durationInMs, string report)> StartPlayingMediaStream(Stream stream, DriveItem driveItem)
  {
    var media = new Media(_libVLC, new StreamMediaInput(stream));

    ArgumentNullException.ThrowIfNull(VideoView1.MediaPlayer, "@@@@@@@@@@@@@@@@@@");

    var playSucces = VideoView1.MediaPlayer.Play(media);
    var report2 = $"►{(playSucces ? "+" : "-")} ";

    VideoView1.MediaPlayer.Volume = _volumePerc;
    VideoView1.MediaPlayer.Mute = VideoView1.MediaPlayer.Volume != _volumePerc;

    var (durationMs, report) = await TryGetBetterDuration(driveItem, media);
    _currentShowTimeMS = Math.Min((int)durationMs, _maxMs);
    SetAnimeDurationInMS(_currentShowTimeMS);
    _sbIntroOutro?.Begin();

    report2 += ($"{report}{durationMs * .001,4:N0}s-durn: seekTo");
    if (durationMs > _maxShowTime.TotalMilliseconds)
    {
      var diffMs = durationMs - _maxShowTime.TotalMilliseconds;
      var seekToMs = _random.Next((int)diffMs);

      VideoView1.MediaPlayer?.SeekTo(TimeSpan.FromMilliseconds(seekToMs));
      report2 += ($"{seekToMs * .001,3:N0}s               .");
    }
    else if (durationMs > 0)
      report2 += ($"  °  :it's <{_maxShowTime.TotalSeconds,3}s prd");
    else
      report2 += ($"  °  Prorate this ext! ▄▀▄▀");

    return (durationMs, report2); // in ms
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
  static async Task<(long durationMs, string report)> TryGetBetterDuration(DriveItem driveItem, Media media)
  {
    const int maxTries = 101;
    var i = 1; for (; i < maxTries && media.Duration <= 0; i++) await Task.Delay(10);
    var rv = i < maxTries ? ($"{i,2} try") : "estimd";

    return media.Duration > 0
      ? (media.Duration, rv)
      : Path.GetExtension(driveItem.Name).ToLower() switch
      {
        ".m2ts" => ((driveItem.Size ?? 0) / (38208 * 1024 / 13000), rv),
        ".mts" => ((driveItem.Size ?? 0) / (20298 * 1024 / 13000), rv),
        _ => (0, "//todo"),
      };
  }

#pragma warning disable IDE0051 // Remove unused private members
  async Task Testingggggggg(string thm, string file)
#pragma warning restore IDE0051 // Remove unused private members
  {
    ArgumentNullException.ThrowIfNull(_graphServiceClient, nameof(_graphServiceClient));
    //var me = await graphServiceClient.Me.Request().GetAsync();
    Image1.Source = (await GetBipmapFromStream(await _graphServiceClient.Me.Photo.Content.Request().GetAsync())).bitmapImage;
    _ = await _graphServiceClient.Drive.Root.Request().Expand(thm).GetAsync();
    _ = await _graphServiceClient.Drive.Root.ItemWithPath("/Pictures").Request().Expand(thm).GetAsync();
    _ = await _graphServiceClient.Drive.Root.ItemWithPath(file).Request().Expand(thm).GetAsync();

    var items = await _graphServiceClient.Me.Drive.Root.Children.Request().GetAsync(); //tu: onedrive root folder items == 16 dirs.
    _ = items.ToList()[12].Folder;
  }
}