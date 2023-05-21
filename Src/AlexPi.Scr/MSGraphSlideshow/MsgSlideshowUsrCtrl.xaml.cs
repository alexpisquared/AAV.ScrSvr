namespace MSGraphSlideshow;
public partial class MsgSlideshowUsrCtrl
{
  const string _allFilesTxt = @"C:\g\Microsoft-Graph\Src\msgraph-training-uwp\DemoApp\Stuff\AllFiles.txt", _thumbnails = "thumbnails,children($expand=thumbnails)";
  readonly Random _random = new(DateTime.Now.Second);
  GraphServiceClient? _graphServiceClient;
  string[] _allFilesArray = Array.Empty<string>();
  readonly LibVLC _libVLC;
  int _periodCurrent = 50;
#if DEBUG
  const int _periodMs = 5_000;
#else
  const int _periodMs = 60_000;
#endif

  public MsgSlideshowUsrCtrl()
  {
    InitializeComponent();
    _libVLC = new LibVLC();
    VideoView1.MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
  }

  async void OnLoaded(object sender, RoutedEventArgs e)
  {
    var (success, report, result) = await new AuthUsagePOC().LogInAsync();
    if (!success)
    {
      ReportExcn.Text = ($"{report}");
      WriteLine($"ERROR: {report}");
    }

    ArgumentNullException.ThrowIfNull(result, nameof(result));

    _graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
    {
      requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
      await Task.CompletedTask;
    }));

        WriteLine($"Log in successful. Loading media files list...");

        var start1 = Stopwatch.GetTimestamp();
#if !DEBUG
    _allFilesArray = System.IO.File.ReadAllLines(_allFilesTxt); //
#else
    _allFilesArray = await OneDrive.GetFileNamesAsync("*.*"); // System.IO.File.WriteAllLines(_allFiles, allFiles);
#endif
    WriteLine($"** {_allFilesArray.Length,8:N0}  files in {Stopwatch.GetElapsedTime(start1).TotalSeconds,5:N1} sec.");

    while (true)
    {
      await LoadWaitThenShowNext();
    }
    //await Testingggggggg(thm, file);
  }

  async Task LoadWaitThenShowNext()
  {
    try
    {
      ArgumentNullException.ThrowIfNull(_graphServiceClient, nameof(_graphServiceClient));

      var file = ReportNext.Text = GetNextMediaFile();

      var driveItem = await _graphServiceClient.Drive.Root.ItemWithPath(file).Request().Expand(_thumbnails).GetAsync();
      ReportNext.Text = $"{.000001 * driveItem.Size,8:N2} mb                              {driveItem.Name}"; // Write($"** {.000001 * driveItem.Size,8:N2} mb   sec:{Stopwatch.GetElapsedTime(start).TotalSeconds,5:N2}");

      if (driveItem.Video is null && driveItem.Image is null && driveItem.Photo is null)
        return;

      var start = Stopwatch.GetTimestamp();
      var taskStream = _graphServiceClient.Drive.Root.ItemWithPath(file).Content.Request().GetAsync();
      var taskDelay = Task.Delay(_periodCurrent);
      await Task.WhenAll(taskStream, taskDelay);
      Write($"{Stopwatch.GetElapsedTime(start).TotalSeconds,8:N2}");

      _periodCurrent = _periodMs;
      VideoView1.MediaPlayer?.Stop();
#if DEBUG
      System.Media.SystemSounds.Beep.Play();
#endif

      if (driveItem.Video is not null)
      {
        var durationInMs = StartPlayingMediaStream(taskStream.Result);
        ReportCrnt.Text = $"{.000001 * driveItem.Size,8:N2} mb  {Stopwatch.GetElapsedTime(start).TotalSeconds:N1} sec    {durationInMs * .001:N0} sec    {driveItem.Name}";
        ImageView1.Visibility = Visibility.Hidden;
        VideoView1.Visibility = Visibility.Visible;
      }
      else if (driveItem.Image is not null)
      {
        ReportCrnt.Text = $"{.000001 * driveItem.Size,8:N2} mb  {Stopwatch.GetElapsedTime(start).TotalSeconds:N1} sec    {driveItem.Image.Width:N0}x{driveItem.Image.Height:N0}    {driveItem.Name}";
        ImageView1.Source = await GetBipmapFromStream(taskStream.Result);
        VideoView1.Visibility = Visibility.Hidden;
        ImageView1.Visibility = Visibility.Visible;
      }
      else if (driveItem.Photo is not null)
      {
        ReportCrnt.Text = $"{.000001 * driveItem.Size,8:N2} mb  {Stopwatch.GetElapsedTime(start).TotalSeconds:N1} sec    !!! PHOTO a new FILE !!!    {driveItem.Photo.CameraMake}x{driveItem.Photo.CameraModel}    {driveItem.Name}";
        ImageView1.Source = await GetBipmapFromStream(taskStream.Result);
        VideoView1.Visibility = Visibility.Hidden;
        ImageView1.Visibility = Visibility.Visible;
      }
      else
      {
        ReportCrnt.Text = $"{.000001 * driveItem.Size,8:N2} mb  {Stopwatch.GetElapsedTime(start).TotalSeconds:N1} sec    !!! NOT A MEDIA FILE !!!    {driveItem.Name}";
      }

      ReportTL.Text = $"{driveItem.CreatedDateTime:yyyy-MM-dd}";
      ReportTR.Text = $"{driveItem.LastModifiedDateTime:yyyy-MM-dd}";
      ReportBL.Text = $"{driveItem.CreatedBy}    {driveItem.CreatedByUser}  ";
      ReportBR.Text = $"{driveItem.LastModifiedBy}    {driveItem.LastModifiedByUser}  ";

      Write($"  {Stopwatch.GetElapsedTime(start).TotalSeconds,5:N1} = {.000001 * driveItem.Size / Stopwatch.GetElapsedTime(start).TotalSeconds,4:N1} mb/sec.    {driveItem.Name}  \n");
    }
    catch (Exception ex)
    {
      ReportExcn.Text = $"** ERROR: {ex.Message}\n  {ReportNext.Text}\n  {ReportCrnt.Text}"; 
      WriteLine($"\nERROR for  {ReportNext.Text}  {ReportCrnt.Text}  {ex.Message}\n"); 
      System.Media.SystemSounds.Hand.Play();

            if (Debugger.IsAttached)
                Debugger.Break;
            else
                await Task.Delay(15_000);
    }
  }
  string GetNextMediaFile()
  {
    for (var i = 0; i < _allFilesArray.Length; i++)
    {
      var next = _random.Next(_allFilesArray.Length);
      var file = _allFilesArray[next][(OneDrive.Root.Length - Environment.UserName.Length + 5)..];
      //file = @"C:\Users\alexp\OneDrive\Pictures\Main\_New\2013-07-14 Lumia520\Lumia520 014.mp4"[OneDrive.Root.Length..]; //100mb
      //file = @"C:\Users\alexp\OneDrive\Pictures\Camera imports\2018-07\VID_20180610_191622.mp4"[OneDrive.Root.Length..]; //700mb takes ~1min to download on WiFi and only then starts playing.
      if (!file.EndsWith(".nar"))
        return file;
    }

    throw new Exception("No suitable media files found ▄▀▄▀▄▀▄▀▄▀▄▀");
  }
  long StartPlayingMediaStream(Stream stream)
  {
    var media = new LibVLCSharp.Shared.Media(_libVLC, new StreamMediaInput(stream));

    _ = VideoView1.MediaPlayer?.Play(media); // non-blocking
    VideoView1.MediaPlayer?.SeekTo(TimeSpan.FromMilliseconds(media.Duration / 2));

    return media.Duration; // in ms
  }
  static async Task<BitmapImage> GetBipmapFromStream(Stream? stream)
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

    await Task.Delay(100);

    return bmp;
  }

  async void OnNext(object sender, RoutedEventArgs e) => await LoadWaitThenShowNext();
  void OnClose(object sender, RoutedEventArgs e) => Close();

  async Task Testingggggggg(string thm, string file)
  {
    ArgumentNullException.ThrowIfNull(_graphServiceClient, nameof(_graphServiceClient));
    //var me = await graphServiceClient.Me.Request().GetAsync();
    Image1.Source = await GetBipmapFromStream(await _graphServiceClient.Me.Photo.Content.Request().GetAsync());
    _ = await _graphServiceClient.Drive.Root.Request().Expand(thm).GetAsync();
    _ = await _graphServiceClient.Drive.Root.ItemWithPath("/Pictures").Request().Expand(thm).GetAsync();
    var driveItem4 = await _graphServiceClient.Drive.Root.ItemWithPath(file).Request().Expand(thm).GetAsync();

    var items = await _graphServiceClient.Me.Drive.Root.Children.Request().GetAsync(); //tu: onedrive root folder items == 16 dirs.
    _ = items.ToList()[12].Folder;
  }
}
