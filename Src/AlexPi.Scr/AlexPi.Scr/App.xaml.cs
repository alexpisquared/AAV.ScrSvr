namespace AlexPi.Scr;
public partial class App : System.Windows.Application
{
  readonly GlobalEventHandler _globalEventHandler = new();
  bool _showBackWindowMaximized = false;
  static TraceSwitch? CurTraceLevel;
  static readonly TraceSwitch AppTraceLevel_Config = new("CfgTraceLevelSwitch", "Switch in config file:  <system.diagnostics><switches><!--0-off, 1-error, 2-warn, 3-info, 4-verbose. --><add name='CfgTraceLevelSwitch' value='3' /> ");
  static readonly TraceSwitch AppTraceLevel_inCode = new("Verbose________Trace", "This is the trace for all               messages.") { Level = TraceLevel.Info };
  static readonly TraceSwitch AppTraceLevel_Warnng = new("ErrorAndWarningTrace", "This is the trace for Error and Warning messages.") { Level = TraceLevel.Warning };
  static readonly ushort _volume = (ushort)(DateTime.Now.Hour is > 8 and < 21 ? ushort.MaxValue : ushort.MaxValue / 16);
  static readonly SpeechSynth _synth;
  static readonly object _thisLock = new();
  static bool? _mustLogEORun = null;
  public static readonly DateTime StartedAt = DateTime.Now;
  public const int
#if DEBUG
    GraceEvLogAndLockPeriodSec = 06, _ScrSvrShowDelayMs = 500;
#else
    GraceEvLogAndLockPeriodSec = 60, _ScrSvrShowDelayMs = 10000;
#endif
  static App()
  {
    var cfg = new ConfigRandomizer();
    var key = cfg.GetValue("AppSecrets:MagicSpeech").Replace("ReplaceDeployReplace", string.Format("{0}{1}{0}79{1}8f86{1}3a6{1}f32d{0}", 4, 5)); // a silly primitive ... just for laughs.
    _synth = new(key, true, voice: cfg.GetRandomFromUserSection("VoiceF"), pathToCache: @$"C:\Users\{Environment.UserName}\OneDrive\Public\AppData\SpeechSynthCache\");
  }

  protected override void OnStartup(StartupEventArgs sea)
  {
    try
    {
      if (DevOps.IsDbg)
      {
        ////await ChimerAlt.FreqWalkUp();
        ////Bpr.BeepBgn3();
        ////await ChimerAlt.Wake(); // AAV.Sys.Helpers.Bpr.Wake();
        ////await App.SpeakAsync($"123");
        //await ChimerAlt.Chime(1);
        //await ChimerAlt.Chime(3);
        //Debugger.Break();
        ////await ChimerAlt.FreqWalkUpDn(70, 40, 50, 1.07);
        //await AlexPi.Scr.AltBpr.ChimerAlt.FreqWalk();
        //for (var i = 3; i < 14; i++) { await AlexPi.Scr.AltBpr.ChimerAlt.Chime(i); }
      }

      CurTraceLevel = //IsDbg ? 
        AppTraceLevel_inCode //: AppTraceLevel_Warnng
        ; // AppTraceLevel_Config; - App.config is not used in Net5.

      base.OnStartup(sea);

      _ = AAV.Sys.Helpers.Tracer.SetupTracingOptions("AlexPi.Scr", CurTraceLevel);
      WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}   {Environment.MachineName}.{Environment.UserDomainName}\\{Environment.UserName}   {VersionHelper.CurVerStr("")}   args: {string.Join(", ", sea.Args)}.");

      //Au2021: too choppy, unable to set intdividually for timeout indicator on slide how: Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = 3 }); //tu: anim CPU usage GLOBAL reduction!!! (Aug2019: 10 was almost OK and <10% CPU. 60 is the deafult)

      //todo: Current.DispatcherUnhandledException += WPF.Helpers.UnhandledExceptionHndlr.OnCurrentDispatcherUnhandledException;
      EventManager.RegisterClassHandler(typeof(TextBox), UIElement.GotFocusEvent, new RoutedEventHandler((s, re) => { (s as TextBox)?.SelectAll(); })); //tu: TextBox

      ShutdownMode = ShutdownMode.OnExplicitShutdown;

      switch (sea.Args.FirstOrDefault()?.ToLower(CultureInfo.InvariantCulture).Trim()[..(sea.Args[0].Length < 2 ? 1 : 2)])
      {
        default:
        case "na": CloseOnUnIdle = false; goto case "sb";      // ignore mouse & keys moves/presses - use like normal app.
        case "lo": WriteLine($"  LogMore is ON.              "); CurTraceLevel = new TraceSwitch("VerboseTrace", "This is the VERBOSE trace for all messages") { Level = System.Diagnostics.TraceLevel.Verbose }; goto case "/s";
        case "sb": _showBackWindowMaximized = false; break;     // Run the Screen Saver - Sans Background windows.
        case "/s": _showBackWindowMaximized = true; break;      // Run the Screen Saver.
        case "/p": showMiniScrSvr(sea.Args[1]); return;         // <HWND> - Preview Screen Saver as child of window <HWND>.
        case "/c": _ = new SettingsWindow().ShowDialog(); return;   // Show the Settings dialog box, modal to the foreground window.
        case "up":
        case "-u":
        case "/u": ShutdownMode = ShutdownMode.OnLastWindowClose; new UpTimeReview2().Show(); return;
      }

      showFullScrSvr_ScheduleArming();
    }
    catch (Exception ex)
    {
      _ = ex.Log(optl: "ASYNC void OnStartup()");
      ex.Pop("ASYNC void OnStartup()");
    }

    //tmi: WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - StartedAt):mm\\:ss\\.ff}    StartUp() - EOMethof.");
  }
  protected override void OnSessionEnding(SessionEndingCancelEventArgs e) { LogScrSvrUptimeOncePerSession("ScrSvr - Dn - App.OnSessionEnding().   "); WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff} ▄▀▄▀App.OnSessionEnding()"); base.OnSessionEnding(e); }
  //protected override void OnDeactivated(EventArgs e) { /* do not LogScrSvrUptimeOncePerSession() <- */ WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff} ▄▀▄▄▀▀▄▀App.OnDeactivated()  "); base.OnDeactivated(e); }
  protected override void OnExit(ExitEventArgs e)
  {
    WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}  App.OnExit() 1/3 => before  LogScrSvrUptimeOncePerSession(\"ScrSvr - Dn - App.OnExit() \");");

    LogScrSvrUptimeOncePerSession("ScrSvr - Dn - App.OnExit() ");
    base.OnExit(e);

    WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}  App.OnExit() 2/3 => before  Process.GetCurrentProcess().Kill(); ");
    Process.GetCurrentProcess().Kill();

    WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}  App.OnExit() 3/3 => never got here! ────────────────────────────");
    Environment.Exit(87);
    Environment.FailFast("Environment.FailFast");
  }

  public static void SpeakFaF(string msg, string voice = "") => Task.Run(async () => await SpeakAsync(msg, voice: voice)); 
  public static async Task SpeakAsync(string msg, string voice = "")         
  {
    if (AppSettings.Instance.IsSpeechOn)
      await _synth.SpeakAsync(msg, voice: voice);
  }

  public static void SayExe(string msg)                                      /**/ => SpeechSynth.SayExe(msg);

  public const int IdleTimeoutSec = 240; // this is by default for/before idle timeout kicks in.  
  public static int Ssto_GpSec => IdleTimeoutSec + GraceEvLogAndLockPeriodSec;  // ScreenSaveTimeOut + Grace Period

  public static void LogScrSvrUptimeOncePerSession(string msg)
  {
    Write($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}  ▓▓  {msg,-80}");

    lock (_thisLock)
    {
      if (_mustLogEORun == null)
        WriteLine($" ... not logged <- flag is not set .. must be too soon to log. ▒▒");
      else if (_mustLogEORun == false)
        WriteLine($" ... not logged <- flag is set to false .. means: already logged before. ▒▒");
      else
      {
        if (!DevOps.IsDbg)
        {
          _mustLogEORun = false;
          EvLogHelper.LogScrSvrEnd(App.StartedAt.AddSeconds(-IdleTimeoutSec), msg);
          SpeakFaF(msg);
        }

        WriteLine($" ... logged SUCCESS ... for Release only, though!!!  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓ ");
      }
    }
  }
  static void showMiniScrSvr(string args1)
  {
    var whndl = long.TryParse(args1, out var tempLong) ? new IntPtr(tempLong) : IntPtr.Zero;

    var miniSS = new MiniScrSvrWindow();

    var parentRectangle = RECT.GetClientRect(whndl);

    var hwndSourceParameters = new System.Windows.Interop.HwndSourceParameters
    {
      Width = parentRectangle.Right - parentRectangle.Left == 0 ? 152 : parentRectangle.Right - parentRectangle.Left,
      Height = parentRectangle.Bottom - parentRectangle.Top == 0 ? 112 : parentRectangle.Bottom - parentRectangle.Top,
      //ParentWindow = whndl,
      PositionX = 0,
      PositionY = 0,
      //WindowStyle = (int)(WindowStyle.VISIBLE | WindowStyle.CHILD | WindowStyle.CLIPCHILDREN)
    };

    if (whndl != IntPtr.Zero)
    {
      hwndSourceParameters.ParentWindow = whndl;
      hwndSourceParameters.WindowStyle = (int)(WindowStyle.VISIBLE | WindowStyle.CHILD | WindowStyle.CLIPCHILDREN);
    }
    else
      hwndSourceParameters.WindowStyle = (int)WindowStyle.VISIBLE;

    miniSS.Height = hwndSourceParameters.Height;
    miniSS.Width = hwndSourceParameters.Width;
    miniSS.Visibility = whndl == IntPtr.Zero ? Visibility.Visible : Visibility.Hidden;
    miniSS.Title = $"{VersionHelper.CurVerStr("")} - SANS FreqWalks \n\n  {miniSS.Width} x {miniSS.Height} \n\n     Slp: {AppSettings.Instance.AutoSleep}   Htr: {AppSettings.Instance.IsHeaterOn}";

    var _HwndSource = new HwndSource(hwndSourceParameters) { RootVisual = miniSS.LayoutRoot };
    _HwndSource.Disposed += (s, e) => miniSS.Close();
  }

  void showFullScrSvr_ScheduleArming()
  {
    _ = Task.Run(async () =>
    {
      var sj = new ConfigRandomizer();
      await SpeakAsync($"Hey, {sj.GetRandomFromUserSection("FirstName")}!");
      SpeakFaF($"{sj.GetRandomFromUserSection("Greetings")} ");
    });

    foreach (var screen in WinFormsControlLib.WinFormHelper.GetAllScreens()) new BackgroundWindow(_globalEventHandler).ShowOnTargetScreen(screen, _showBackWindowMaximized);

    new ControlPanel(_globalEventHandler).Show();
    if (AppSettings.Instance.IsSaySecOn)
    {
      if (DevOps.IsDbg)
      {
        _ = Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 5) * 1000)).ContinueWith(_ => SpeakAsync($"5"));
        _ = Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 4) * 1000)).ContinueWith(_ => SpeakAsync($"4"));
        _ = Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 3) * 1000)).ContinueWith(_ => SpeakAsync($"3"));
        _ = Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 2) * 1000)).ContinueWith(_ => SpeakAsync($"2"));
        _ = Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 1) * 1000)).ContinueWith(_ => SpeakAsync($"1"));
      }
      else
      {
        _ = Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 50) * 1000)).ContinueWith(_ => SpeakAsync($"50"));
        _ = Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 40) * 1000)).ContinueWith(_ => SpeakAsync($"40"));
        _ = Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 30) * 1000)).ContinueWith(_ => SpeakAsync($"30"));
        _ = Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 20) * 1000)).ContinueWith(_ => SpeakAsync($"20"));
        //puzzle: runs 50 sec delay for all and read all at that moment: for (var i = 50; i > 0; i -= 5)        Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - i) * 1000)).ContinueWith(_ => SpeakAsync($"{i}+{i}=x"));
      }
    }

    _ = Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 10) * 1000)).ContinueWith(_ => SpeakAsync($"Really?"));
    _ = Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 00) * 1000)).ContinueWith(ArmAndLegEvent());
  }

  static Action<Task> ArmAndLegEvent() => _ =>
  {
    _mustLogEORun = true;

    if (DevOps.IsDbg)
      SpeakAsync($"no event logging in debug mode.").Wait();
    else
      EvLogHelper.LogScrSvrBgn(App.Ssto_GpSec);

    _ = Task.Run(async () =>
    {
      if (AppSettings.Instance.AutoLocke && StartedAt == DateTime.MinValue) // suspended <= to simpify maint-ce at home office (2021)
      {
        SpeakFaF($"Locking in          {AppSettings.Instance.Min2Locke} minutes.");
        await Task.Delay(TimeSpan.FromMinutes(AppSettings.Instance.Min2Locke));
        //await ChimerAlt.WakeAudio(); // wake up monitor's audio.
        await SpeakAsync($" {AppSettings.Instance.Min2Locke} minutes has passed. Computer to be Locked in a minute ..."); //try to speak async so that dismissal by user was possible (i.e., not locked the UI):
        await Task.Delay(TimeSpan.FromSeconds(60));

        SpeakFaF($"Enforcing lock down now.");
        LockWorkStation();
      }
    });

    _ = Task.Run(async () =>
    {
      try
      {
        if (!AppSettings.Instance.AutoSleep)
        {
          await SpeakAsync("Armed! Sleepless mode.");
          return;
        }

        if (DevOps.IsDbg)
          SpeakFaF($"Armed!");

        await Task.Delay(TimeSpan.FromMinutes(AppSettings.Instance.Min2Sleep + .25));                                                                                  /**/ Write($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}  1/7  after  await Task.Delay({TimeSpan.FromMinutes(AppSettings.Instance.Min2Sleep + .25)}min);.\n"); //await ChimerAlt.WakeAudio(); // wake up monitor's audio.
        await SpeakAsync($"Hey! {(DateTime.Now - StartedAt + TimeSpan.FromSeconds(IdleTimeoutSec)).TotalMinutes:N0} minutes has passed. Turning off ...in a minute."); /**/ Write($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}  2/7  after  await SpeakAsync($'Hey! {(IdleTimeoutSec / 60) + AppSettings.Instance.Min2Sleep} minutes has passed. Sending computer to sleep ...in a minute.');.\n");
        await Task.Delay(TimeSpan.FromMinutes(1.15));                                                                                                                  /**/ Write($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}  3/7  after  await Task.Delay(TimeSpan.FromMinutes(1.15));.\n");
        await SpeakAsync($"{Environment.UserName}! Not sure if 30 seconds will be enough.");                                                                           /**/ Write($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}  4/7  after  await SpeakAsync($'...Not sure if 30 seconds will be enough\n"); ;
        await Task.Delay(TimeSpan.FromMinutes(0.50));                                                                                                                  /**/ Write($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}  5/7  after  await Task.Delay(TimeSpan.FromMinutes(1.2));.\n");        //await EvLogHelper.UpdateEvLogToDb(10, $"The Enforcing-Sleep moment.");
        LogScrSvrUptimeOncePerSession("ScrSvr - Dn - PC sleep enforced by AAV.scr!");                                                                                  /**/ Write($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}  6/7  after  LogScrSvrUptime.\n");
        SleepStandby();                                                                                                                                                /**/ Write($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}  7/7  after  sleepStandby();.\n");
      }
      catch (Exception ex) { ex.Pop("ASYNC void OnStartup()"); }
    });
  };

  static void SleepStandby(bool isDeepHyberSleep = false) { WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}\t Starting {(isDeepHyberSleep ? "Hibernating" : "LightSleeping")}:  SetSuspendState(); ..."); _ = SetSuspendState(hiberate: isDeepHyberSleep, forceCritical: false, disableWakeEvent: false); }

  Window? _cntrA; public Window CntrA => _cntrA ??= new ContainerA(_globalEventHandler);
  Window? _cntrB; public Window CntrB => _cntrB ??= new ContainerB(_globalEventHandler);
  Window? _cntrC; public Window CntrC => _cntrC ??= new ContainerC(_globalEventHandler);
  Window? _cntrD; public Window CntrD => _cntrD ??= new ContainerD(_globalEventHandler);
  Window? _cntrE; public Window CntrE => _cntrE ??= new ContainerE(_globalEventHandler);
  Window? _cntrF; public Window CntrF => _cntrF ??= new ContainerF(_globalEventHandler);
  Window? _cntrG; public Window CntrG => _cntrG ??= new ContainerG(_globalEventHandler);
  Window? _cntrH; public Window CntrH => _cntrH ??= new ContainerH(_globalEventHandler);
  Window? _cntrI; public Window CntrI => _cntrI ??= new ContainerI(_globalEventHandler);
  Window? _cntrJ; public Window CntrJ => _cntrJ ??= new ContainerJ(_globalEventHandler);
  Window? _cntrK; public Window CntrK => _cntrK ??= new ContainerK(_globalEventHandler);
  Window? _cntrL; public Window CntrL => _cntrL ??= new ContainerL(_globalEventHandler);

  public static bool CloseOnUnIdle { get; set; } = true;

  [Flags] enum WindowStyle { CLIPCHILDREN = 33554432, VISIBLE = 268435456, CHILD = 1073741824 }
  [DllImport("Powrprof.dll", CharSet = CharSet.Auto, ExactSpelling = true)] static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);
  [DllImport("user32")] public static extern void LockWorkStation();
}
/// Install-Package Expression.Blend.Sdk
/// Use for deployment:  Release + Any CPU  

//Rogers chat on Feb 10, 2023:
//Okay Alex all set I have submitted that for you! You should get a return label emailed to you within the next day or so.Alternatively even without the label you can simply pack your items in a box and bring it to a Canada Post location with the Return ID Number PR823007. Canada Post will provide you with a receipt and take care of things from there!
//03:58 PMDorian
//Sorry for the mixup Alex and Thanks for your patience while we got everything sorted I really appreciate it! Let me know if you had any other questions at all or anything else you wanted to look into today.