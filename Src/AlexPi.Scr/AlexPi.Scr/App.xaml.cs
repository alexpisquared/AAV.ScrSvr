using AAV.Sys.Helpers;
using AAV.WPF.Ext;
using AlexPi.Scr.AltBpr;
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

    static readonly object _thisLock = new object();
    static bool _mustLogEORun = false;
    public static readonly DateTime Started = DateTime.Now;
    public const int
#if DEBUG
      GraceEvLogAndLockPeriodSec = 6,
      _ScrSvrShowDelayMs = 500;
#else
      GraceEvLogAndLockPeriodSec = 60,
      _ScrSvrShowDelayMs = 10000;
#endif

    static readonly SpeechSynth _synth;
    public static void SpeakFaF(string msg) => Task.Run(async () => await _synth.SpeakAsync(msg)); // FaF - Fire and Forget
    public static async Task SpeakAsync(string msg) => await _synth.SpeakAsync(msg);

    static App()
    {
      Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - Started):mm\\:ss\\.ff}    Ctor()     0/n     {Environment.CurrentDirectory}");
      _synth = new SpeechSynth();
    }
    protected override async void OnStartup(StartupEventArgs sea)
    {
      Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - Started):mm\\:ss\\.ff}    StartUp()  1/2     {Environment.CurrentDirectory}");
      try
      {
        base.OnStartup(sea);
        //Bpr.BeepBgn3();

#if DEBUG_
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
        Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - Started):mm\\:ss\\.ff}    StartUp()  2/2     args: {string.Join(", ", sea.Args)}.");

        Trace.WriteLineIf(CurTraceLevel.TraceWarning, $"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - Started):mm\\:ss\\.ff}    StartUp()  {Environment.MachineName,-8}{VerHelper.CurVerStr(".NET 4.8"),-12}   {string.Join(" ", Environment.GetCommandLineArgs())}  ");
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
              SpeakFaF(rprt);
              Trace.WriteLineIf(CurTraceLevel.TraceWarning, $"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - Started):mm\\:ss\\.ff}    StartUp() - {rprt}");
              //}).ContinueWith(_ => 
              Shutdown()
              //, TaskScheduler.FromCurrentSynchronizationContext()); //?? Aug 2019.
              ;
              return;
          }
        }

        showFullScrSvr_ScheduleArming();
      }
      catch (Exception ex) { ex.Pop("ASYNC void OnStartup()"); }

      Trace.WriteLineIf(CurTraceLevel.TraceWarning, $"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - Started):mm\\:ss\\.ff}    StartUp() - EOMethof.");
    }


    protected override void OnExit(ExitEventArgs e) {                           /* KeepAwakeHelper.Restore();*/ LogScrSvrUptime("ScrSvr - Dn - App.OnExit()          "); Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - Started):mm\\:ss\\.ff} App.OnExit()         "); base.OnExit(e); }
    protected override void OnDeactivated(EventArgs e) {                        /* KeepAwakeHelper.Restore();*/ LogScrSvrUptime("ScrSvr - Dn - App.OnDeactivated().  "); Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - Started):mm\\:ss\\.ff} App.OnDeactivated()  "); base.OnDeactivated(e); }
    protected override void OnSessionEnding(SessionEndingCancelEventArgs e) {   /* KeepAwakeHelper.Restore();*/ LogScrSvrUptime("ScrSvr - Dn - App.OnSessionEnding()."); Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - Started):mm\\:ss\\.ff} App.OnSessionEnding()"); base.OnSessionEnding(e); }

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

    public static void LogScrSvrUptime(string msg)
    {
      Trace.WriteIf(CurTraceLevel.TraceWarning, $"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - Started):mm\\:ss\\.ff}    EvLogHlpr.Log({msg})");

      lock (_thisLock)
      {
        if (_mustLogEORun)
        {
#if DEBUG
          SpeakFaF($"no end-up logging.");
#else
          _mustLogEORun = false;
          EvLogHelper.LogScrSvrEnd(App.Started.AddSeconds(-ScrSvrTimeoutSec), ScrSvrTimeoutSec, msg);
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
        //WindowStyle = (int)(WS.VISIBLE | WS.CHILD | WS.CLIPCHILDREN)
      };

      if (whndl != IntPtr.Zero)
      {
        hwndSourceParameters.ParentWindow = whndl;
        hwndSourceParameters.WindowStyle = (int)(WS.VISIBLE | WS.CHILD | WS.CLIPCHILDREN);
      }
      else
        hwndSourceParameters.WindowStyle = (int)(WS.VISIBLE);

      miniSS.Height = hwndSourceParameters.Height;
      miniSS.Width = hwndSourceParameters.Width;
      miniSS.Visibility = whndl == IntPtr.Zero ? Visibility.Visible : Visibility.Hidden;
      miniSS.Title = $"{VerHelper.CurVerStr(".NET 4.8")} \n\n  {miniSS.Width} x {miniSS.Height} \n\n     Slp: {AppSettings.Instance.AutoSleep}   Htr: {AppSettings.Instance.IsHeaterOn}";

      var _HwndSource = new HwndSource(hwndSourceParameters) { RootVisual = miniSS.LayoutRoot };
      _HwndSource.Disposed += (s, e) => miniSS.Close();
    }
    void showFullScrSvr_ScheduleArming()
    {
      Task.Run(async () => await ChimerAlt.FreqRunUpHiPh());

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
        Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 50) * 1000)).ContinueWith(_ => SpeakAsync($"a 50"));
        Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 40) * 1000)).ContinueWith(_ => SpeakAsync($"a 40"));
        Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 30) * 1000)).ContinueWith(_ => SpeakAsync($"a 30"));
        Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 20) * 1000)).ContinueWith(_ => SpeakAsync($"a 20"));
        Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 10) * 1000)).ContinueWith(_ => SpeakAsync($"a 10"));      //puzzle: runs 50 sec delay for all and read all at that moment: for (var i = 50; i > 0; i -= 5)        Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - i) * 1000)).ContinueWith(_ => SpeakAsync($"{i}+{i}=x"));
#endif
      }

      Task.Run(async () => await Task.Delay((GraceEvLogAndLockPeriodSec - 00) * 1000)).ContinueWith(armAndLegEvent());
    }

    static Action<Task> armAndLegEvent() => _ =>
    {
      _mustLogEORun = true;

#if DEBUG
      SpeakFaF($"no start-up event logging");
#else
      EvLogHelper.LogScrSvrBgn(App.Ssto_GpSec);
#endif

      Task.Run(async () => await App.SleepLogic());
      Task.Run(async () => await App.LockeLogic());
    };

    public static void SleepStandby(bool isDeepHyberSleep = false)
    {
      Trace.WriteLineIf(CurTraceLevel.TraceWarning, $"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - Started):mm\\:ss\\.ff}>\t {(isDeepHyberSleep ? "Hibernating" : "LightSleeping")} started.");
      SetSuspendState(isDeepHyberSleep, true, true);
    }

    [DllImport("Powrprof.dll", CharSet = CharSet.Auto, ExactSpelling = true)] public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);
    [DllImport("user32")] public static extern void LockWorkStation();

    public static async Task SleepLogic()
    {
      if (VerHelper.IsVIP && !AppSettings.Instance.AutoSleep)
      {
        App.SpeakFaF("Armed! Sleepless mode.");
      }
      else
      {
#if DEBUG
        App.SpeakFaF($"Armed!");
#endif
        await Task.Delay(TimeSpan.FromMinutes(AppSettings.Instance.Min2Sleep + .25));
        App.SpeakFaF($"                    {AppSettings.Instance.Min2Sleep} minutes has passed. Sending computer to a light non-hibernating sleep ...in a minute."); //try to speak async so that dismissal by user was possible (i.e., not locked the UI):
        await ChimerAlt.FreqWalkUp();
        await Task.Delay(TimeSpan.FromMinutes(1));
        App.SpeakFaF($"Enforcing sleep now.");
        await ChimerAlt.FreqWalkDn();

        await EvLogHelper.UpdateEvLogToDb(10, $"The Enforcing-Sleep moment.");

        App.LogScrSvrUptime("ScrSvr - Dn - Sleep enforced by AAV.scr!");
        App.SleepStandby(); //Environment.MachineName.ToLower() == "nuc2");

        //never showing up: MessageBox.Show("ScrSvr - Dn - Sleep enforced by AAV.scr!");
        //App.Shutdown();
      }
    }
    public static async Task LockeLogic()
    {
      if (VerHelper.IsVIP && !AppSettings.Instance.AutoLocke)
      {
        //too much talking: App.SpeakSynch("Lockeless mode.");
      }
      else
      {
        App.SpeakFaF($"Locking in          {AppSettings.Instance.Min2Locke} minutes.");
        await Task.Delay(TimeSpan.FromMinutes(AppSettings.Instance.Min2Locke));
        await App.SpeakAsync($"                    {AppSettings.Instance.Min2Locke} minutes has passed. Computer to be Locked in a minute ..."); //try to speak async so that dismissal by user was possible (i.e., not locked the UI):
        await Task.Delay(TimeSpan.FromSeconds(60));
        App.SpeakFaF($"Enforcing lock down now.");

        LockWorkStation();
      }
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
  }

  [Flags]
  public enum WS //     Window styles
  {
    CLIPCHILDREN = 33554432,
    VISIBLE = 268435456,
    CHILD = 1073741824,
  }
}

/// Install-Package Expression.Blend.Sdk
/// Use for deployment:  Release + Any CPU  
