using AAV.Sys.Helpers;
using AAV.WPF.AltBpr;
using AAV.WPF.Ext;
using AlexPi.Scr.Logic;
using AlexPi.Scr.Vws;
using AsLink;
using SpeechSynthLib;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using WindowsFormsLib;

namespace AlexPi.Scr
{
  public partial class App : Application
  {
    readonly GlobalEventHandler _globalEventHandler = new GlobalEventHandler();
    public static TraceSwitch CurTraceLevel,
      AppTraceLevel_Config = new TraceSwitch("CfgTraceLevelSwitch", "Switch in config file:  <system.diagnostics><switches><!--0-off, 1-error, 2-warn, 3-info, 4-verbose. --><add name='CfgTraceLevelSwitch' value='3' /> "),
      AppTraceLevel_inCode = new TraceSwitch("Verbose________Trace", "This is the trace for all               messages.") { Level = TraceLevel.Info },
      AppTraceLevel_Warnng = new TraceSwitch("ErrorAndWarningTrace", "This is the trace for Error and Warning messages.") { Level = TraceLevel.Warning };

    static readonly ushort _volume = (ushort)(8 < DateTime.Now.Hour && DateTime.Now.Hour < 21 ? ushort.MaxValue : ushort.MaxValue / 16);
    static readonly SpeechSynth _synth = new SpeechSynth();
    static readonly object _thisLock = new object();
    static bool _mustLogEORun = false;
    public static readonly DateTime StartedAt = DateTime.Now;
    public const int
#if DEBUG
      GraceEvLogAndLockPeriodSec = 06, _ScrSvrShowDelayMs = 500;
#else
      GraceEvLogAndLockPeriodSec = 60,      _ScrSvrShowDelayMs = 10000;
#endif
    protected override async void OnStartup(StartupEventArgs sea)
    {
      try
      {
        base.OnStartup(sea);

#if DEBUG_
        //await ChimerAlt.FreqWalkUp();
        //Bpr.BeepBgn3();
        //await ChimerAlt.Wake(); // AAV.Sys.Helpers.Bpr.Wake();
        //await App.SpeakAsync($"123");
        await ChimerAlt.Chime(1);
        await ChimerAlt.Chime(3);
        Debugger.Break();
        await ChimerAlt.FreqWalk(70, 40, 50, 1.07);
        //await AlexPi.Scr.AltBpr.ChimerAlt.FreqWalk();
        //for (var i = 3; i < 14; i++) { await AlexPi.Scr.AltBpr.ChimerAlt.Chime(i); }
        CurTraceLevel = AppTraceLevel_inCode; // cfg seems to be not available for ScrSvr launches?
#else
        CurTraceLevel = AppTraceLevel_Config;
#endif
        Tracer.SetupTracingOptions("AlexPi.Scr", CurTraceLevel);
        //Tracer.ReportErrorLevel(AppTraceLevel_Config, "App.CFG");
        //Tracer.ReportErrorLevel(AppTraceLevel_inCode, "App.app");
        //Tracer.ReportErrorLevel(AppTraceLevel_Warnng, "App.wrn"); // Trace.WriteLine("<= while app cfg=Verb, inc=Info, wrn=Warn, from App=Verb, .CFG=Verb");
        Trace.WriteLine($"\n{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{DateTime.Now - StartedAt:mm\\:ss\\.ff}    args: {string.Join(", ", sea.Args)}   {Environment.UserName}   {Environment.MachineName}   {VerHelper.CurVerStr(".Net 5.0")}");

        Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = 3 }); //tu: anim CPU usage GLOBAL reduction!!! (Aug2019: 10 was almost OK and <10% CPU. 60 is the deafult)
        //todo: Current.DispatcherUnhandledException += WPF.Helpers.UnhandledExceptionHndlr.OnCurrentDispatcherUnhandledException;
        EventManager.RegisterClassHandler(typeof(TextBox), UIElement.GotFocusEvent, new RoutedEventHandler((s, re) => { (s as TextBox)?.SelectAll(); })); //tu: TextBox

        //if (Debugger.IsAttached) PerformanceCounterHelper.L1();
        //?what for LoadCompleted += (s, e) => (FindResource("Opacity_1MinLowCpu") as Storyboard).Duration = new Duration(TimeSpan.FromSeconds(10));
        //todo: AppSettings.InitStore(storageMode: StorageMode.OneDriveU);
        //? is this one keeping hanging? if (AppSettings.Instance.KeepAwake) KeepAwakeHelper.KeepAwakeForever();

        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        if (sea.Args.Length > 0)
        {
          switch (sea.Args[0].ToLower(CultureInfo.InvariantCulture).Trim().Substring(0, sea.Args[0].Length < 2 ? 1 : 2))
          {
            default: Trace.WriteLineIf(CurTraceLevel.TraceWarning, $"  Unknown Args (Knowns are: /s /p /c up -u /u lo)"); goto case "/s";
            case "lo": Trace.WriteLineIf(CurTraceLevel.TraceWarning, $"  LogMore is ON.              "); CurTraceLevel = new TraceSwitch("VerboseTrace", "This is the VERBOSE trace for all messages") { Level = System.Diagnostics.TraceLevel.Verbose }; goto case "/s";
            case "/s": break;                                       // Run the Screen Saver.
            case "/p": showMiniScrSvr(sea.Args[1]); return;         // <HWND> - Preview Screen Saver as child of window <HWND>.
            case "/c": new SettingsWindow().ShowDialog(); return;   // Show the Settings dialog box, modal to the foreground window.
            case "up":
            case "-u":
            case "/u": new UpTimeReview2(false).Show(); return;     // uptime review.
            case "si":                                              // SilentDbUpdate
                                                                    //Task.Run(async () =>              {
              var evNo = await EvLogHelper.UpdateEvLogToDb(15, $"");
              var rprt = $"{(evNo < -3 ? "No" : evNo.ToString())} new events found/stored to MDB file.";
              await SpeakAsync(rprt);
              Trace.WriteLineIf(CurTraceLevel.TraceWarning, $"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - StartedAt):mm\\:ss\\.ff}    StartUp() - {rprt}");
              //}).ContinueWith(_ => 
              Shutdown()
              //, TaskScheduler.FromCurrentSynchronizationContext()); //?? Aug 2019.
              ;
              return;
            case "ct": // Chime Test
              await SpeakAsync($"Testing FreqWalkUp start");
              Trace.Write($"Testing FreqWalkUp start ... ");
              var sw = Stopwatch.StartNew();
              //await ChimerAlt.PlayWhistle(_volume);
              await ChimerAlt.FreqWalkUp(_volume);
              sw.Stop();
              Trace.WriteLine($" ... Testing FreqWalkUp finished. Took {sw.Elapsed.TotalSeconds:N0}");
              await SpeakAsync($"Testing FreqWalkUp finished. Took {sw.Elapsed.TotalSeconds:N0} sec.");
              return;
          }
        }

        showFullScrSvr_ScheduleArming();
      }
      catch (Exception ex) { ex.Pop("ASYNC void OnStartup()"); }

      Trace.WriteLineIf(CurTraceLevel.TraceWarning, $"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - StartedAt):mm\\:ss\\.ff}    StartUp() - EOMethof.");
    }
    protected override void OnSessionEnding(SessionEndingCancelEventArgs e) { LogScrSvrUptime("ScrSvr - Dn - App.OnSessionEnding()."); Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - StartedAt):mm\\:ss\\.ff} App.OnSessionEnding()"); base.OnSessionEnding(e); }
    protected override void OnDeactivated(EventArgs e) { LogScrSvrUptime("ScrSvr - Dn - App.OnDeactivated() == lost focus!!! actually .  "); Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - StartedAt):mm\\:ss\\.ff} App.OnDeactivated()  "); base.OnDeactivated(e); }
    protected override void OnExit(ExitEventArgs e)
    {
      LogScrSvrUptime("ScrSvr - Dn - App.OnExit()          ");
      base.OnExit(e);

      Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - StartedAt):mm\\:ss\\.ff}   App.OnExit() => Process.GetCurrentProcess().Kill(); ");
      Process.GetCurrentProcess().Kill();
      Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - StartedAt):mm\\:ss\\.ff}   App.OnExit() => never got here!");
      Environment.Exit(87);
      Environment.FailFast("Environment.FailFast");
    }

    public static void StopSpeakingAsync() => _synth.StopSpeakingAsync();
    public static void SpeakFaF(string msg, string voice = null) => Task.Run(async () => await _synth.SpeakAsync(msg, VMode.Prosody, voice)); // FaF - Fire and Forget
    public static async Task SpeakAsync(string msg, string voice = null)         /**/ => await _synth.SpeakAsync(msg, VMode.Express, voice);

    static int _ssto = -1; public static int ScrSvrTimeoutSec
    {
      get
      {
        if (_ssto == -1)
        {
          _ssto = EvLogHelper.Ssto;
        }

        return _ssto;
      }
    }
    public static int Ssto_GpSec => ScrSvrTimeoutSec + GraceEvLogAndLockPeriodSec;  // ScreenSaveTimeOut + Grace Period
    static void LogScrSvrUptime(string msg)
    {
      Trace.WriteIf(CurTraceLevel.TraceWarning, $"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - StartedAt):mm\\:ss\\.ff}    EvLogHlpr.Log({msg})");

      lock (_thisLock)
      {
        if (_mustLogEORun)
        {
#if !DEBUG
          _mustLogEORun = false;
          EvLogHelper.LogScrSvrEnd(App.StartedAt.AddSeconds(-ScrSvrTimeoutSec), ScrSvrTimeoutSec, msg);
          Trace.WriteIf(CurTraceLevel.TraceWarning, $" ... SUCCESS.");
#endif
        }
        else
          Trace.WriteIf(CurTraceLevel.TraceWarning, $" ... never armed OR done before !!!!!!!!!!!!!!!!");
      }

      Trace.WriteIf(CurTraceLevel.TraceWarning, $"\n");

      AAV.Sys.Helpers.Bpr.BeepEnd3();
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
        hwndSourceParameters.WindowStyle = (int)(WindowStyle.VISIBLE);

      miniSS.Height = hwndSourceParameters.Height;
      miniSS.Width = hwndSourceParameters.Width;
      miniSS.Visibility = whndl == IntPtr.Zero ? Visibility.Visible : Visibility.Hidden;
      miniSS.Title = $"{VerHelper.CurVerStr(".Net 5.0")} \n\n  {miniSS.Width} x {miniSS.Height} \n\n     Slp: {AppSettings.Instance.AutoSleep}   Htr: {AppSettings.Instance.IsHeaterOn}";

      var _HwndSource = new HwndSource(hwndSourceParameters) { RootVisual = miniSS.LayoutRoot };
      _HwndSource.Disposed += (s, e) => miniSS.Close();
    }
    void showFullScrSvr_ScheduleArming()
    {
      //Task.Run(async () =>      {        await ChimerAlt.PlayWhistle(_volume);      });

      var sj = new SpeakerJob();
      SpeakFaF($"Hey {sj.GetRandomFromUserSection("FirstName")}! {sj.GetRandomFromUserSection("Greetings")} ", sj.GetRandomFromUserSection("VoiceF"));

      foreach (var screen in WinFormHelper.GetAllScreens()) new BackgroundWindow(_globalEventHandler).ShowOnTargetScreen(screen);

      new ControlPanel(_globalEventHandler).Show();
      if (AppSettings.Instance.IsSaySecOn)
      {
#if DEBUG
        Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 5) * 1000)).ContinueWith(_ => SpeakAsync($"5"));
        Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 4) * 1000)).ContinueWith(_ => SpeakAsync($"4"));
        Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 3) * 1000)).ContinueWith(_ => SpeakAsync($"3"));
        Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 2) * 1000)).ContinueWith(_ => SpeakAsync($"2"));
        Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 1) * 1000)).ContinueWith(_ => SpeakAsync($"1"));
#else
        Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 50) * 1000)).ContinueWith(_ => SpeakAsync($"50"));
        Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 40) * 1000)).ContinueWith(_ => SpeakAsync($"40"));
        Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 30) * 1000)).ContinueWith(_ => SpeakAsync($"30"));
        Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 20) * 1000)).ContinueWith(_ => SpeakAsync($"20"));
        Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 10) * 1000)).ContinueWith(_ => SpeakAsync($"10"));      //puzzle: runs 50 sec delay for all and read all at that moment: for (var i = 50; i > 0; i -= 5)        Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - i) * 1000)).ContinueWith(_ => SpeakAsync($"{i}+{i}=x"));
#endif
      }

      Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 00) * 1000)).ContinueWith(armAndLegEvent());
    }
    static Action<Task> armAndLegEvent() => _ =>
    {
      _mustLogEORun = true;

#if DEBUG
      SpeakAsync($"no start-up event logging").Wait();
#else
      EvLogHelper.LogScrSvrBgn(App.Ssto_GpSec);
#endif

      Task.Run(async () => await App.sleepLogic());
      Task.Run(async () => await App.lockeLogic());
    };
    static void sleepStandby(bool isDeepHyberSleep = false)
    {
      Trace.WriteLineIf(CurTraceLevel.TraceVerbose, $"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - StartedAt):mm\\:ss\\.ff}>\t {(isDeepHyberSleep ? "Hibernating" : "LightSleeping")} started.");
      SetSuspendState(isDeepHyberSleep, true, true);
    }
    static async Task lockeLogic()
    {
      if (VerHelper.IsVIP && !AppSettings.Instance.AutoLocke)
      {
        //too much talking: App.SpeakSynch("Lockeless mode.");
      }
      else
      {
        App.SpeakFaF($"Locking in          {AppSettings.Instance.Min2Locke} minutes.");
        await Task.Delay(TimeSpan.FromMinutes(AppSettings.Instance.Min2Locke));
        await ChimerAlt.WakeAudio(); // wake up monitor's audio.
        await SpeakAsync($" {AppSettings.Instance.Min2Locke} minutes has passed. Computer to be Locked in a minute ..."); //try to speak async so that dismissal by user was possible (i.e., not locked the UI):
        await Task.Delay(TimeSpan.FromSeconds(60));
        App.SpeakFaF($"Enforcing lock down now.");

        LockWorkStation();
      }
    }
    static async Task sleepLogic()
    {
      try
      {
        Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - StartedAt):mm\\:ss\\.ff}    SL()  0.");
        if (VerHelper.IsVIP && !AppSettings.Instance.AutoSleep)
        {
          await SpeakAsync("Armed! Sleepless mode.");
        }
        else
        {
#if DEBUG
          SpeakFaF($"Armed and extremely dangerous!");
#endif
          await Task.Delay(TimeSpan.FromMinutes(AppSettings.Instance.Min2Sleep + .25));                                                                                                     /**/ Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - StartedAt):mm\\:ss\\.ff}    SL()  after  await Task.Delay(TimeSpan.FromMinutes(AppSettings.Instance.Min2Sleep + .25));.");

          await ChimerAlt.WakeAudio(); // wake up monitor's audio.
          await SpeakAsync($"Hey {Environment.UserName}! {(((DateTime.Now - StartedAt) + TimeSpan.FromSeconds(ScrSvrTimeoutSec)).TotalMinutes):N0} minutes has passed. Sending computer to sleep ...in a minute.");    /**/ Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - StartedAt):mm\\:ss\\.ff}    SL()  after  await SpeakAsync($'Hey {Environment.UserName}! {(ScrSvrTimeoutSec * 60 + AppSettings.Instance.Min2Sleep)} minutes has passed. Sending computer to sleep ...in a minute.');.");
          await ChimerAlt.FreqWalkUp(_volume);                                                                                                                                              /**/ Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - StartedAt):mm\\:ss\\.ff}    SL()  after  await ChimerAlt.FreqWalkUp(_volume);.");

          await Task.Delay(TimeSpan.FromMinutes(1.15));                                                                                                                                 /**/ Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - StartedAt):mm\\:ss\\.ff}    SL()  after  await Task.Delay(TimeSpan.FromMinutes(1.2));.");

          await SpeakAsync($"Hey {Environment.UserName}! Enforcing sleep now.");                                                                                                         /**/ Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - StartedAt):mm\\:ss\\.ff}    SL()  after  'Enforcing sleep now.'.        <<<<<<<<<<");
          await ChimerAlt.FreqWalkDn(_volume);                                                                                                                                               /**/ Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - StartedAt):mm\\:ss\\.ff}    SL()  after  await ChimerAlt.FreqWalkDn().  >>>>>>>>>>");
          await SpeakAsync($"Hey {Environment.UserName}! Ha ha. Ha ha ha. Too late.");

          await EvLogHelper.UpdateEvLogToDb(10, $"The Enforcing-Sleep moment.");

          LogScrSvrUptime("ScrSvr - Dn - Sleep enforced by AAV.scr!");                                                                                                                   /**/ Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - StartedAt):mm\\:ss\\.ff}    SL()  after  LogScrSvrUptime.");
          sleepStandby();                                                                                                                                                                    /**/ Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - StartedAt):mm\\:ss\\.ff}    SL()  after  sleepStandby();  .");
        }
      }
      catch (Exception ex) { ex.Pop("ASYNC void OnStartup()"); }
    }

    Window _cntrA; public Window CntrA => _cntrA ??= new ContainerA(_globalEventHandler);
    Window _cntrB; public Window CntrB => _cntrB ??= new ContainerB(_globalEventHandler);
    Window _cntrC; public Window CntrC => _cntrC ??= new ContainerC(_globalEventHandler);
    Window _cntrD; public Window CntrD => _cntrD ??= new ContainerD(_globalEventHandler);
    Window _cntrE; public Window CntrE => _cntrE ??= new ContainerE(_globalEventHandler);
    Window _cntrF; public Window CntrF => _cntrF ??= new ContainerF(_globalEventHandler);
    Window _cntrG; public Window CntrG => _cntrG ??= new ContainerG(_globalEventHandler);
    Window _cntrH; public Window CntrH => _cntrH ??= new ContainerH(_globalEventHandler);
    Window _cntrI; public Window CntrI => _cntrI ??= new ContainerI(_globalEventHandler);
    Window _cntrJ; public Window CntrJ => _cntrJ ??= new ContainerJ(_globalEventHandler);
    Window _cntrK; public Window CntrK => _cntrK ??= new ContainerK(_globalEventHandler);
    Window _cntrL; public Window CntrL => _cntrL ??= new ContainerL(_globalEventHandler);

    [Flags]
    enum WindowStyle { CLIPCHILDREN = 33554432, VISIBLE = 268435456, CHILD = 1073741824 }
    [DllImport("Powrprof.dll", CharSet = CharSet.Auto, ExactSpelling = true)] public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);
    [DllImport("user32")] public static extern void LockWorkStation();
  }
}

/// Install-Package Expression.Blend.Sdk
/// Use for deployment:  Release + Any CPU  
