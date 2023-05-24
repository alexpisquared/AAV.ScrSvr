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

  async Task LoadWaitThenShowNext()
  {
    var file = GetNextMediaFile();

    try
    {
      ArgumentNullException.ThrowIfNull(_graphServiceClient, nameof(_graphServiceClient));
      ArgumentNullException.ThrowIfNull(_sbIntroOutro, nameof(_sbIntroOutro));

      var driveItem = await _graphServiceClient.Drive.Root.ItemWithPath(file).Request().Expand(_thumbnails).GetAsync();      //file = $"{.000001 * driveItem.Size,8:N2} mb                              {driveItem.Name}"; // Write($"** {.000001 * driveItem.Size,8:N2} mb   sec:{Stopwatch.GetElapsedTime(start).TotalSeconds,5:N2}");

      if (driveItem.Video is null && driveItem.Image is null && driveItem.Photo is null)
        return;

      var start = Stopwatch.GetTimestamp();
      var taskStream = _graphServiceClient.Drive.Root.ItemWithPath(file).Content.Request().GetAsync();
      var taskDelay = Task.Delay(_periodCurrent, _cancellationTokenSource.Token);
      await Task.WhenAll(taskStream, taskDelay);
      // Write($"{Stopwatch.GetElapsedTime(start).TotalSeconds,8:N2}s ");

      _periodCurrent = _sbIntroOutro.Duration.TimeSpan;
      VideoView1.MediaPlayer?.Stop();
#if DEBUG
      System.Media.SystemSounds.Beep.Play();
#endif

      if (driveItem.Video is not null)
      {
        var durationInMs = await StartPlayingMediaStream(taskStream.Result, driveItem);
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
        ReportTC.Text = $"{.000001 * driveItem.Size,8:N2} mb  !!! PHOTO a new FILE !!!    {driveItem.Photo.CameraMake}x{driveItem.Photo.CameraModel}    {driveItem.Name}   ▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄";
        ImageView1.Source = await GetBipmapFromStream(taskStream.Result);
        VideoView1.Visibility = Visibility.Hidden;
        ImageView1.Visibility = Visibility.Visible;
      }
      else
      {
        ReportTC.Text = $"{.000001 * driveItem.Size,8:N2} mb  !!! NOT A MEDIA FILE !!!    {driveItem.Name}   ▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄";
      }

      ReportTL.Text = $"{driveItem.CreatedDateTime:yyyy-MM-dd}";
      //ReportTR.Text = $"{driveItem.LastModifiedDateTime:yyyy-MM-dd}";
      //ReportBL.Text = $"{driveItem.CreatedBy}    {driveItem.CreatedByUser}  ";
      //ReportBR.Text = $"{driveItem.LastModifiedBy}    {driveItem.LastModifiedByUser}  ";

      _sbIntroOutro.Begin();

      Write($"{DateTime.Now:HH:mm:ss.f}  dnld {.000001 * driveItem.Size,5:N1} mb in{Stopwatch.GetElapsedTime(start).TotalSeconds,4:N0} s.{driveItem.Name,52}  ");

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
  async Task<long> StartPlayingMediaStream(Stream stream, DriveItem driveItem)
  {
    var media = new Media(_libVLC, new StreamMediaInput(stream));

    _ = VideoView1.MediaPlayer?.Play(media); // non-blocking

    for (int i = 0; i < 3 && media.Duration <= 0; i++) await Task.Delay(333);

    var durationMs = GetDuration(driveItem, media);
    Write($"    Duration:{durationMs * .001,4:N0} s  ==> starting from ");
    if (durationMs > _periodCurrent.TotalMilliseconds)
    {
      var diffMs = durationMs - _periodCurrent.TotalMilliseconds;
      var seekToMs = _random.Next((int)diffMs);

      //await Task.Delay(2_500);
      VideoView1.MediaPlayer?.SeekTo(TimeSpan.FromMilliseconds(seekToMs));
      Write($"{seekToMs * .001,4:N0} s. \n");
    }
    else if (durationMs > 0)
    {
      WriteLine($"the START.   :it is < period: {_periodCurrent.TotalSeconds,4:N0} s. ");
      await Task.Delay((int)durationMs);
      _cancellationTokenSource.Cancel();
    }
    else
      WriteLine($"the START.   Prorate this extension!!! ▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄");

    return durationMs; // in ms
  }

  static long GetDuration(DriveItem driveItem, Media media)
  {
    if (media.Duration > 0)
      return
        media.Duration;        
   
    switch (Path.GetExtension( driveItem.Name).ToLower())
    {
      case ".mts": return (driveItem.Size ?? 0) / (20298 * 1024 / 13000); // 20298 * 1024 bytes ~~ 13000 ms
      default: return 0;
    }
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
