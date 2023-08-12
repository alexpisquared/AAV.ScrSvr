namespace MSGraphSlideshow;
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public partial class MsgSlideshowUsrCtrl
{
  const int _volumePerc = 5;
  const string _thumbnails = "thumbnails,children($expand=thumbnails)";
  Storyboard? _sbIntroOutro;
  GraphServiceClient? _graphServiceClient;
  CancellationTokenSource? _cancellationTokenSource;
  readonly Random _random = new(Guid.NewGuid().GetHashCode());
  readonly LibVLC? _libVLC;
  readonly SizeWeightedRandomPicker _sizeWeightedRandomPicker = new(OneDrive.Folder("Pictures"));
  readonly AuthUsagePOC _authUsagePOC = new();
#if DEBUG
  const int _maxMs = 15_000;
#else
  const int _maxMs = 59_000;
#endif
  int _currentShowTimeMS = 0;
  bool _alreadyPrintedHeader;
  string? _pathfile;

  public MsgSlideshowUsrCtrl()
  {
    try
    {
      InitializeComponent();

      _libVLC = new LibVLC(enableDebugLogs: true);
      VideoView1.MediaPlayer = new MediaPlayer(_libVLC) { Volume = _volumePerc }; // percent
      VideoView1.MediaPlayer.EndReached += OnEndReached;
      _ = new DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Normal, new EventHandler(OnMoveProgressBarTimerTick), Dispatcher.CurrentDispatcher);

      ReportBC.FontSize = 48; 
      ReportBC.Content = VersionHelper.CurVerStr("MM.dd-HH:mm");
    }
    catch (Exception ex)
    {
      ReportBC.FontSize = 60;
      ReportBC.Content = $"■ {ex.Message}";
      Logger.Log(LogLevel.Error, ex.Message);
      //not good here: ex.Pop(_logger, $"ERR {ReportBC.Content} ");
      //if (Debugger.IsAttached) Debugger.Break();      //else      await Task.Delay(15_000);
    }
  }

  void SetAnimeDurationInMS(long ms)
  {
    var showTime = new System.Windows.Duration(TimeSpan.FromMilliseconds(ms));

    ((Storyboard)FindResource("_sbIntroOutro")).Duration =
    ((DoubleAnimation)FindResource("_d2IntroOutro")).Duration =
    ((DoubleAnimation)FindResource("_d3IntroOutro")).Duration = showTime;
  }
  public static readonly DependencyProperty ClientIdProperty = DependencyProperty.Register("ClientId", typeof(string), typeof(MsgSlideshowUsrCtrl)); public string ClientId { get => (string)GetValue(ClientIdProperty); set => SetValue(ClientIdProperty, value); } // public string ClientId { get; set; }
  public string? ClientNm { get; set; }
  public bool ScaleToHalf { get; set; }
  ILogger? _logger; public ILogger Logger => _logger ??= (DataContext as dynamic)?.Logger ?? SeriLogHelper.CreateFallbackLogger<MsgSlideshowUsrCtrl>();
  IBpr? _bpr;  public IBpr? Bpr => _bpr ??= (DataContext as dynamic)?.Bpr;

  void OnMoveProgressBarTimerTick(object? s, EventArgs e) => ProgressBar2.Value = VideoView1.MediaPlayer?.Position ?? 0;
  async void OnLoaded(object s, RoutedEventArgs e)
  {
    if (DesignerProperties.GetIsInDesignMode(this)) return; //tu: design mode

    try
    {
      _sbIntroOutro = (Storyboard)FindResource("_sbIntroOutro");

      this.FindParentWindow().WindowState = WindowState.Minimized;
      var (success, report, result) = await _authUsagePOC.LogInAsync(ClientId);
      this.FindParentWindow().WindowState = WindowState.Normal;
      if (!success)
      {
        ReportBC.Content = $"{ClientNm}:- {report}";
        Logger.Log(LogLevel.Information, $"° {report}");
      }

      ArgumentNullException.ThrowIfNull(result, $"▀▄▀▄▀▄ {report}");
      ArgumentNullException.ThrowIfNull(_sbIntroOutro, $"▀▄▀▄▀▄ {nameof(_sbIntroOutro)}");

      _graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
      {
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
        await Task.CompletedTask;
      }));

      while (VideoView1.MediaPlayer is not null) // forever
      {
        if (chkIsOn.IsChecked == true)
          chkIsOn.IsChecked = await LoadWaitThenShowNext();
        else
          await Task.Delay(100);
      }
    }
    catch (Exception ex)
    {
      ReportBC.FontSize = 60;
      ReportBC.Content = $"■ {ex.Message}";
      ex.Pop(Logger, $"ERR {ReportBC.Content} ");
    }
    finally
    {
      this.FindParentWindow().WindowState = WindowState.Normal;
    }
  }
  void OnMute(object s, RoutedEventArgs e) { if (VideoView1.MediaPlayer is not null) VideoView1.MediaPlayer.Mute = ((System.Windows.Controls.CheckBox)s).IsChecked ?? false; }
  void OnPrev(object s, RoutedEventArgs e) { }
  void OnNext(object s, RoutedEventArgs e) => _cancellationTokenSource?.Cancel();
  void OnEndReached(object? s, EventArgs e) => _cancellationTokenSource?.Cancel();
  async void OnSignOut(object s, RoutedEventArgs e)
  {
    ReportBC.Content = await _authUsagePOC.SignOut(); // LogOut
    System.Windows.Application.Current.Shutdown();
  }
  void OnSizeChanged(object s, SizeChangedEventArgs e)
  {
    if (!ScaleToHalf) return;

    // this is a hack to make the video controls fit the video ONLY when used by OleksaScrSvr.
    // not sure why, but this must be done in code behind and this: <Grid ... VerticalAlignment="Top" HorizontalAlignment="Left" Margin="16" Tag="16 is just about right">    
    GridVideoControls.Width = e.NewSize.Width / 2;
    GridVideoControls.Height = e.NewSize.Height / 2;
    GridVideoControls.VerticalAlignment = VerticalAlignment.Top;
    GridVideoControls.HorizontalAlignment = HorizontalAlignment.Left;
    GridVideoControls.Margin = new Thickness(16); // 16 is just about right for the OleksaScrSvr case.
  }
  void OnDelete(object s, RoutedEventArgs e)
  {
    if (_pathfile is not null && System.IO.File.Exists(_pathfile))
      _ = System.Diagnostics.Process.Start("Explorer.exe", $"/select, \"{_pathfile}\"");
    else
      _ = MessageBox.Show($"Filename \n\n{_pathfile} \n\ndoes not exist", "Warning");
  }

  async Task<bool> LoadWaitThenShowNext()
  {
    string mediaType = "----", streamReport = "-- ", cancelReport = "";
    var dnldTime = TimeSpan.Zero;
    var driveItem = (DriveItem?)default;
    DateTimeOffset? takenDateTime = null;

    _pathfile = GetRandomSizeProportinalMediaFile();

    try
    {
      ArgumentNullException.ThrowIfNull(_graphServiceClient, nameof(_graphServiceClient));
      ArgumentNullException.ThrowIfNull(_sbIntroOutro, nameof(_sbIntroOutro));

      driveItem = await _graphServiceClient.Drive.Root.ItemWithPath(_pathfile).Request().Expand(_thumbnails).GetAsync();      // ~200 ms    Write($"** {.000001 * driveItem.Size,8:N1}mb   sec:{Stopwatch.GetElapsedTime(start).TotalSeconds,5:N2}");
      if (driveItem.Video is null && driveItem.Image is null && driveItem.Photo is null)
        return true;

      HistoryL.Content = $"{.000001 * driveItem.Size,5:N1}";

      var taskStream = TaskDownloadStreamGraph(_pathfile); //todo: TaskDownloadStreamAPI($"https://graph.microsoft.com/v1.0/me/drive/items/{driveItem.Id}/content"); //todo: Partial range downloads   from   https://learn.microsoft.com/en-us/graph/api/driveitem-get-content?view=graph-rest-1.0&tabs=http#code-try-1
      ArgumentNullException.ThrowIfNull(taskStream, nameof(taskStream));

      try
      {
        _cancellationTokenSource = new();
        var taskDelay = Task.Delay(_currentShowTimeMS, _cancellationTokenSource.Token);
        await Task.WhenAll(taskStream, taskDelay);
        dnldTime = taskStream.Result.dnldTime;
      }
      catch (OperationCanceledException) { cancelReport = "Canceled ~end reached"; }
      catch (Exception ex)
      {
        ReportBC.Content = $"{ClientNm}:- {ex.Message}  {.000001 * driveItem.Size,5:N1}mb   {driveItem.Name}";
        Logger.Log(LogLevel.Error, $"ERR inn {ReportBC.Content} ");
        Bpr?.Error(); // System.Media.SystemSounds.Hand.Play();

        if (Debugger.IsAttached) Debugger.Break();        //else        //  await Task.Delay(15_000);
      }
      finally { _cancellationTokenSource?.Dispose(); _cancellationTokenSource = null; }

      ArgumentNullException.ThrowIfNull(VideoView1.MediaPlayer, "VideoView1.MediaPlayer ... @@@@@@@@@@@@@@@@@++++++++@");
      //if (VideoView1.MediaPlayer.CanPause == true)        VideoView1.MediaPlayer.Pause();
      VideoView1.MediaPlayer.Stop();

#if DEBUG
      Bpr?.Yes(); // System.Media.SystemSounds.Hand.Play();
#endif

      HistoryR.Content += $"\n{ReportBR.Content}";
      ReportBR.Content = $"{driveItem.Name}";
      ReportBL.Content = $"{.000001 * driveItem.Size,5:N1}";

      VideoInterval.Visibility = Visibility.Hidden;

      takenDateTime = EarliestDate(
        driveItem?.Photo?.TakenDateTime,
        driveItem?.CreatedDateTime,
        driveItem?.LastModifiedDateTime,
        driveItem?.FileSystemInfo?.CreatedDateTime,
        driveItem?.FileSystemInfo?.LastModifiedDateTime);

      ReportTL.Content = $"{takenDateTime:yyyy-MM-dd}";

      if (driveItem.Image is not null)
      {
        mediaType = $"img";
        ReportTR.Content = streamReport = $"{driveItem.Image.Width,6:N0} x {driveItem.Image.Height,-6:N0}";
        ImageView1.Source = (await GetBipmapFromStream(taskStream.Result.stream)).bitmapImage;
        ImageView1.Visibility = Visibility.Visible;
        SetAnimeDurationInMS(_maxMs);
        _sbIntroOutro?.Begin();
        ReportTL.Content = $"{driveItem?.Photo?.TakenDateTime ?? driveItem?.CreatedDateTime:yyyy-MM-dd}";
      }
      else if (driveItem.Video is not null)
      {
        mediaType = $"Video";
        var (durationInMs, isExact, report) = await StartPlayingMediaStream(taskStream.Result.stream, driveItem);
        ReportTR.Content = $"{(isExact ? ' ' : '~')}{durationInMs * .001,-3:N0}s";
        streamReport = $"{report}  Vol:{VideoView1.MediaPlayer.Volume}%";
        ImageView1.Visibility = Visibility.Hidden;
      }
      else if (driveItem.Photo is not null)
      {
        mediaType = $"■Photo■";
        ReportBC.Content = $"{ClientNm}:- {.000001 * driveItem.Size,8:N1}mb  ??? What to do with Photo? ??     {driveItem.Photo?.CameraMake} x {driveItem.Photo?.CameraModel}    {driveItem.Name}   ▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐";
        Logger?.Log(LogLevel.Information, $" !? {_pathfile}  {ReportBC.Content}  ");
        ImageView1.Source = (await GetBipmapFromStream(taskStream.Result.stream)).bitmapImage;
        VideoInterval.Visibility = Visibility.Hidden;
        ImageView1.Visibility = Visibility.Visible;
        ReportTL.Content = $"{takenDateTime:yyyy-MM-dd}";
      }
      else
      {
        mediaType = $"■ else ■";
        ReportBC.Content = $"{ClientNm}:- {.000001 * driveItem.Size,8:N1}mb  !!! NOT A MEDIA FILE !!!    {driveItem.Name}   ▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐";
        Logger?.Log(LogLevel.Information, $" !? {_pathfile}  {ReportBC.Content}  ");
        ReportTL.Content = $"{takenDateTime:yyyy-MM-dd}";
      }

      //Logger?.Log(LogLevel.Warning, $"xx  {takenDateTime:yy-MM-dd}  {_pathfile} ");

      ReportBC.FontSize = 4 + (ReportBC.FontSize / 2);

      return true;
    }
    catch (ServiceException ex)
    {
      ReportBC.FontSize = 60;
      ReportBC.Content = $"{ClientNm}:- {ex.Message}  {.000001 * driveItem?.Size,5:N1}mb   {driveItem?.Name ?? _pathfile}";
      ex.Pop(Logger, $"ERR out {ReportBC.Content} ");

      if (Debugger.IsAttached) Debugger.Break();      //else      await Task.Delay(15_000);
      return false;
    }
    catch (Exception ex)
    {
      ReportBC.FontSize = 60;
      ReportBC.Content = $"{ClientNm}:- {ex.Message}  {.000001 * driveItem?.Size,5:N1}mb   {driveItem?.Name ?? _pathfile}";
      ex.Pop(Logger, $"ERR out {ReportBC.Content} ");

      if (Debugger.IsAttached) Debugger.Break();      //else      await Task.Delay(15_000);
      return false;
    }
    finally
    {
      if (!_alreadyPrintedHeader)
      {
        _alreadyPrintedHeader = true;
        Logger?.Log(LogLevel.Information, "dld mb/sec  Media  len by to/drn s Posn%                                                     driveItem.Name  takenYMD  cancelReport  ");
      }

      Logger?.Log(LogLevel.Information, $"{.000001 * driveItem?.Size,6:N0}/{dnldTime.TotalSeconds,2:N0}{mediaType,8}  {streamReport,-26}{driveItem?.Name,62}  {takenDateTime:yy-MM-dd}  {cancelReport}");

      _currentShowTimeMS = _maxMs;
    }
  }

  DateTimeOffset EarliestDate(DateTimeOffset? takenDateTime, DateTimeOffset? createdDateTime1, DateTimeOffset? lastModifiedDateTime1, DateTimeOffset? createdDateTime2, DateTimeOffset? lastModifiedDateTime2)
  {
    var rb = new[] { takenDateTime, createdDateTime1, lastModifiedDateTime1, createdDateTime2, lastModifiedDateTime2, DateTimeOffset.Now }.Where(d => d.HasValue && d > new DateTimeOffset(new DateTime(1970, 01, 01))).Min(d => d.Value);
    Debug.WriteLine($"*/>  taken {takenDateTime,8:yy-MM-dd}   created {createdDateTime1:yy-MM-dd}   lastModified {lastModifiedDateTime1:yy-MM-dd}   file sys: created {createdDateTime2:yy-MM-dd}   lastModified {lastModifiedDateTime2:yy-MM-dd}  =>  {rb:yy-MM-dd HH:mm}");
    return rb;
  }

  async Task<(Stream stream, TimeSpan dnldTime)> TaskDownloadStreamGraph(string file)
  {
    ArgumentNullException.ThrowIfNull(_graphServiceClient, nameof(_graphServiceClient));
    var start = Stopwatch.GetTimestamp();
    var stream = await _graphServiceClient.Drive.Root.ItemWithPath(file).Content.Request().GetAsync();
    var dnldTm = Stopwatch.GetElapsedTime(start);

    return (stream, dnldTm);
  }
  async Task<(Stream stream, TimeSpan dnldTime)> TaskDownloadStreamAPI(string url)
  {
    var start = Stopwatch.GetTimestamp();
    var httpClient = new System.Net.Http.HttpClient();
    var graphRequest = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
    var response = await httpClient.SendAsync(graphRequest);
    var stream = await response.Content.ReadAsStreamAsync();
    var dnldTm = Stopwatch.GetElapsedTime(start);

    return (stream, dnldTm);
  }
  string GetRandomSizeProportinalMediaFile()
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
      //".mov",
      //".mp4",
      //".mpg",
      //".mpo",
      //".MPO",
      //".m2ts",
      //".mts",
      //".png",
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
      var pathfile = fileinfo.FullName[(OneDrive.Root.Length - Environment.UserName.Length + 5)..];      //file = @"C:\Users\alexp\OneDrive\Pictures\Main\_New\2013-07-14 Lumia520\Lumia520 014.mp4"[OneDrive.Root.Length..]; //100mb      //file = @"C:\Users\alexp\OneDrive\Pictures\Camera imports\2018-07\VID_20180610_191622.mp4"[OneDrive.Root.Length..]; //700mb takes ~1min to download on WiFi and only then starts playing.
      if (_blackList.Contains(Path.GetExtension(pathfile).ToLower()) == false
#if DEBUG
        && 2_000_000 < fileinfo.Length && fileinfo.Length < 6_000_000
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

    var report2 = report;
    if (durationMs > _currentShowTimeMS)
    {
      var diffMs = durationMs - _currentShowTimeMS;
      var seekToMSec = _random.Next((int)diffMs);
      var seekToPerc = (double)seekToMSec / durationMs;
      ProgressBar2.Value = seekToPerc;

      await Task.Delay(333);
#if DEBUG
      Bpr?.Yes(); // System.Media.SystemSounds.Hand.Play();
#endif

      VideoView1.MediaPlayer.SetPause(true);
      VideoView1.MediaPlayer.SeekTo(TimeSpan.FromMilliseconds(seekToMSec));
      VideoView1.MediaPlayer.SetPause(false);

      //while (VideoView1.MediaPlayer.Position < seekToPerc)      {        VideoView1.MediaPlayer.SeekTo(TimeSpan.FromMilliseconds(seekToMSec));        await Task.Delay(10);        Write("!");      }

      //_logger?.Log(LogLevel.Trace, $"      {seekToPerc,6:N2} % ~~ {TimeSpan.FromMilliseconds(seekToMSec):mm\\:ss}         <== new setting to !!!!!!!");
      //_logger?.Log(LogLevel.Trace, $"  Psn:{VideoView1.MediaPlayer.Position,6:N2} %         after ^^ ");

      report2 += $"{(int)(seekToMSec * .001),3}/{(int)(durationMs * .001),-3}";

#if DEBUG_SEEK
      await Task.Delay(500);
      report2 += $" {(VideoView1.MediaPlayer.Position <= seekToPerc ? "FAILS" : "+ + +")} dt:{(VideoView1.MediaPlayer.Position - seekToPerc) * 100,3:N1}%";
      Bpr?.Yes(); // System.Media.SystemSounds.Hand.Play();
#endif

      var k = 1000.0 / durationMs;
      rectnglStart.Width = seekToMSec * k;
      progressBar3.Width = _currentShowTimeMS * k;
      rectnglRest1.Width = (durationMs - seekToMSec - _currentShowTimeMS) * k;

      VideoInterval.Visibility = Visibility.Visible;
    }
    else if (durationMs > 0)
      report2 += $"  0/{durationMs * .001,-3:N0}";
    else
      report2 += $" ° :Prorate this ext! ▄▀▄▀";

    if (durationMs <= _currentShowTimeMS)
    {
      progressBar3.Width = 0;
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

    var bmp = new BitmapImage();
    bmp.BeginInit();
    bmp.StreamSource = memoryStream;
    bmp.EndInit();

    await Task.Yield();

    return (bmp, "++");
  }
  static async Task<(long durationMs, bool isExact, string report)> TryGetBetterDuration(DriveItem driveItem, Media media)
  {
    if (driveItem.Video.Duration > 0)
      return ((long)driveItem.Video.Duration, true, "drvItm");

    const int maxTries = 101;
    var i = 1; for (; i < maxTries && media.Duration <= 0; i++) await Task.Delay(10);
    var rv = i < maxTries ? $"{i,2} try" : "estimd";

    //Debug._logger?.Log(LogLevel.Trace, $" ------------- {driveItem.Video.Duration} == {media.Duration}");

    return media.Duration > 0
      ? (media.Duration, true, rv)
      : Path.GetExtension(driveItem.Name).ToLower() switch
      {
        ".m2ts" => ((driveItem.Size ?? 0) / (38208 * 1024 / 13000), false, rv),
        ".mts" => ((driveItem.Size ?? 0) / (20298 * 1024 / 13000), false, rv),
        _ => (0, false, "//todo"),
      };
  }

  async Task Testingggggggg(string thm, string file)
  {
    ArgumentNullException.ThrowIfNull(_graphServiceClient, nameof(_graphServiceClient));
    //var me = await graphServiceClient.Me.Request().GetAsync();
    ImageView1.Source = (await GetBipmapFromStream(await _graphServiceClient.Me.Photo?.Content.Request().GetAsync())).bitmapImage;
    _ = await _graphServiceClient.Drive.Root.Request().Expand(thm).GetAsync();
    _ = await _graphServiceClient.Drive.Root.ItemWithPath("/Pictures").Request().Expand(thm).GetAsync();
    _ = await _graphServiceClient.Drive.Root.ItemWithPath(file).Request().Expand(thm).GetAsync();

    var items = await _graphServiceClient.Me.Drive.Root.Children.Request().GetAsync(); //tu: onedrive root folder items == 16 dirs.
    _ = items.ToList()[12].Folder;
  }
}
/*
 To retrieve the download URL for a file, you can make a request that includes the @microsoft.graph.downloadUrl property. Here’s an example of how to retrieve the download URL for a file using the Microsoft Graph API:

using Microsoft.Graph;
using System.Threading.Tasks;

public async Task<string> GetDownloadUrl(string itemId)
{
    var graphClient = new GraphServiceClient(authProvider);

    var driveItem = await graphClient.Drives["{drive-id}"].Items[itemId]
        .Request()
        .Select("id,@microsoft.graph.downloadUrl")
        .GetAsync();

    return driveItem.DownloadUrl;
}
Copy
You can replace {drive-id} with the ID of the drive that contains the item you want to download. You can also replace itemId with the ID of the item you want to download.


To specify the range of bytes you want to download, you can use the Range header in your HTTP request. Here’s an example of how to download a range of bytes from a file using the Microsoft Graph API:

using Microsoft.Graph;
using System.Net.Http;
using System.Threading.Tasks;

public async Task TaskDownloadStreamAPI(string url)
{
    var httpClient = new HttpClient();
    var graphRequest = new HttpRequestMessage(HttpMethod.Get, url);
    graphRequest.Headers.Range = new RangeHeaderValue(100, 200);
    var response = await httpClient.SendAsync(graphRequest);
    var contentStream = await response.Content.ReadAsStreamAsync();
    // Do something with the stream.
}
Copy
You can replace url with the download URL for the file you want to download. The RangeHeaderValue constructor takes two parameters: the start and end positions of the byte range you want to download.

The Range header is defined in the HTTP/1.1 specification (RFC 2616) . The Range header is used to specify the range of bytes that the client wants to retrieve from the server. The server responds with a 206 Partial Content status code and sends the requested range of bytes in the response body.

So, yes, using the Range header to specify the range of bytes you want to download complies with RFC 2616.


 */