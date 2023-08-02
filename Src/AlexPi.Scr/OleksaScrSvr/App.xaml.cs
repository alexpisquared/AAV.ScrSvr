namespace OleksaScrSvr;
public partial class App : System.Windows.Application
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

    ServiceProvider.GetRequiredService<INavSvc>().Navigate();

    MainWindow = ServiceProvider.GetRequiredService<MainNavView>();
    MainWindow.Show();

    var cfg = ServiceProvider.GetRequiredService<IConfigurationRoot>();
    _audit = VersionHelper.DevDbgAudit(cfg, $"ClientId_{Environment.UserName}:{cfg[$"ClientId_{Environment.UserName}"]}");

    base.OnStartup(e);

    ServiceProvider.GetRequiredService<ILogger>().LogInformation($"StU{(DateTime.Now - _appStarted).TotalSeconds,5:N1}s  {_audit}");

    var mainVM = (MainVM)MainWindow.DataContext;  // mainVM.DeploymntSrcExe = Settings.Default.DeplSrcExe; //todo: for future only.    
    _ = await mainVM.InitAsync();                 // blocking due to vesrion checker.
    _ = await TimedSleepAndExit(true, MinToSleep);
  }
  protected override async void OnExit(ExitEventArgs e)
  {
    ServiceProvider.GetRequiredService<ILogger>().LogInformation($"╘══{(DateTimeOffset.Now - _appStarted).TotalHours,5:N1}h  {_audit} \n██");

    if (Current is not null) Current.DispatcherUnhandledException -= UnhandledExceptionHndlr.OnCurrentDispatcherUnhandledException;
    //_serviceProvider.GetRequiredService<OleksaScrSvrModel>().Dispose();

    if (DateTime.Now == DateTime.Today) LogAllLevels(ServiceProvider.GetRequiredService<ILogger>());

    await ServiceProvider.GetRequiredService<IBpr>().AppFinishAsync();

    base.OnExit(e);
  }

  async Task<bool> TimedSleepAndExit(bool autoSleep, int min2sleep)
  {
    var speech = ServiceProvider.GetRequiredService<SpeechSynth>();
    var logger = ServiceProvider.GetRequiredService<ILogger>();

    Current.Resources["ExecutionDuration"] = new Duration(TimeSpan.FromMinutes(min2sleep));

    try
    {
      if (!autoSleep)
      {
        await speech.SpeakAsync("Armed! Sleepless mode.");
        return false;
      }

      speech.SpeakFAF($"Armed!");

      await Task.Delay(TimeSpan.FromMinutes(min2sleep));    /**/ logger.Log(LogLevel.Information, $"+{DateTime.Now - _appStarted:mm\\:ss\\.ff}  1/5  ██  after  await Task.Delay({TimeSpan.FromMinutes(min2sleep)}).");
      await speech.SpeakAsync($"Turning off in a minute."); /**/ logger.Log(LogLevel.Information, $"+{DateTime.Now - _appStarted:mm\\:ss\\.ff}  2/5  ██  ");
      await Task.Delay(TimeSpan.FromMinutes(1.15));         /**/ logger.Log(LogLevel.Information, $"+{DateTime.Now - _appStarted:mm\\:ss\\.ff}  3/5  ██  ");
      await speech.SpeakAsync($"Final 30 seconds.");        /**/ logger.Log(LogLevel.Information, $"+{DateTime.Now - _appStarted:mm\\:ss\\.ff}  4/5  ██  ");
      await Task.Delay(TimeSpan.FromMinutes(0.50));         /**/ logger.Log(LogLevel.Information, $"+{DateTime.Now - _appStarted:mm\\:ss\\.ff}  5/5  ██  ");

      logger.Log(LogLevel.Information, $"+{DateTime.Now - _appStarted:mm\\:ss\\.ff}  Process.GetCurrentProcess().Close(); \n█···"); Process.GetCurrentProcess().Close();
      logger.Log(LogLevel.Information, $"+{DateTime.Now - _appStarted:mm\\:ss\\.ff}  SetSuspendState(hibernate: false..); \n██··"); _ = SetSuspendState(hiberate: false, forceCritical: false, disableWakeEvent: false);
      logger.Log(LogLevel.Information, $"+{DateTime.Now - _appStarted:mm\\:ss\\.ff}  Process.GetCurrentProcess().Kill();  \n███·"); Process.GetCurrentProcess().Kill();

      // never gets here: 
      Environment.Exit(87);
      Environment.FailFast("Environment.FailFast");
    }
    catch (Exception ex) { logger.LogError(ex, _audit); }

    return true;
  }

  int MinToSleep =>
    Environment.MachineName == "RAZER1" ? 26 :
    Environment.MachineName == "ASUS2" ? 36 :
    Environment.MachineName == "YOGA1" ? 46 :
    Environment.MachineName == "NUC2" ? 16 :
    1111;

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

  [DllImport("Powrprof.dll", CharSet = CharSet.Auto, ExactSpelling = true)] static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);
}