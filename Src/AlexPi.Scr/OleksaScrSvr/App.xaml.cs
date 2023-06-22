using OleksaScrSvr.Services;
using OleksaScrSvr.VM.VMs;

namespace OleksaScrSvr;
public partial class App : System.Windows.Application
{
  readonly DateTimeOffset _appStarted = DateTimeOffset.Now;
  readonly IServiceProvider _serviceProvider;
  string _audit = "audit is unassigned";

  public App()
  {
    IServiceCollection services = new ServiceCollection();

    AppStartHelper.InitAppSvcs(services);

    MvvmInitHelper.InitMVVM(services);

    _ = services.AddSingleton<IAddChild, MainNavView>();
    _ = services.AddSingleton<MainNavView>(s => new MainNavView(s.GetRequiredService<ILogger>(), s.GetRequiredService<IConfigurationRoot>(), s.GetRequiredService<IBpr>()) { DataContext = s.GetRequiredService<MainVM>() });

    _serviceProvider = services.BuildServiceProvider();

    ShutdownMode = ShutdownMode.OnMainWindowClose;
  }

  protected override async void OnStartup(StartupEventArgs e)
  {
    UnhandledExceptionHndlr.Logger = _serviceProvider.GetRequiredService<ILogger>();
    Current.DispatcherUnhandledException += UnhandledExceptionHndlr.OnCurrentDispatcherUnhandledException;
    EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotFocusEvent, new RoutedEventHandler((s, re) => { (s as TextBox ?? new TextBox()).SelectAll(); }));
    ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(15000));

    //if (Debugger.IsAttached) while (true) { await AppStartHelper.Testc(); Debugger.Break(); }

    _serviceProvider.GetRequiredService<INavSvc>().Navigate();

    MainWindow = _serviceProvider.GetRequiredService<MainNavView>();
    MainWindow.Show();

    SafeAudit();

    base.OnStartup(e);

    _serviceProvider.GetRequiredService<ILogger>().LogInformation($"StU{(DateTime.Now - _appStarted).TotalSeconds,4:N1}s  {_audit}");

    var mainVM = (MainVM)MainWindow.DataContext;  // mainVM.DeploymntSrcExe = Settings.Default.DeplSrcExe; //todo: for future only.    
    _ = await mainVM.InitAsync();                 // blocking due to vesrion checker.
  }
  protected override async void OnExit(ExitEventArgs e)
  {
    if (Current is not null) Current.DispatcherUnhandledException -= UnhandledExceptionHndlr.OnCurrentDispatcherUnhandledException;
    //_serviceProvider.GetRequiredService<OleksaScrSvrModel>().Dispose();

    if (DateTime.Now == DateTime.Today) LogAllLevels(_serviceProvider.GetRequiredService<ILogger>());

    _serviceProvider.GetRequiredService<ILogger>().LogInformation($"└──{(DateTimeOffset.Now - _appStarted).TotalHours,4:N1}h  {_audit} \n██");

    await _serviceProvider.GetRequiredService<IBpr>().ClickAsync();
    await _serviceProvider.GetRequiredService<IBpr>().AppFinishAsync();
    await Task.Delay(1500);

    base.OnExit(e);
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
      var cfg = _serviceProvider.GetRequiredService<IConfigurationRoot>();
      if (string.IsNullOrEmpty(cfg[CfgName.SqlVerIpm]))
      {
        TryFixingCfgAndRestart($"Unable to continue\n\ncfg[DeplConstIpm.SqlVerIpm] is null!!!    '{cfg[CfgName.SqlVerIpm]}' ");
        return;
      }

      _audit = VersionHelper.DevDbgAudit(cfg, "[no sql needee]");
    }
    catch (Exception ex)
    {
      _serviceProvider.GetRequiredService<ILogger>().LogError(ex, _audit);
      TryFixingCfgAndRestart($"▓▓  ▓▓  ▓▓  Restarting due to the exception in SafeAudit:  {ex.Message}  ▓▓  ▓▓  ▓▓  ");
    }
  }
  void TryFixingCfgAndRestart(string reason)
  {
    if (MessageBox.Show($"{reason}\n\nTry to fix config ecosystem?", "App Config Problem", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
    {
      _ = ConfigHelper.AutoInitConfigHardcoded();
      _serviceProvider.GetRequiredService<ILogger>().LogWarning(reason);
      _ = Process.Start(new ProcessStartInfo(Assembly.GetEntryAssembly()?.Location.Replace(".dll", ".exe") ?? "Notepad.exe"));
    }

    _ = Current.Dispatcher.InvokeAsync(async () => //nogo: Task.Run(async () =>
    {
      await Task.Delay(2600);
      Current.Shutdown();
    });
  }
}