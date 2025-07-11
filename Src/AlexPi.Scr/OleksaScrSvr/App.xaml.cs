﻿using MSGraphSlideshow;
using ScreenTimeUsrCtrlLib.AsLink;
using static AmbienceLib.SpeechSynth;

namespace OleksaScrSvr;
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public partial class App : System.Windows.Application
{
  bool _mustLogEORun = !DevOps.IsDbg;
  string _audit = "audit is unassigned";
  readonly DateTimeOffset _appStarted = DateTimeOffset.Now;
  int f = 0;

  public IServiceProvider ServiceProvider { get; }

  public App()
  {
    IServiceCollection services = new ServiceCollection();

    AppStartHelper.InitAppSvcs(services);
    MvvmInitHelper.InitMVVM(services);

    _ = services.AddSingleton<IAddChild, MainNavView>();
    _ = services.AddSingleton<MainNavView>(s => new MainNavView(s.GetRequiredService<ILogger>(), s.GetRequiredService<IConfigurationRoot>(), s.GetRequiredService<IBpr>(), s.GetRequiredService<SpeechSynth>()) { DataContext = s.GetRequiredService<MainVM>() });

    ServiceProvider = services.BuildServiceProvider();

    ShutdownMode = ShutdownMode.OnMainWindowClose;
  }

  protected override async void OnStartup(StartupEventArgs e)
  {
    UnhandledExceptionHndlrUI.Logger = ServiceProvider.GetRequiredService<ILogger>();
    Current.DispatcherUnhandledException += UnhandledExceptionHndlrUI.OnCurrentDispatcherUnhandledException;
    EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotFocusEvent, new RoutedEventHandler((s, re) => { (s as TextBox ?? new TextBox()).SelectAll(); }));
    ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(15000));
    ToolTipService.InitialShowDelayProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(15));

    ServiceProvider.GetRequiredService<INavSvc>().Navigate();

    MainWindow = ServiceProvider.GetRequiredService<MainNavView>();
    MainWindow.Show();

    _audit = VersionHelper.DevDbgAudit(ServiceProvider.GetRequiredService<IConfigurationRoot>(), "°");

    base.OnStartup(e);

    ServiceProvider.GetRequiredService<ILogger>().LogInformation($"┌──{TimeSoFar} {_audit}");

    var mainVM = (MainVM)MainWindow.DataContext;  // mainVM.DeploymntSrcExe = Settings.Default.DeplSrcExe; //todo: for future only.    
    _ = await mainVM.InitAsync();                 // blocking due to version checker.
    FromOutlookCrashChecker();
    _ = await TimedSleepAndExit(StandardLib.Consts.ScrSvrPresets.MinToPcSleep);
  }

  async void FromOutlookCrashChecker()
  {
    if (!Environment.MachineName.Contains("33")) { return; }

    var bpr = ServiceProvider.GetRequiredService<IBpr>();
    double _periodInMin = 14;

    for (var i = 0; i < 2_500; i++)
    {
      var dt = DateTime.Now - _appStarted;
      if (dt.TotalMinutes > (_periodInMin * 1) && dt.TotalMinutes < ((_periodInMin * 1) + 1)) // check/restart Outlook every ~15 minutes <== should be sufficient for never missing a meeting.
      {
        _ = WinAPI.Beep(200 + (800 * (f % 4)), 240 / (1 + (f++ % 4)));
      }

      await Task.Delay(DevOps.IsDbg ? 5_950 : 14_960);
      if (i == 1) await bpr.AreYouSure1kAsync();
      if (i == 2) await bpr.ChockingUhmAsync();
    }
  }

  protected override void OnExit(ExitEventArgs e)
  {
    LogScrSvrUptimeOncePerSession("ScrSvr down - OnExit.");

    //tmi: ServiceProvider.GetRequiredService<ILogger>().LogInformation($"╘══{TimeSoFar} OnExit  :on manual action\n██");

    if (Current is not null)
    {
      Current.DispatcherUnhandledException -= UnhandledExceptionHndlrUI.OnCurrentDispatcherUnhandledException;
    }
    //_serviceProvider.GetRequiredService<OleksaScrSvrModel>().Dispose();

    if (DateTime.Now == DateTime.Today)
    {
      LogAllLevels(ServiceProvider.GetRequiredService<ILogger>());
    }

    ServiceProvider.GetRequiredService<IBpr>().AppFinish();

    base.OnExit(e);
  }

  async Task<bool> TimedSleepAndExit(double minToPcSleep)
  {
    var speech = ServiceProvider.GetRequiredService<SpeechSynth>();
    var logger = ServiceProvider.GetRequiredService<ILogger>();
    var beeper = ServiceProvider.GetRequiredService<IBpr>();

    Current.Resources["ExecutionDuration"] = new Duration(TimeSpan.FromMinutes(minToPcSleep));

    try
    {
      if (DevOps.IsDbg)
      {
        new EarliestDateTests().TestEarliestDate();
        await Task.Delay(TimeSpan.FromMinutes(minToPcSleep - 0)); speech.SpeakFAF($"Turning off in 15 seconds.");

        LastMinuteChanceToCancelShutdown(speech, .25, logger, beeper);
      }
      else
      {
        await Task.Delay(TimeSpan.FromSeconds(40)); // speech.SpeakFAF($"Really?", volumePercent: 60);
        await Task.Delay(TimeSpan.FromSeconds(10)); // speech.SpeakFAF($"Locking...", volumePercent: 20);
        await Task.Delay(TimeSpan.FromSeconds(10)); // speech.SpeakFAF($"Locked!", volumePercent: 20);

        _mustLogEORun = true;
        new EvLogHelper().LogScrSvrBgn(300); // 300 sec of idle has passed

        await Task.Delay(TimeSpan.FromMinutes(02)); // SpeakRandomFunMessage();

        await Task.Delay(TimeSpan.FromMinutes(minToPcSleep - 3)); speech.SpeakFAF($"Turning off in a minute.", volumePercent: 20);

        LastMinuteChanceToCancelShutdown(speech, 1, logger, beeper);
      }
    }
    catch (Exception ex) { logger.LogError(ex, _audit); }

    return true;
  }
  void SpeakRandomFunMessage()
  {
    var speech = ServiceProvider.GetRequiredService<SpeechSynth>();
    var funMsg = new FunMessages().RandomMessage;

    switch (new Random(DateTime.Now.Microsecond).Next(4))
    {
      case 3: speech.SpeakFAF(funMsg, voice: CC.Aria, style: CC.whispering); break;
      case 1: speech.SpeakFAF(funMsg, voice: CC.Xiaomo, style: CC.sad, role: CC.Girl); break;
      case 0: speech.SpeakFAF(funMsg, voice: CC.Xiaomo, style: CC.fearful, role: CC.Girl); break;
      case 2: speech.SpeakFAF(funMsg, voice: CC.Xiaomo, style: CC.affectionate, role: CC.Girl); break;
      default: break;
    }

    ServiceProvider.GetRequiredService<ILogger>().Log(LogLevel.Information, $"╞══  Played this '{funMsg}'│");
  }
  void LastMinuteChanceToCancelShutdown(SpeechSynth speech, double oneMinute, ILogger logger, IBpr bpr) => _ = Task.Run(async () =>
                                                                                                            {
                                                                                                              var taskDelay = Task.Delay(TimeSpan.FromMinutes(oneMinute)); // must go first, or else it will be scheduled AFTER! completion of the scream.
                                                                                                              var taskScream =
                                                                                                              Environment.MachineName switch
                                                                                                              {
                                                                                                                "origin" // or "NUC2" // or "GRAM1" or "ASUS2" or "YOGA1" or "BEELINK1"
                                                                                                                  => bpr.GradientAsync(52, 9_000, 19, (int)(120_000 * oneMinute)),
                                                                                                                "NUC2"
                                                                                                                  => bpr.GradientAsync(52, 0_500, 19, (int)(120_000 * oneMinute)),
                                                                                                                "RAZER1"
                                                                                                                  => bpr.GradientAsync(52, 0_800, 19, (int)(120_000 * oneMinute)),
                                                                                                                "MINISFORUM1"
                                                                                                                  => bpr.GradientAsync(52, 1_100, 19, (int)(120_000 * oneMinute)),
                                                                                                                "CETLAP33"
                                                                                                                  => bpr.GradientAsync(52, 1_100, 19, (int)(120_000 * oneMinute)),
                                                                                                                _ => bpr.GradientAsync(52, 0_800, 19, (int)(120_000 * oneMinute))
                                                                                                              };

                                                                                                              await Task.WhenAll(taskDelay, taskScream);
                                                                                                            }).ContinueWith(async t =>
                                                                                                            {
                                                                                                              if (DevOps.IsDbg)
                                                                                                              {
                                                                                                                speech.SpeakFAF("PC is sent to sleep in release mode only.");
                                                                                                              }
                                                                                                              else
                                                                                                              {
                                                                                                                if (Environment.MachineName.Contains("33")) // no sleep needed on 33 .. right? (May 2025)
                                                                                                                {
                                                                                                                  logger.Log(LogLevel.Information, $"╞══{TimeSoFar} what to do here on PC33? ·· nothing; the PC keeps idling until scrsvr is closed.");
                                                                                                                }
                                                                                                                else
                                                                                                                {
                                                                                                                  await speech.SpeakAsync($"Sweet dreams.");
                                                                                                                  LogScrSvrUptimeOncePerSession("ScrSvr - Dn - PC sleep enforced by the screen saver."); ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Log ScrSvr Uptime (___Idle) Once Per Session
                                                                                                                  await Task.Delay(1_000); // give it a second to finish the speech.

                                                                                                                  var sleepStart = DateTimeOffset.Now;
                                                                                                                  logger.Log(LogLevel.Information, $"╞══{TimeSoFar} SetSuspendState(); ■ never?! goes beyond this on NUC2, GRAM1; only on RAZER1, MF1. On MF1 this is EOIdle  \n█·                     │");
                                                                                                                  _ = SetSuspendState(hibernate: false, forceCritical: true, disableWakeEvent: true); //2025: was all false: did not work on MinisForum. Trying also prevent periodic wake-ups.

                                                                                                                  logger.Log(LogLevel.Information, $"╞══{TimeSoFar} Process()..Close();  !!! Wake time !!!  Slept for {VersionHelper.TimeAgo(DateTimeOffset.Now - sleepStart),8} ··");
                                                                                                                  Process.GetCurrentProcess().Close();        //logger.Log(LogLevel.Information, $"+{TimeSoFar}  Process().Kill();    \n███·"); Process.GetCurrentProcess().Kill();
                                                                                                                }

                                                                                                                // never gets here: 
                                                                                                                //Environment.Exit(87);
                                                                                                                //Environment.FailFast("Environment.FailFast");
                                                                                                              }
                                                                                                            }, TaskScheduler.FromCurrentSynchronizationContext());

  void LogScrSvrUptimeOncePerSession(string msg)
  {
    if (!Environment.CommandLine.Contains("Sched", StringComparison.OrdinalIgnoreCase))
    {
      ServiceProvider.GetRequiredService<ILogger>().LogInformation($"╘══{TimeSoFar} OnExit   '{msg}'   *NOT* logged into the EventLog ▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄ :(Environment.CommandLine. ! Contains(\"Sched\"))\n██");
    }
    else if (Environment.CommandLine.Contains("idle", StringComparison.OrdinalIgnoreCase) && (DateTimeOffset.Now - _appStarted).TotalMinutes < 1)
    {
      ServiceProvider.GetRequiredService<ILogger>().LogInformation($"╘══{TimeSoFar} OnExit   '{msg}'   *NOT* logged into the EventLog ▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄ :too early - discarded by the user\n██");
    }
    else if (!_mustLogEORun)
    {
      ServiceProvider.GetRequiredService<ILogger>().LogInformation($"╘══{TimeSoFar} OnExit   '{msg}'   *NOT* logged into the EventLog ▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀\n██");
    }
    else
    {
      _mustLogEORun = false; // prevent multiple logging per session (Mar 31, 2025)
      new EvLogHelper().LogScrSvrEnd(_appStarted.DateTime.AddSeconds(-240), msg);
      ServiceProvider.GetRequiredService<ILogger>().LogInformation($"╘══{TimeSoFar} OnExit   '{msg}'   logged into the EventLog ■ ■ ■ .. on MF1 - it keeps running ..");
      ServiceProvider.GetRequiredService<SpeechSynth>().SpeakFAF($"End-of-idling event logged.");
    }
  }

  string TimeSoFar => $"{VersionHelper.TimeAgo(DateTimeOffset.Now - _appStarted),8}";

  void LogAllLevels(ILogger lgr)
  {
    lgr.Log(LogLevel.Warning,     /**/ "│   °°° The current LogLevel is:");
    lgr.Log(LogLevel.Trace,       /**/ "│   °°° LogLevel.Trace");
    lgr.Log(LogLevel.Debug,       /**/ "│   °°° LogLevel.Debug");
    lgr.Log(LogLevel.Information, /**/ "│   °°° LogLevel.Information");
    lgr.Log(LogLevel.Warning,     /**/ "│   °°° LogLevel.Warning");
    lgr.Log(LogLevel.Error,       /**/ "│   °°° LogLevel.Error");
    lgr.Log(LogLevel.Critical,    /**/ "│   °°° LogLevel.Critical");
    lgr.Log(LogLevel.None,        /**/ "│   °°° LogLevel.None - is the highest level ... higher than Critical");
  }

  [DllImport("Powrprof.dll", CharSet = CharSet.Auto, ExactSpelling = true)] static extern bool SetSuspendState(bool hibernate, bool forceCritical, bool disableWakeEvent);
}