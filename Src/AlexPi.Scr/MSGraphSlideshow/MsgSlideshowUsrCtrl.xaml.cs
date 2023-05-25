namespace MSGraphSlideshow;
public partial class MsgSlideshowUsrCtrl
{
  const int _volumePerc = 26;
  const string _thumbnails = "thumbnails,children($expand=thumbnails)";
  Storyboard? _sbIntroOutro;
  readonly Random _random = new(Guid.NewGuid().GetHashCode());
  GraphServiceClient? _graphServiceClient;
  readonly LibVLC _libVLC;
  TimeSpan _periodCurrent = TimeSpan.FromMicroseconds(22);
  CancellationTokenSource _cancellationTokenSource = new();
  readonly SizeWeightedRandomPicker _sizeWeightedRandomPicker = new(OneDrive.Folder("Pictures"));

  public MsgSlideshowUsrCtrl()
  {
    InitializeComponent();
    _libVLC = new LibVLC(enableDebugLogs: true);
    VideoView1.MediaPlayer = new MediaPlayer(_libVLC) { Volume = _volumePerc }; // percent
    VideoView1.MediaPlayer.EndReached += OnEndReached;
    _ = new DispatcherTimer(TimeSpan.FromMicroseconds(50), DispatcherPriority.Background, new EventHandler(OnTimerTick), Dispatcher.CurrentDispatcher);
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

#if DEBUG
#else
#endif

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
  void OnNext(object sender, RoutedEventArgs e) => _cancellationTokenSource.Cancel();
  void OnEndReached(object? sender, EventArgs e) => _cancellationTokenSource.Cancel();

  async Task LoadWaitThenShowNext()
  {
    string adtn = "---", report2 = "---";
    var periodOrDnldTime = TimeSpan.Zero;
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

      var start = Stopwatch.GetTimestamp();
      var taskStream = _graphServiceClient.Drive.Root.ItemWithPath(file).Content.Request().GetAsync();
      var taskDelay = Task.Delay(_periodCurrent, _cancellationTokenSource.Token);
      await Task.WhenAll(taskStream, taskDelay);
      periodOrDnldTime = Stopwatch.GetElapsedTime(start);

      _periodCurrent = _sbIntroOutro.Duration.TimeSpan;
      VideoView1.MediaPlayer?.Stop();
#if DEBUG
      System.Media.SystemSounds.Beep.Play();
#endif

      if (driveItem.Image is not null)
      {
        ReportTC.Text = $"{.000001 * driveItem.Size,8:N1}mb    {driveItem.Image.Width,8:N0}x{driveItem.Image.Height,-8:N0}    {driveItem.Name}";
        adtn = $"Image";
        ImageView1.Source = (await GetBipmapFromStream(taskStream.Result)).bitmapImage;
        VideoView1.Visibility = Visibility.Hidden;
        ImageView1.Visibility = Visibility.Visible;
      }
      else if (driveItem.Video is not null)
      {
        var (durationInMs, report) = await StartPlayingMediaStream(taskStream.Result, driveItem);
        ReportTC.Text = $"{.000001 * driveItem.Size,8:N1}mb    {durationInMs * .001:N0}s-durn    {driveItem.Name,-52}";
        adtn = $"{durationInMs * .001,9:N0}s-durn";
        report2 = report;
        ImageView1.Visibility = Visibility.Hidden;
        VideoView1.Visibility = Visibility.Visible;
      }
      else if (driveItem.Photo is not null)
      {
        ReportTC.Text = $"{.000001 * driveItem.Size,8:N1}mb  ??? What to do with Photo? ??     {driveItem.Photo.CameraMake}x{driveItem.Photo.CameraModel}    {driveItem.Name}   ▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄";
        ImageView1.Source = (await GetBipmapFromStream(taskStream.Result)).bitmapImage;
        adtn = $"Photo ■ ■ ■";
        VideoView1.Visibility = Visibility.Hidden;
        ImageView1.Visibility = Visibility.Visible;
      }
      else
      {
        ReportTC.Text = $"{.000001 * driveItem.Size,8:N1}mb  !!! NOT A MEDIA FILE !!!    {driveItem.Name}   ▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐";
      }
    }
    catch (OperationCanceledException)
    {
      Write($"---Reached the end or NEXT was clicked   {ReportTC.Text}---");
      _cancellationTokenSource.Dispose();
      _cancellationTokenSource = new CancellationTokenSource();
    }
    catch (Exception ex)
    {
      ReportBC.Text = $"** ERROR: {ex.Message}\n  {file}";
      WriteLine($"\nERROR for  {file}  {ReportTC.Text}  {ex.Message}\n");
      System.Media.SystemSounds.Hand.Play();

      if (Debugger.IsAttached)
        Debugger.Break();
      else
        await Task.Delay(15_000);
    }
    finally
    {
      WriteLine($"{DateTime.Now:HH:mm:ss.f}  dnld{.000001 * driveItem?.Size,6:N1}mb in{periodOrDnldTime.TotalSeconds,3:N0}s{adtn,20}{driveItem?.Name,52}  {report2} ");
      ReportBC.Text = "";
      ReportTL.Text = $"{driveItem?.CreatedDateTime:yyyy-MM-dd}";
      //ReportTR.Text = $"{driveItem.LastModifiedDateTime:yyyy-MM-dd}";
      //ReportBL.Text = $"{driveItem.CreatedBy}    {driveItem.CreatedByUser}  ";
      //ReportBR.Text = $"{driveItem.LastModifiedBy}    {driveItem.LastModifiedByUser}  ";

      _sbIntroOutro?.Begin();
    }
  }
  string GetRandomMediaFile()
  {
    var _blackList = new string[]{
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

    for (var i = 0; i < _sizeWeightedRandomPicker.Count; i++)
    {
      var fileinfo = _sizeWeightedRandomPicker.PickRandomFile();
      var file = fileinfo.FullName[(OneDrive.Root.Length - Environment.UserName.Length + 5)..];      //file = @"C:\Users\alexp\OneDrive\Pictures\Main\_New\2013-07-14 Lumia520\Lumia520 014.mp4"[OneDrive.Root.Length..]; //100mb      //file = @"C:\Users\alexp\OneDrive\Pictures\Camera imports\2018-07\VID_20180610_191622.mp4"[OneDrive.Root.Length..]; //700mb takes ~1min to download on WiFi and only then starts playing.
      if (_blackList.Contains(Path.GetExtension(file).ToLower()) == false
        && 16_000_000 < fileinfo.Length && fileinfo.Length < 30_000_000)
        return file;
    }

    throw new Exception("No suitable media files found ▄▀▄▀▄▀▄▀▄▀▄▀");
  }
  async Task<(long durationInMs, string report)> StartPlayingMediaStream(Stream stream, DriveItem driveItem)
  {
    string rv;
    var media = new Media(_libVLC, new StreamMediaInput(stream));

    ArgumentNullException.ThrowIfNull(VideoView1.MediaPlayer, "@@@@@@@@@@@@@@@@@@");

    VideoView1.MediaPlayer.Volume = _volumePerc;
    var playSucces = VideoView1.MediaPlayer.Play(media);
    rv = ($"►{(playSucces ? "+" : "-")} ");

    var i = 1;
    for (; i < 32 && media.Duration <= 0; i++)
    {
      await Task.Delay(50);
    }

    rv += ($" got durn on i={i} try");

    VideoView1.MediaPlayer.Volume = _volumePerc;
    VideoView1.MediaPlayer.Mute = VideoView1.MediaPlayer.Volume != _volumePerc;

    var durationMs = TryGetBetterDuration(driveItem, media);
    rv += ($"    {durationMs * .001,4:N0}s-durn  vol:{VideoView1.MediaPlayer.Volume,2}%  ==> starting from ");
    if (durationMs > _periodCurrent.TotalMilliseconds)
    {
      var diffMs = durationMs - _periodCurrent.TotalMilliseconds;
      var seekToMs = _random.Next((int)diffMs);

      //await Task.Delay(2_500);
      VideoView1.MediaPlayer?.SeekTo(TimeSpan.FromMilliseconds(seekToMs));
      rv += ($"{seekToMs * .001,4:N0} s. ");
    }
    else if (durationMs > 0)
    {
      rv += ($"the START.   :it is < period: {_periodCurrent.TotalSeconds,4:N0} s. ");
      await Task.Delay((int)durationMs);
      _cancellationTokenSource.Cancel();
    }
    else
      rv += ($"the START.   Prorate this extension!!! ▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄");

    return (durationMs, rv); // in ms
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
  static long TryGetBetterDuration(DriveItem driveItem, Media media)
  {
    return media.Duration > 0
      ? media.Duration
      : Path.GetExtension(driveItem.Name).ToLower() switch
      {
        ".mts" => (driveItem.Size ?? 0) / (20298 * 1024 / 13000),// 20298 * 1024 bytes ~~ 13000 ms
        _ => 0,
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