using System.ComponentModel;
using System.Threading;
using System.Windows.Media.Animation;

namespace MSGraphSlideshow;
public partial class MsgSlideshowUsrCtrl
{
  const string _thumbnails = "thumbnails,children($expand=thumbnails)";
  Storyboard? _sbIntroOutro;
  readonly Random _random = new(Guid.NewGuid().GetHashCode());
  GraphServiceClient? _graphServiceClient;
  readonly LibVLC _libVLC;
  TimeSpan _periodCurrent = TimeSpan.FromMicroseconds(22);
  readonly CancellationTokenSource _cancellationTokenSource = new();
  SizeWeightedRandomPicker _sizeWeightedRandomPicker = new(OneDrive.Folder("Pictures"));
  public string ClientId { get; set; } = "9ba0619e-3091-40b5-99cb-c2aca4abd04e";
#if DEBUG
#else
#endif

  public MsgSlideshowUsrCtrl()
  {
    InitializeComponent();
    _libVLC = new LibVLC();
    VideoView1.MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
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

    WriteLine($"Log in successful. Loading media files list...");

#if DEBUG
    // _sizeWeightedRandomPicker.Serialize();
#else
#endif

    if (DesignerProperties.GetIsInDesignMode(this)) return;

    while (true)
    {
      if (chkIsOn.IsChecked == true)
        await LoadWaitThenShowNext();
      else
        await Task.Delay(100);
    }
  }

  async Task LoadWaitThenShowNext()
  {
    var file = GetNextMediaFile();

    try
    {
      ArgumentNullException.ThrowIfNull(_graphServiceClient, nameof(_graphServiceClient));

      var driveItem = await _graphServiceClient.Drive.Root.ItemWithPath(file).Request().Expand(_thumbnails).GetAsync();      //file = $"{.000001 * driveItem.Size,8:N2} mb                              {driveItem.Name}"; // Write($"** {.000001 * driveItem.Size,8:N2} mb   sec:{Stopwatch.GetElapsedTime(start).TotalSeconds,5:N2}");

      if (driveItem.Video is null && driveItem.Image is null && driveItem.Photo is null)
        return;

      var start = Stopwatch.GetTimestamp();
      var taskStream = _graphServiceClient.Drive.Root.ItemWithPath(file).Content.Request().GetAsync();
      var taskDelay = Task.Delay(_periodCurrent, _cancellationTokenSource.Token);
      await Task.WhenAll(taskStream, taskDelay);
      Write($"{Stopwatch.GetElapsedTime(start).TotalSeconds,8:N2}s to get {file}");

      _periodCurrent = _sbIntroOutro.Duration.TimeSpan;
      VideoView1.MediaPlayer?.Stop();
#if DEBUG
      System.Media.SystemSounds.Beep.Play();
#endif

      if (driveItem.Video is not null)
      {
        var durationInMs = await StartPlayingMediaStream(taskStream.Result);
        ReportTC.Text = $"{.000001 * driveItem.Size,8:N2} mb    {durationInMs * .001:N0} sec    {driveItem.Name}";
        ImageView1.Visibility = Visibility.Hidden;
        VideoView1.Visibility = Visibility.Visible;
      }
      else if (driveItem.Image is not null)
      {
        ReportTC.Text = $"{.000001 * driveItem.Size,8:N2} mb    {driveItem.Image.Width:N0}x{driveItem.Image.Height:N0}    {driveItem.Name}";
        ImageView1.Source = await GetBipmapFromStream(taskStream.Result);
        VideoView1.Visibility = Visibility.Hidden;
        ImageView1.Visibility = Visibility.Visible;
      }
      else if (driveItem.Photo is not null)
      {
        ReportTC.Text = $"{.000001 * driveItem.Size,8:N2} mb  !!! PHOTO a new FILE !!!    {driveItem.Photo.CameraMake}x{driveItem.Photo.CameraModel}    {driveItem.Name}";
        ImageView1.Source = await GetBipmapFromStream(taskStream.Result);
        VideoView1.Visibility = Visibility.Hidden;
        ImageView1.Visibility = Visibility.Visible;
      }
      else
      {
        ReportTC.Text = $"{.000001 * driveItem.Size,8:N2} mb  !!! NOT A MEDIA FILE !!!    {driveItem.Name}";
      }

      ReportTL.Text = $"{driveItem.CreatedDateTime:yyyy-MM-dd}";
      //ReportTR.Text = $"{driveItem.LastModifiedDateTime:yyyy-MM-dd}";
      //ReportBL.Text = $"{driveItem.CreatedBy}    {driveItem.CreatedByUser}  ";
      //ReportBR.Text = $"{driveItem.LastModifiedBy}    {driveItem.LastModifiedByUser}  ";

      _sbIntroOutro.Begin();

      Write($"  {Stopwatch.GetElapsedTime(start).TotalSeconds,5:N1} = {.000001 * driveItem.Size / Stopwatch.GetElapsedTime(start).TotalSeconds,4:N1} mb/sec.    {driveItem.Name}  \n");

      ReportBC.Text = "";
    }
    catch (OperationCanceledException ex)
    {
      WriteLine($"\nNEXT was clicked for  {file}  {ReportTC.Text}  {ex.Message}\n");
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
  }
  string GetNextMediaFile()
  {
    for (var i = 0; i < _sizeWeightedRandomPicker.Count; i++)
    {
      var file = _sizeWeightedRandomPicker.PickRandomFile().FullName[(OneDrive.Root.Length - Environment.UserName.Length + 5)..];      //file = @"C:\Users\alexp\OneDrive\Pictures\Main\_New\2013-07-14 Lumia520\Lumia520 014.mp4"[OneDrive.Root.Length..]; //100mb      //file = @"C:\Users\alexp\OneDrive\Pictures\Camera imports\2018-07\VID_20180610_191622.mp4"[OneDrive.Root.Length..]; //700mb takes ~1min to download on WiFi and only then starts playing.
      if (!file.EndsWith(".nar"))
        return file;
    }

    throw new Exception("No suitable media files found ▄▀▄▀▄▀▄▀▄▀▄▀");
  }
  async Task<long> StartPlayingMediaStream(Stream stream)
  {
    var media = new LibVLCSharp.Shared.Media(_libVLC, new StreamMediaInput(stream));

    _ = VideoView1.MediaPlayer?.Play(media); // non-blocking

    for (int i = 0; i < 8 && media.Duration <= 0; i++) { await Task.Delay(1_000); }

    if (media.Duration > _periodCurrent.TotalMicroseconds)
    {
      var diffMs = media.Duration - _periodCurrent.TotalMicroseconds;
      var seekToMs = _random.Next((int)diffMs);

      VideoView1.MediaPlayer?.SeekTo(TimeSpan.FromMilliseconds(seekToMs));
      WriteLine($"    media.Duration: {media.Duration,8} ms ... starting from {seekToMs} ms.");
    }
    
    WriteLine($"    media.Duration: {media.Duration,8} ms ... starting from the START.");

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

  void OnNext(object sender, RoutedEventArgs e) => _cancellationTokenSource.Cancel();

  void OnClose(object sender, RoutedEventArgs e) => Close();

  async Task Testingggggggg(string thm, string file)
  {
    ArgumentNullException.ThrowIfNull(_graphServiceClient, nameof(_graphServiceClient));
    //var me = await graphServiceClient.Me.Request().GetAsync();
    Image1.Source = await GetBipmapFromStream(await _graphServiceClient.Me.Photo.Content.Request().GetAsync());
    _ = await _graphServiceClient.Drive.Root.Request().Expand(thm).GetAsync();
    _ = await _graphServiceClient.Drive.Root.ItemWithPath("/Pictures").Request().Expand(thm).GetAsync();
    _ = await _graphServiceClient.Drive.Root.ItemWithPath(file).Request().Expand(thm).GetAsync();

    var items = await _graphServiceClient.Me.Drive.Root.Children.Request().GetAsync(); //tu: onedrive root folder items == 16 dirs.
    _ = items.ToList()[12].Folder;
  }
}
