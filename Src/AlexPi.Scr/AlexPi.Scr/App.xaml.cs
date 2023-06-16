namespace AlexPi.Scr;
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public partial class App : Application
{
  #region fields
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
  const string _unidle = "Un-idleable instance", _bts = "by Task Scheduler";
  public static readonly DateTime StartedAt = DateTime.Now;
  public const int
#if DEBUG
    GraceEvLogAndLockPeriodSec = 16, _ScrSvrShowDelayMs = 500, IdleTimeoutSec = 240; // this is by default for/before idle timeout kicks in.  
#else
    GraceEvLogAndLockPeriodSec = 60, _ScrSvrShowDelayMs = 10000, IdleTimeoutSec = 240; // this is by default for/before idle timeout kicks in.  
#endif
  readonly DateTimeOffset _appStarted = DateTimeOffset.Now;
  readonly IServiceProvider _serviceProvider;
  string _audit = "audit is unassigned";
  #endregion
  static App()
  {
    _cfg = new ConfigRandomizer();
    var key = _cfg.GetValue("AppSecrets:MagicSpeech").Replace("ReplaceDeployReplace", string.Format("{0}{1}{0}79{1}8f86{1}3a6{1}f32d{0}", 4, 5)); // a silly primitive ... just for laughs.
    _synth = new(key, true, voice: _cfg.GetRandomFromUserSection("VoiceF"), pathToCache: @$"C:\Users\{Environment.UserName}\OneDrive\Public\AppData\SpeechSynthCache\");
  }
  public App()
  {
    IServiceCollection services = new ServiceCollection();

    AppStartHelper.InitAppSvcs(services);

    _ = services.AddSingleton<IAddChild, UnCloseableWindow>();
    _ = services.AddSingleton<UnCloseableWindow>(s => new UnCloseableWindow(s.GetRequiredService<ILogger>(), s.GetRequiredService<IConfigurationRoot>(), s.GetRequiredService<IBpr>()));

    _serviceProvider = services.BuildServiceProvider();
  }
  protected override async void OnStartup(StartupEventArgs startupEventArgs)
  {
    try
    {
      CurTraceLevel = //IsDbg ? 
        AppTraceLevel_inCode //: AppTraceLevel_Warnng
        ; // AppTraceLevel_Config; - App.config is not used in Net5.

      base.OnStartup(startupEventArgs);

      _ = AAV.Sys.Helpers.Tracer.SetupTracingOptions("AlexPi.Scr", CurTraceLevel);
      WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}  {Environment.MachineName}.{Environment.UserDomainName}\\{Environment.UserName}  wai:{_cfg.GetValue("WhereAmI")}  {VersionHelper.CurVerStr("")}  args:{string.Join(",", startupEventArgs.Args)}.");

      //Au2021: too choppy, unable to set intdividually for timeout indicator on slide how: Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = 3 }); //tu: anim CPU usage GLOBAL reduction!!! (Aug2019: 10 was almost OK and <10% CPU. 60 is the deafult)

      //todo: Current.DispatcherUnhandledException += WPF.Helpers.UnhandledExceptionHndlr.OnCurrentDispatcherUnhandledException;
      EventManager.RegisterClassHandler(typeof(TextBox), UIElement.GotFocusEvent, new RoutedEventHandler((s, re) => { (s as TextBox)?.SelectAll(); })); //tu: TextBox

      ShutdownMode = ShutdownMode.OnExplicitShutdown;

      switch (startupEventArgs.Args.FirstOrDefault()?.ToLower(CultureInfo.InvariantCulture).Trim()[..(startupEventArgs.Args[0].Length < 2 ? 1 : 2)])
      {
        default:
        case "na": CloseOnUnIdle = false; goto case "sb";             // ignore mouse & keys moves/presses - use like normal app.
        case "un": // _unidle:                                        // mind   mouse & keys moves/presses - full scr saver mode, idle time counted.
        case "sb": _showBackWindowMaximized = false; break;           // Run the Screen Saver - Sans Background windows.
        case "/s": _showBackWindowMaximized = true; break;            // Run the Screen Saver.
        case "/p": showMiniScrSvr(startupEventArgs.Args[1]); return;  // <HWND> - Preview Screen Saver as child of window <HWND>.
        case "/c": _ = new SettingsWindow().ShowDialog(); return;     // Show the Settings dialog box, modal to the foreground window.
        case "up":
        case "-u":
        case "/u": ShutdownMode = ShutdownMode.OnLastWindowClose; new UpTimeReview2().Show(); return;
        case "lo": WriteLine($" LogMore is ON. "); CurTraceLevel = new TraceSwitch("VerboseTrace", "This is the VERBOSE trace for all messages") { Level = TraceLevel.Verbose }; goto case "/s";
      }
     
      if (Environment.GetCommandLineArgs().Any(a => a.Contains(_bts))) // if by scheduler   - wait for 1 minute to allow user to dismiss by mouse or keyboard.
        await Wait1minuteThenRelaunch();
      else                                                             // if not dismissed  - relaunch as Screen Saver in un-unidle-able mode by args "ScreenSaver"
        _ = FullScrSvrModeWithEventLoggin(Environment.GetCommandLineArgs().Any(a => a.Contains(_unidle)));
    }
    catch (Exception ex) { _ = ex.Log(optl: "ASYNC void OnStartup()"); ex.Pop("ASYNC void OnStartup()"); }
  }
  async Task Wait1minuteThenRelaunch()
  {
    _ = Task.Run(async () =>
    {
      var sj = new ConfigRandomizer();
      await SpeakAsync($"Hey, {sj.GetRandomFromUserSection("FirstName")}!");
      SpeakFaF($"{sj.GetRandomFromUserSection("Greetings")} ");
    });

    foreach (var screen in WinFormsControlLib.WinFormHelper.GetAllScreens()) new BackgroundWindow(_globalEventHandler).ShowOnTargetScreen(screen, showMaximized: true);

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

    await Task.Delay((GraceEvLogAndLockPeriodSec - 10) * 1000);
    await SpeakAsync($"Really?");

    WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}  Launching another instance lest be closed by unidling.");
    var me = Process.GetCurrentProcess();
    _ = Process.Start(me.MainModule?.FileName ?? "Notepad.exe", _unidle);
    Shutdown();
  }
  async Task FullScrSvrModeWithEventLoggin(bool skipLogging)
  {
    _mustLogEORun = true;

    if (DevOps.IsDbg || skipLogging)
      await SpeakAsync($"event logging is off.");
    else
      EvLogHelper.LogScrSvrBgn(App.Ssto_GpSec);

    foreach (var screen in WinFormsControlLib.WinFormHelper.GetAllScreens()) new BackgroundWindow(_globalEventHandler).ShowOnTargetScreen(screen, _showBackWindowMaximized);

    new ControlPanel(_globalEventHandler).Show();

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
        await Task.Delay(TimeSpan.FromMinutes(0.50));                                                                                                                  /**/ Write($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}  5/7  after  await Task.Delay(TimeSpan.FromMinutes(1.2));.\n");
        LogScrSvrUptimeOncePerSession("ScrSvr - Dn - PC sleep enforced by AAV.scr!");                                                                                  /**/ Write($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}  6/7  after  LogScrSvrUptime.\n");
        SleepStandby();                                                                                                                                                /**/ Write($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}  7/7  after  sleepStandby();.\n");
      }
      catch (Exception ex) { ex.Pop("ASYNC void OnStartup()"); }
    });
  }
  bool ShutdownIfAlreadyRunning()
  {
    var me = Process.GetCurrentProcess();
    if (Process.GetProcessesByName(me.ProcessName).Any(p => p.Id != me.Id && p.MainModule?.FileName == me.MainModule?.FileName))
    {
      WriteLine($"  Another instance is running.  Shutting down this instance.");
      Shutdown();
      return false;
    }

    if (Environment.GetCommandLineArgs().Any(a => a.Contains(_bts)))
    {
      WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}  Launching another instance lest be closed by unidling.");
      _ = Process.Start(me.MainModule?.FileName ?? "Notepad.exe", "Un-idleable instance");
      Shutdown();
      return false;
    }

    return true;
  }
  protected override void OnSessionEnding(SessionEndingCancelEventArgs e) { LogScrSvrUptimeOncePerSession("ScrSvr - Dn - App.OnSessionEnding().   "); WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff} ▄▀▄▀App.OnSessionEnding()"); base.OnSessionEnding(e); }
  protected override async void OnExit(ExitEventArgs e)
  {
    base.OnExit(e);
    WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}  App.OnExit() 1/3 => before  LogScrSvrUptimeOncePerSession(\"ScrSvr - Dn - App.OnExit() \");");
    LogScrSvrUptimeOncePerSession("ScrSvr - Dn - App.OnExit() ");
    await SpeakAsync("Closed");
    await Task.Delay(512); // :Speak underestimates the time needed to speak the text.
  }
  //protected override void OnDeactivated(EventArgs e) { /* do not LogScrSvrUptimeOncePerSession() <- */ WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff} ▄▀▄▄▀▀▄▀App.OnDeactivated()  "); base.OnDeactivated(e); }
  void MustExit()
  {
    WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}  MustExit() 1/3 => before  LogScrSvrUptimeOncePerSession(\"ScrSvr - Dn - MustExit() \");");

    LogScrSvrUptimeOncePerSession("ScrSvr - Dn - MustExit() ");

    WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}  MustExit() 2/3 => before  Process.GetCurrentProcess().Kill(); ");
    Process.GetCurrentProcess().Kill();

    WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}  MustExit() 3/3 => never got here! ────────────────────────────");
    Environment.Exit(87);
    Environment.FailFast("Environment.FailFast");
  }
  public static void SpeakFaF(string msg, string voice = "", bool ignoreBann = false) => Task.Run(async () => await SpeakAsync(msg, voice: voice, ignoreBann));
  public static void SpeakFree(string msg) => SpeechSynth.SpeakFree(msg);
  public static async Task SpeakAsync(string msg, string voice = "", bool ignoreBann = false)
  {
    //WriteLine($"\t\t\t{msg}");

    if (AppSettings.Instance.IsSpeechOn || ignoreBann)
      await _synth.SpeakAsync(msg, voice: voice);
  }
  public static int Ssto_GpSec => IdleTimeoutSec + GraceEvLogAndLockPeriodSec;
  public static void LogScrSvrUptimeOncePerSession(string msg)
  {
    // Write($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}  ▓▓  {msg,-60}");

    lock (_thisLock)
    {
      if (_mustLogEORun == null)
        Write(""); // WriteLine($"not logged <- flag is not set .. must be too soon to log. ▒▒");
      else if (_mustLogEORun == false)
        Write(""); // WriteLine($"not logged <- flag is set to false .. means: already logged before. ▒▒");
      else // (_mustLogEORun == true)
      {
        _mustLogEORun = false;

        if (!DevOps.IsDbg)
        {
          EvLogHelper.LogScrSvrEnd(StartedAt.AddSeconds(-IdleTimeoutSec), msg);
        }

        Write($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}  ▓▓ {msg,-60}");
        WriteLine($"ev. log SUCCESS ... for Release only, though!!!  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓ ");
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
  void SleepStandby(bool isDeepHyberSleep = false)
  {
    WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}\t Starting {(isDeepHyberSleep ? "Hibernating" : "LightSleeping")}:  SetSuspendState(); ...");
    _ = SetSuspendState(hiberate: isDeepHyberSleep, forceCritical: false, disableWakeEvent: false);
    MustExit();
  }
  #region Ctrls
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
  Window? _cntrL;
  private static ConfigRandomizer _cfg;

  public Window CntrL => _cntrL ??= new ContainerL(_globalEventHandler);
  public static bool CloseOnUnIdle { get; set; } = true;
  [Flags] enum WindowStyle { CLIPCHILDREN = 33554432, VISIBLE = 268435456, CHILD = 1073741824 }
  [DllImport("Powrprof.dll", CharSet = CharSet.Auto, ExactSpelling = true)] static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);
  [DllImport("user32")] public static extern void LockWorkStation();
  #endregion
}
/// Install-Package Expression.Blend.Sdk
/// Use for deployment:  Release + Any CPU  

//Rogers chat on Feb 10, 2023:
//Okay Alex all set I have submitted that for you! You should get a return label emailed to you within the next day or so.Alternatively even without the label you can simply pack your items in a box and bring it to a Canada Post location with the Return ID Number PR823007. Canada Post will provide you with a receipt and take care of things from there!
//03:58 PMDorian
//Sorry for the mixup Alex and Thanks for your patience while we got everything sorted I really appreciate it! Let me know if you had any other questions at all or anything else you wanted to look into today.
