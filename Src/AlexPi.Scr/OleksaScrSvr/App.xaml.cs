﻿namespace OleksaScrSvr;
public partial class App // : System.Windows.Application
{
  string _audit = "audit is unassigned";
  readonly DateTimeOffset _appStarted = DateTimeOffset.Now;

  public IServiceProvider ServiceProvider { get; }

  public App()
  {
    IServiceCollection services = new ServiceCollection();

    AppStartHelper.InitAppSvcs(services);

    MvvmInitHelper.InitMVVM(services);

    _ = services.AddSingleton<IAddChild, MainNavView>();
    _ = services.AddSingleton<MainNavView>(s => new MainNavView(s.GetRequiredService<ILogger>(), s.GetRequiredService<IConfigurationRoot>(), s.GetRequiredService<IBpr>()) { DataContext = s.GetRequiredService<MainVM>() });

    ServiceProvider = services.BuildServiceProvider();

    ShutdownMode = ShutdownMode.OnMainWindowClose;
  }

  protected override async void OnStartup(StartupEventArgs e)
  {
    UnhandledExceptionHndlr.Logger = ServiceProvider.GetRequiredService<ILogger>();
    Current.DispatcherUnhandledException += UnhandledExceptionHndlr.OnCurrentDispatcherUnhandledException;
    EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotFocusEvent, new RoutedEventHandler((s, re) => { (s as TextBox ?? new TextBox()).SelectAll(); }));
    ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(15000));

    //if (Debugger.IsAttached) while (true) { await AppStartHelper.Testc(); Debugger.Break(); }

    SafeAudit();

    ServiceProvider.GetRequiredService<INavSvc>().Navigate();

    MainWindow = ServiceProvider.GetRequiredService<MainNavView>();
    MainWindow.Show();

    SafeAudit();

    base.OnStartup(e);

    ServiceProvider.GetRequiredService<ILogger>().LogInformation($"StU{(DateTime.Now - _appStarted).TotalSeconds,4:N1}s  {_audit}");

    var mainVM = (MainVM)MainWindow.DataContext;  // mainVM.DeploymntSrcExe = Settings.Default.DeplSrcExe; //todo: for future only.    
    _ = await mainVM.InitAsync();                 // blocking due to vesrion checker.

    _ = await TimedSleepAndExit();
  }
  protected override async void OnExit(ExitEventArgs e)
  {
    if (Current is not null) Current.DispatcherUnhandledException -= UnhandledExceptionHndlr.OnCurrentDispatcherUnhandledException;
    //_serviceProvider.GetRequiredService<OleksaScrSvrModel>().Dispose();

    if (DateTime.Now == DateTime.Today) LogAllLevels(ServiceProvider.GetRequiredService<ILogger>());

    ServiceProvider.GetRequiredService<ILogger>().LogInformation($"└──{(DateTimeOffset.Now - _appStarted).TotalHours,4:N1}h  {_audit} \n██");

    await ServiceProvider.GetRequiredService<IBpr>().ClickAsync();
    await ServiceProvider.GetRequiredService<IBpr>().AppFinishAsync();
    await Task.Delay(1500);

    base.OnExit(e);
  }

  async Task<bool> TimedSleepAndExit(bool AutoSleep = true, int Min2Sleep = 6/*16+4=20*/)
  {
    var speechSynth = ServiceProvider.GetRequiredService<SpeechSynth>();
    var logger = ServiceProvider.GetRequiredService<ILogger>();

    try
    {

      if (!AutoSleep)
      {
        await speechSynth.SpeakAsync("Armed! Sleepless mode.");
        return false;
      }

      if (DevOps.IsDbg)
        speechSynth.SpeakFAF($"Armed!");

      await Task.Delay(TimeSpan.FromMinutes(Min2Sleep));         /**/ logger.Log(LogLevel.Trace, $"+{DateTime.Now - _appStarted:mm\\:ss\\.ff}  1/5  after  await Task.Delay({TimeSpan.FromMinutes(Min2Sleep)}).");
      await speechSynth.SpeakAsync($"Turning off in a minute."); /**/ logger.Log(LogLevel.Trace, $"+{DateTime.Now - _appStarted:mm\\:ss\\.ff}  2/5  ");
      await Task.Delay(TimeSpan.FromMinutes(1.15));              /**/ logger.Log(LogLevel.Trace, $"+{DateTime.Now - _appStarted:mm\\:ss\\.ff}  3/5  ");
      await speechSynth.SpeakAsync($"Final 30 seconds.");        /**/ logger.Log(LogLevel.Trace, $"+{DateTime.Now - _appStarted:mm\\:ss\\.ff}  4/5  ");
      await Task.Delay(TimeSpan.FromMinutes(0.50));              /**/ logger.Log(LogLevel.Trace, $"+{DateTime.Now - _appStarted:mm\\:ss\\.ff}  5/5  ");


      logger.Log(LogLevel.Trace, $"+{DateTime.Now - _appStarted:mm\\:ss\\.ff}  Process.GetCurrentProcess().Close(); \n█···"); Process.GetCurrentProcess().Close();
      logger.Log(LogLevel.Trace, $"+{DateTime.Now - _appStarted:mm\\:ss\\.ff}  SetSuspendState(hibernate: false..); \n██··"); _ = SetSuspendState(hiberate: false, forceCritical: false, disableWakeEvent: false);
      logger.Log(LogLevel.Trace, $"+{DateTime.Now - _appStarted:mm\\:ss\\.ff}  Process.GetCurrentProcess().Kill();  \n███·"); Process.GetCurrentProcess().Kill();

      // never gets here: 
      Environment.Exit(87);
      Environment.FailFast("Environment.FailFast");
    }
    catch (Exception ex) { logger.LogError(ex, _audit); }

    return true;
  }

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
  void SafeAudit()
  {
    try
    {
      var cfg = ServiceProvider.GetRequiredService<IConfigurationRoot>();
      _audit = VersionHelper.DevDbgAudit(cfg, $"ClientId_{Environment.UserName}:{cfg[$"ClientId_{Environment.UserName}"]}");
    }
    catch (Exception ex) { ServiceProvider.GetRequiredService<ILogger>().LogError(ex, _audit); }
  }

  [DllImport("Powrprof.dll", CharSet = CharSet.Auto, ExactSpelling = true)] static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);
}