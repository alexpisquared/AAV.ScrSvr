using Azure.Identity;

namespace MSGraphSlideshow;
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public partial class MsgSlideshowUsrCtrl
{
  const int _veryQuiet = 33; // 9 is already hardly hearable on max. 26 for Nuc2 is still low since it is usually on 10 there already.
  const string _thumbnails = "thumbnails,children($expand=thumbnails)";
  Storyboard? _sbIntroOutro;
  GraphServiceClient? _graphServiceClient;
  CancellationTokenSource? _cancellationTokenSource;
  readonly Random _random = new(Guid.NewGuid().GetHashCode());
  readonly LibVLC? _libVLC;
  readonly SizeWeightedRandomPicker _sizeWeightedRandomPicker = new(OneDrive.Folder("Pictures"));
  readonly AuthUsagePOC _authUsagePOC = new();
#if DEBUG
  const int _maxMs = 12_000;
#else
  const int _maxMs = 60_000;
#endif
  int _currentShowTimeMS = 0;
  bool _alreadyPrintedHeader, _notShutdown = true;
  string? _filename;

  public MsgSlideshowUsrCtrl()
  {
    try
    {
      InitializeComponent();

      _libVLC = new LibVLC(enableDebugLogs: true);
      VideoView1.MediaPlayer = new MediaPlayer(_libVLC) { Volume = _veryQuiet }; // percent
      VideoView1.MediaPlayer.EndReached += OnEndReached;
      _ = new DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Normal, new EventHandler(OnMoveProgressBarTimerTick), Dispatcher.CurrentDispatcher);

      ReportBC.FontSize = 48;
      ReportBC.Content = VersionHelper.CurVerStr;

      ScheduleShutdown(StandardLib.Consts.ScrSvrPresets.MinToPcSleep);
    }
    catch (Exception ex)
    {
      ReportBC.FontSize = 16;
      ReportBC.Content = $"■ {ex.Message}";
      Logger.Log(LogLevel.Error, ex.Message);
      //not good here: ex.Pop(_logger, $"ERR {ReportBC.Content} ");
    }
  }

  void SetAnimeDurationInMS(long ms)
  {
    var duration = new System.Windows.Duration(TimeSpan.FromMilliseconds(ms));

    ((Storyboard)FindResource("_sbIntroOutro")).Duration =
    ((DoubleAnimation)FindResource("_d2IntroOutro")).Duration =
    ((DoubleAnimation)FindResource("_d3IntroOutro")).Duration = duration;
  }
  public static readonly DependencyProperty ClientIdProperty = DependencyProperty.Register("ClientId", typeof(string), typeof(MsgSlideshowUsrCtrl)); public string ClientId { get => (string)GetValue(ClientIdProperty); set => SetValue(ClientIdProperty, value); } // public string ClientId { get; set; }
  public static readonly DependencyProperty ClientSecretProperty = DependencyProperty.Register("ClientSecret", typeof(string), typeof(MsgSlideshowUsrCtrl)); public string ClientSecret { get => (string)GetValue(ClientSecretProperty); set => SetValue(ClientSecretProperty, value); } // public string ClientSecret { get; set; }
  public bool ScaleToHalf { get; set; }
  public void ScheduleShutdown(double minToPcSleep)
  {
    _ = Task.Run(async () =>
    {
      await Task.Delay(TimeSpan.FromMinutes(minToPcSleep));
    }).
    ContinueWith(_ =>
    {
      ShutdownIndicatorStart();
    }, TaskScheduler.FromCurrentSynchronizationContext());
  }

  ILogger? _logger; public ILogger Logger => _logger ??= (DataContext as dynamic)?.Logger ?? SeriLogHelper.CreateLogger<MsgSlideshowUsrCtrl>();
  IBpr? _bpr; public IBpr? Bpr => _bpr ??= (DataContext as dynamic)?.Bpr;
  SpeechSynth? _synth; public SpeechSynth? Synth => _synth ??= (DataContext as dynamic)?.Synth;

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
        ReportBC.Content = $"{_filename}:- {report}";
        Logger.Log(LogLevel.Information, $"° {report}");
      }

      ArgumentNullException.ThrowIfNull(result, $"▀▄▀▄▀▄ {report}");
      ArgumentNullException.ThrowIfNull(_sbIntroOutro, $"▀▄▀▄▀▄ {nameof(_sbIntroOutro)}");












      var clientSecretCredential = new ClientSecretCredential(tenantId: "common", ClientId, clientSecret: ClientSecret, new TokenCredentialOptions { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud });

      _graphServiceClient = new GraphServiceClient(clientSecretCredential, new[] { "https://graph.microsoft.com/.default" }); // _graphServiceClient = new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) => { requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken); await Task.CompletedTask; }));

      while (_notShutdown)
      {
        if (chkIsOn.IsChecked == true)
          _ = await LoadWaitThenShowNext();
        else
        {
          ArgumentNullException.ThrowIfNull(Bpr, "Bpr... ■325");
          await Bpr.BeepAsync(300, sec: .500); ;
          await Task.Delay(2_000);
        }
      }
    }
    catch (Exception ex)
    {
      ReportBC.FontSize = 16;
      ReportBC.Content = $"■ {ex.Message}";
      ex.Pop(Logger, $"ERR {ReportBC.Content} ");
    }
    finally
    {
      this.FindParentWindow().WindowState = WindowState.Normal;
    }
  }
  void OnMute(object s, RoutedEventArgs e) { if (VideoView1.MediaPlayer is not null) VideoView1.MediaPlayer.Mute = ((CheckBox)s).IsChecked ?? false; }
  void OnLoud(object s, RoutedEventArgs e) { if (VideoView1.MediaPlayer is not null) VideoView1.MediaPlayer.Volume = ((CheckBox)s).IsChecked == true ? _veryQuiet : 88; }
  void OnPrev(object s, RoutedEventArgs e) { }
  void OnNext(object s, RoutedEventArgs e) => _cancellationTokenSource?.Cancel();
  void OnEndReached(object? s, EventArgs e) => _cancellationTokenSource?.Cancel();
  void OnSnapshotOld(object s, RoutedEventArgs e) => new GuiCapture(Logger).StoreActiveWindowScreenshotToFile("ManualTest_Old", false);
  void OnSnapshotNew(object s, RoutedEventArgs e) => new GuiCapture(Logger).StoreActiveWindowScreenshotToFile("ManualTest_New", true);
  void OnShutdown(object s, RoutedEventArgs e) { ((Button)s).Visibility = Visibility.Collapsed; ScheduleShutdown(.03); }

  void ShutdownIndicatorStart()
  {
    try
    {
      ((Storyboard)FindResource("sbFinal1m"))?.Begin();

      _cancellationTokenSource?.Cancel();
      if (VideoView1?.MediaPlayer?.IsPlaying == true) VideoView1?.MediaPlayer.Stop(); // hangs if is not playing
      chkIsOn.IsChecked = _notShutdown = false;
      vbFinal1m.Visibility = Visibility.Visible;

      ArgumentNullException.ThrowIfNull(VideoView1, "VideoView1... ■325");

      ImageView1.Visibility =
      VideoView1.Visibility = Visibility.Collapsed;
      Bpr?.Finish();
    }
    catch (Exception ex)
    {
      ReportBC.FontSize = 16;
      ReportBC.Content = $"■ {ex.Message}";
      ex.Pop(Logger, $"ERR {ReportBC.Content} ");
    }
  }
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
    if (_filename is not null && System.IO.File.Exists(_filename))
      _ = System.Diagnostics.Process.Start("Explorer.exe", $"/select, \"{_filename}\"");
    else
      _ = MessageBox.Show($"Filename \n\n{_filename} \n\ndoes not exist", "Warning");
  }

  async Task<bool> LoadWaitThenShowNext()
  {
    string mediaType = "----", streamReport = "-- ", cancelReport = "", allDates = "", thmb = "";
    var dnldTime = TimeSpan.Zero;
    var driveItem = (DriveItem?)default;
    DateTimeOffset? minDate = null;

    _filename = GetRandomSizeProportionalMediaFile();

    //pass:  Logger.Log(LogLevel.Warning, "Test");

    try
    {
      ArgumentNullException.ThrowIfNull(_graphServiceClient, nameof(_graphServiceClient));
      ArgumentNullException.ThrowIfNull(_sbIntroOutro, nameof(_sbIntroOutro));

      driveItem = await _graphServiceClient.Drives[""].Root.ItemWithPath(_filename).GetAsync();      // ~200 ms    Write($"** {.000001 * driveItem.Size,8:N1}mb   sec:{Stopwatch.GetElapsedTime(start).TotalSeconds,5:N2}");
      if (driveItem.Video is null && driveItem.Image is null && driveItem.Photo is null)
        return true;

      Trace.WriteLine(thmb = driveItem.Thumbnails[0].Large.Url);
      /*
https://chi01pap001files.storage.live.com/y4mgPw3S1CXM_o-MciLRv-kvsHUZsaTyaBoHJ1qtwVCgxCsfeHZW7Rg7NzdYcLOmrPrh5rERJ1vC1WIkGy_XDdFJ9XWyG60w1evS0G_cd71_VT_hShJMBfGPy7nwTPgfbizPLOOR-mt2veIElvIN9xVYa6k9bWvJ6cw_3flK4GONGQpNV1gLh-gaicXwwlOZieaBdILqIq9RbApaGeayEs_UHRAgoPs4PRFNv3RTEFTf1Q?width=176&height=132&cropmode=none
https://sat02pap002files.storage.live.com/y4mmV1qPhwV0gkRdceZ5M2ZzcAK5CrfwO0dr7M6ov6ALMUzQJVfj-LmiZ_FO0wS1jM9g06187J6cJyhQfjfZfRcYoRPXaVb-pCNSnEIfOr2QFIUfcm3iXRal0WMC2Dct16nOavwUZURc3rm_4TMashczqKYLLZVIQf10vFLYMQFtVyRVyvYWlLfqwkmOFB6b5QJN54yfl-IviYFiabxNFqqVpooYdvkjehbZ2iXzMsJrKM?width=176&height=132&cropmode=none
https://dsm04pap002files.storage.live.com/y4mgllQ0lUeEq3TbhrVCBKl_Pq23VLMzT1MqzqoMjXOLsPbIDQRFMuaC8h_TodzrJd1tju3HLjH9_LZfycSteKnROdQEdrNNoXdQjX3oq4N2pAIvQdZ6v8ZrCpTeK3JWMOtnWA-NHWrdM_XOKw4mV_P9qNGJeJXHziYR-dEVaqNrTGur_mx1rqUPeFgj2M0eyJ2owJcfMfaq8gpKgapRgn47fpMkhKslyz46RJBV8YgqEo?width=176&height=132&cropmode=none
https://dsm04pap002files.storage.live.com/y4mbsw7kRKcftBgFdyj6iJf3YoGJB6FjhJWtbjZp_fxvpOFUOdTMeY-hci8nxaF8_7m11q0M93dFWNyQAK2nkkd8xbZtkGX8cA7LWH0kKt3RPyR9Tf3_TF9mdG2-pnotPfxnQy_2xsuY_lFLePyerwUwuJDU3tVv-kmgLVrIjfWNcnpjPo0t6HuAGnc4Nmg13UNFxyJzjhCT8qrycF1GHQutHPCusN9VVRvhrwt5WZYzvQ?width=800&height=450&cropmode=none
https://dsm04pap002files.storage.live.com/y4mlMtZ0OTrAHRuk2lSRog-oEHo0FdIxBqahzXdTrJB4oRYjZfAKPmx3TTb_-R_BpRL-DDSyzIpQSjHpwAe1euoGqE7uGSh5TCzDV7TZFSexRyutDT2WT6B5YgJ2Pre8mm5q8abCATo53lK2xd-3R9Jder6-83e66ZBAF1rF_XsE8rtjZ8oDtetl8FBUa7guaJAtTSWjo5k8RrySAyM2AcCcnp0Xw_Agz-AZWdYjqjeMP8?width=800&height=600&cropmode=none
https://dsm04pap002files.storage.live.com/y4mW-Voj3_xwEPbWiWcuYXHfMWJx4siDgLNBLKnlKi2OWFASu6G36otUT2dnemhbUFGRvR7QrpQrAYDFqge_KXWVYF4QWmpjuTVVL9YjAHdPovFCiarQ3Jlu3jAvkz1c3LlmBPqtmXKCCKjzpSEgyFZsVlNXbZXLs9kY_D4s-QLtxGinXCIkEWSvncLB-kJa5UMu-lxfKWars3JFftIBNXzZW1oq66KrNiunrVkHKSMjog?width=800&height=450&cropmode=none
https://dsm04pap002files.storage.live.com/y4mdvexSNHnylvhqU4Tbowf5oiEgSdAHDvY2NF1TFTYoNG-1YBVqwRcfT7vnI6mgw_d5Qs5eojoqm9VqTUNOy6vn1lWYCX3BUelPNOPaiRFsHYkXC1m2MK5M8w5_pX4X46uFqwLlglA7p6l27HQs0clXGmaIOVHL1eduyABzPgFq6tB5sew8WapERQyIdlhgowmuHZTGg2V2bddzHKJ2c8NQpRsTb1zdvBpHM1TWtXZOiI?width=800&height=451&cropmode=none
https://ch3301files.storage.live.com/y4mhjal8TnOw7xVO_wnDljaBFZqR12IYyWxxZxJRuICXHaBqihS_TuvgLG6nRmpnaRJc-x-rRTm9Jcas42bOT2o60l0LbBg3HUwwtKNOgaRM12cQjEnnMlIIIUxUqzzfhr06NuyvMmN_Ts_ey6g6sS398XdCOtqbrrxlTwjlE6WPDP1Z9L1s8EzADBYJPsV886sn6OETqQ7ekQoW32gQ-Bc9vqJIKUpxDVJ1dhBGVqrwD4?width=800&height=450&cropmode=none
https://dsm04pap002files.storage.live.com/y4mu7LUeRWk8ATb8nJxGd_PpTjbTxOo_jY_pA5m_SUm5oDHu_Xdo7r16VMpn9DF-NLl2YGKTqahl-0LmbAvDjan5zgyOTGxi_ebJpSeXydyyFl6NUJU1L9D3NNVrAzIFWLvZ6gASursbKNNoKpzO9VrMnNskKgTafHMgnp4gY7GgKFuFM9hqWmCNXKLRtD2Ioz0BDRrN3UvjlVsg6GVcpmFxrO2kh79Ebxb0HoK5D3nneU?width=800&height=450&cropmode=none
https://dsm04pap002files.storage.live.com/y4mTG2tj7147QTuN5i77cXlz7_8w0UxhdLXu3vfPv8Yb9nAzVtsJHJN204bwngRpEMXlHFi1Qz5RhsaV2vZfTc3Xk_L3poxNQslT1ES036rnQZreHhXBJwQhHYZbxc_o4O46Oe2K7kDPTnhTmBiCMzxi_i14-jdWQ4SIG3ZgDplga9RJi1YCgaXWJePb-Xa_gn8Qq3ZSnh6eFe-iavEvEh-3pR47-ID3seCila9VZh75a4?width=800&height=451&cropmode=none
https://dsm04pap002files.storage.live.com/y4mRKHNrX-YhXbWPqv4Ixz3gH6IdzHtiM1oho6qTmlNrPzdwxrZWzypp95gRzhxx9ss51Pweuqt688KWLaRtaQXJQqZBtKRrY1Lpdbiaad7WLwe_nBY8zwXNWB1PBh5Pc6vTT7A7Nt3J9EQPcVGu18_vCqyPZXlmyRXSqLXehYmiUDT7_-d5_Odw_IwA7x758Q16aI2sj4SGMi_jx2K7liVHb0EJ9eMqKoNOHD0H7nTlgg?width=450&height=800&cropmode=none
https://dsm04pap002files.storage.live.com/y4m7kPrdpWp5_Hbr3NT48zolOUVDW6Kptk2aOGnXlwFHI-p-CHBeAftuG5pPONEOhV4dhXtjG4jrX2VlgTQe_TL6kvLow4I3oajmMD_Z_2P-rM-9TcWmbaEepXRwafEageKbAxa_V8_0icqSP-lm5Q70sNbwP2qELeHfbGvCU3zHi-ooRnaUvqkziOVSgbqmUc_aTNJD0xrFvDiIW-vMVIdtwT61o34C-QFuJdoFblsbZY?width=800&height=533&cropmode=none
https://chi01pap001files.storage.live.com/y4mb1b0drT4MbnDiSRBHRQ98y2otL-SGpdelVKHf_aujRoseNwjmwiEFoqHIILBmVQRX1QHZ7yk6c-hructLqhSqbpxawo7e_TN94YBGRPcv_Fz1rWnO3icDKUb16Lzv6T6jPuHTrf3UoQ0LKHryVfRgKKT_L0PQIYJ4d8utupHaqSOsPyCjEhpRkOSRQ8QwQtuIk4PsPNRT06LaYjnQwWZoFmazbSf2daFdVQJqv8SZik?width=800&height=600&cropmode=none
       */
      HistoryL.Content = $"{.000001 * driveItem.Size,5:N1}";

      var taskStream = TaskDownloadStreamGraph(_filename); //todo: TaskDownloadStreamAPI($"https://graph.microsoft.com/v1.0/me/drive/items/{driveItem.Id}/content"); //todo: Partial range downloads   from   https://learn.microsoft.com/en-us/graph/api/driveitem-get-content?view=graph-rest-1.0&tabs=http#code-try-1
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
        ReportBC.Content = $"{_filename}:- {ex.Message}  {.000001 * driveItem.Size,5:N1}mb   {driveItem.Name}";

        ex.Pop(Logger, $"ERR inn {ReportBC.Content}"); // Logger.Log(LogLevel.Error, $"ERR inn {ReportBC.Content} ");        Bpr?.Error(); // System.Media.SystemSounds.Hand.Play();
      }
      finally { _cancellationTokenSource?.Dispose(); _cancellationTokenSource = null; }

      if (VideoView1?.MediaPlayer?.IsPlaying == true) VideoView1?.MediaPlayer.Stop(); // hangs if is not playing  //if (VideoView1.MediaPlayer.CanPause == true)        VideoView1.MediaPlayer.Pause();

      HistoryR.Content += $"\n{ReportBR.Content}";
      ReportBR.Content = $"{driveItem.Name}";
      ReportBL.Content = $"{.000001 * driveItem.Size,5:N1}";

      VideoInterval.Visibility = Visibility.Hidden;

      var tr = MicrosoftGraphHelpers.GetEarliestDate(driveItem);
      minDate = tr.minDate;
      allDates = tr.report;

      ReportTL.Content = $"{minDate:yyyy-MM-dd}";

      ArgumentNullException.ThrowIfNull(VideoView1, "VideoView1... ■321");
      ArgumentNullException.ThrowIfNull(VideoView1.MediaPlayer, "VideoView1.MediaPlayer ... ■321");

      if (driveItem?.Image is not null)
      {
        mediaType = $"img";
        ReportTR.Content = streamReport = $"{driveItem.Image.Width,5:N0} x {driveItem.Image.Height,-6:N0} ";
        ImageView1.Source = (await GetBipmapFromStream(taskStream.Result.stream)).bitmapImage;
        ImageView1.Visibility = Visibility.Visible;
        VideoView1.Visibility = Visibility.Hidden;
        SetAnimeDurationInMS(_maxMs);
        _sbIntroOutro?.Begin();
        ReportTL.Content = $"{driveItem?.Photo?.TakenDateTime ?? driveItem?.CreatedDateTime:yyyy-MM-dd}";
      }
      else if (driveItem?.Video is not null)
      {
        mediaType = $"Video";
        if (_notShutdown && chkIsOn.IsChecked == true) // a waste ... I know.
        {
          var (durationInMs, isExact, report) = await StartPlayingMediaStream(taskStream.Result.stream, driveItem);
          ReportTR.Content = $"{(isExact ? ' ' : '~')}{durationInMs * .001,-3:N0}s";
          streamReport = $"{report}  Vol:{VideoView1.MediaPlayer?.Volume}%";
          ImageView1.Visibility = Visibility.Hidden;
          VideoView1.Visibility = Visibility.Visible;
        }
      }
      else if (driveItem?.Photo is not null)
      {
        mediaType = $"■Photo■";
        ReportBC.Content = $"{_filename}:- {.000001 * driveItem.Size,8:N1}mb  ??? What to do with Photo? ??     {driveItem.Photo?.CameraMake} x {driveItem.Photo?.CameraModel}    {driveItem.Name}   ▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐";
        Logger?.Log(LogLevel.Information, $" !? {_filename}  {ReportBC.Content}  ");
        ImageView1.Source = (await GetBipmapFromStream(taskStream.Result.stream)).bitmapImage;
        VideoInterval.Visibility = Visibility.Hidden;
        ImageView1.Visibility = Visibility.Visible;
        ReportTL.Content = $"{minDate:yyyy-MM-dd}";
      }
      else
      {
        mediaType = $"■ else ■";
        ReportBC.Content = $"{_filename}:- {.000001 * driveItem?.Size,8:N1}mb  !!! NOT A MEDIA FILE !!!    {driveItem?.Name}   ▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐▌▐";
        Logger?.Log(LogLevel.Information, $" !? {_filename}  {ReportBC.Content}  ");
        ReportTL.Content = $"{minDate:yyyy-MM-dd}";
      }

      ReportBC.FontSize = 4 + (ReportBC.FontSize / 2);

      return true;
    }
    catch (AuthenticationFailedException ex)
    { ReportBC.FontSize = 16; ReportBC.Content = $"{_filename}:- {ex.Message}  {.000001 * driveItem?.Size,5:N1}mb   {driveItem?.Name ?? _filename}"; ex.Pop(Logger, $"ERR out {ReportBC.Content} "); return false; }
    catch (ServiceException ex)
    { ReportBC.FontSize = 16; ReportBC.Content = $"{_filename}:- {ex.Message}  {.000001 * driveItem?.Size,5:N1}mb   {driveItem?.Name ?? _filename}"; ex.Pop(Logger, $"ERR out {ReportBC.Content} "); return false; }
    catch (Exception ex)
    { ReportBC.FontSize = 16; ReportBC.Content = $"{_filename}:- {ex.Message}  {.000001 * driveItem?.Size,5:N1}mb   {driveItem?.Name ?? _filename}"; ex.Pop(Logger, $"ERR out {ReportBC.Content} "); return false; }
    finally
    {
      if (driveItem is not null)
      {
        if (!_alreadyPrintedHeader) { _alreadyPrintedHeader = true; Logger?.Log(LogLevel.Information, "dld mb/sec  Media  len by to/drn s Posn%                                                     driveItem.Name  takenYMD  cancelReport              taken     created   lastModi  fsi.crea  fsi.last    the Earliest!!"); }

        Logger?.Log(LogLevel.Information, $"{.000001 * driveItem?.Size,6:N0}/{dnldTime.TotalSeconds,2:N0}{mediaType,8}  {streamReport,-26}{driveItem?.Name,62}  {minDate:yy-MM-dd}  {cancelReport,-26}{allDates}");

        var videoLogFile = OneDrive.Folder(@"Public\Logs\OleksaScrSvr.Video.log"); //nogo: ...= OneDrive.Folder(@"Documents\Logs\OleksaScrSvr.Video.log"); // logs for private use only :since URL is in the log.
        await System.IO.File.AppendAllTextAsync(videoLogFile, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\t{minDate:yyyy-MM-dd HH:mm}\t{.000001 * driveItem?.Size,6:N0}\t{new string('■', (int)(.00000001 * driveItem?.Size ?? 0)),-9}\t{driveItem.WebUrl.Replace("https://1drv.ms/i/s!AGmSfHgV-", "")}\t{_filename}\t{thmb}\n"); /////////////////////////////////////////////

        _currentShowTimeMS = _maxMs;
      }
    }
  }

  async Task<(Stream stream, TimeSpan dnldTime)> TaskDownloadStreamGraph(string file)
  {
    ArgumentNullException.ThrowIfNull(_graphServiceClient, nameof(_graphServiceClient));
    var start = Stopwatch.GetTimestamp();
    var stream = await _graphServiceClient.Drives[""].Root.ItemWithPath(file).Content.GetAsync();
    var dnldTm = Stopwatch.GetElapsedTime(start);

#if DEBUG
    Synth?.SpeakFAF("Got it!");
#endif

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
  string GetRandomSizeProportionalMediaFile()
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
      //".3gp",
      //".dng",
      //".jpg",
      //".mov",
      //".mp4",
      //".mpg",
      //".mpo",
      //".MPO",
      //".m2ts",
      //".mts",
      //".png",
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
#if DEBUG_
      var pathfile = 
          @"Pictures\2016-09\wp_ss_20160901_0005.png";
          /*
          @"Pictures/id.png"; // the only one works on WO
          @"Pictures\2016-09\WP_20160907_19_43_10_Pro.mp4";
          @"Pictures\2016-09\wp_ss_20160901_0006.png";
          @"Pictures\2016-09\wp_ss_20160913_0001.png";
          @"Pictures\2016-09\wp_ss_20160913_0002.png";
          @"Pictures\2016-09\wp_ss_20160913_0003.png";
          @"Pictures\2016-09\wp_ss_20160913_0004.png";
          @"Pictures\2016-09\wp_ss_20160915_0001.png"};
          */
#else
      var fileinfo = _sizeWeightedRandomPicker.PickRandomFile();
      var pathfile = fileinfo.FullName[OneDrive.Root.Length..];      //file = @"C:\Users\alexp\OneDrive\Pictures\Main\_New\2013-07-14 Lumia520\Lumia520 014.mp4"[OneDrives[""].Root.Length..]; //100mb      //file = @"C:\Users\alexp\OneDrive\Pictures\Camera imports\2018-07\VID_20180610_191622.mp4"[OneDrives[""].Root.Length..]; //700mb takes ~1min to download on WiFi and only then starts playing.
#endif
      if (_blackList.Contains(Path.GetExtension(pathfile).ToLower()) == false
#if DEBUG
        //&& 500_000_000 < fileinfo.Length && fileinfo.Length < 3_000_000_000     // a big 2gb file on Zoe's account
        //&&  100_000 < fileinfo.Length && fileinfo.Length < 2_000_000            // tiny pics mostly ... I hope.
        //&& 12_000_000 < fileinfo.Length && fileinfo.Length < 26_000_000         // small videos
        && 3_000_000 < fileinfo.Length && fileinfo.Length < 22_000_000            // 50/50 videos/pics mix
#endif
        )
        return pathfile;
    }

    throw new Exception("No suitable media files found ▄▀▄▀▄▀▄▀▄▀▄▀");
  }
  async Task<(long durationInMs, bool isExact, string report)> StartPlayingMediaStream(Stream stream, DriveItem driveItem)
  {
    var media = new Media(_libVLC ?? throw new ArgumentNullException("_libVLC"), new StreamMediaInput(stream));

    ArgumentNullException.ThrowIfNull(VideoView1.MediaPlayer, "■555");

    _ = VideoView1.MediaPlayer.Play(media);

#if DEBUG
    Synth?.SpeakFAF("Play!");
#endif

    VideoView1.MediaPlayer.Volume = _veryQuiet;
    VideoView1.MediaPlayer.Mute = false;

    var (durationMs, isExact, report) = await TryGetBetterDuration(driveItem, media);
    _currentShowTimeMS = Math.Min((int)durationMs, _maxMs);
    SetAnimeDurationInMS(_currentShowTimeMS);
    _sbIntroOutro?.Begin();

    var report2 = report;
    if (durationMs > _currentShowTimeMS)
    {
      VideoInterval.Visibility = Visibility.Visible;

      var diffMs = durationMs - _currentShowTimeMS;
      var seekToMSec = _random.Next((int)diffMs);
      var seekToPerc = (double)seekToMSec / durationMs;
      ProgressBar2.Value = seekToPerc;

      await Task.Delay(999); // to demo the seek to a better position.

      VideoView1.MediaPlayer.SetPause(true);
      VideoView1.MediaPlayer.SeekTo(TimeSpan.FromMilliseconds(seekToMSec));
      VideoView1.MediaPlayer.SetPause(false);

#if DEBUG
      Synth?.SpeakFAF("Seek!");
#endif

      report2 += $"{(int)(seekToMSec * .001),3}/{(int)(durationMs * .001),-3}";

#if DEBUG_SEEK
      await Task.Delay(500);
      report2 += $" {(VideoView1.MediaPlayer.Position <= seekToPerc ? "FAILS" : "+ + +")} dt:{(VideoView1.MediaPlayer.Position - seekToPerc) * 100,3:N1}%";
#endif

      var k = 1000.0 / durationMs;
      rectnglStart.Width = seekToMSec * k;
      progressBar3.Width = _currentShowTimeMS * k;
      rectnglRest1.Width = (durationMs - seekToMSec - _currentShowTimeMS) * k;
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
    ImageView1.Source = (await GetBipmapFromStream((await _graphServiceClient.Me.Photo.Content.GetAsync()))).bitmapImage;
    _ = await _graphServiceClient.Drives[""].Root.GetAsync();
    _ = await _graphServiceClient.Drives[""].Root.ItemWithPath("/Pictures").GetAsync();
    _ = await _graphServiceClient.Drives[""].Root.ItemWithPath(file).GetAsync();

    var items = await _graphServiceClient.Me.Drives[""].GetAsync(); //tu: onedrive root folder items == 16 dirs.
    //_ = items.ToList()[12].Folder;
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