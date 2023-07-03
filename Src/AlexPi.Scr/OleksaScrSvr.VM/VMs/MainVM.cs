namespace OleksaScrSvr.VM.VMs;
public partial class MainVM : BaseMinVM
{
  readonly bool _ctored;
  readonly NavigationStore _navigationStore;
  readonly ModalNavgnStore _modalNavgnStore;
  readonly IsBusyStore _IsBusyStore;
  readonly Window _mainWin;
  public MainVM(NavigationStore navigationStore, ModalNavgnStore modalNavgnStore, IsBusyStore IsBusyStore, ILogger lgr, IBpr bpr, UserSettingsSPM usrStgns, IAddChild wnd)
  {
    _navigationStore = navigationStore;
    _modalNavgnStore = modalNavgnStore;
    _IsBusyStore = IsBusyStore;
    Logger = lgr;
    Bpr = bpr;
    UsrStgns = usrStgns;
    _mainWin = (Window)wnd;

    _navigationStore.CurrentVMChanged += OnCurrentVMChanged;
    _modalNavgnStore.CurrentVMChanged += OnCurrentModalVMChanged;
    _IsBusyStore.IsBusyChanged += OnIsBusy_Store_Changed;

    if (DevOps.IsSelectModes)
    {
      UsrStgns.IsAudible = false;
      UsrStgns.IsAnimeOn = false;
    }

    Bpr.SuppressTicks = Bpr.SuppressAlarm = !(IsAudible = UsrStgns.IsAudible);
    IsAnimeOn = UsrStgns.IsAnimeOn;

    _ctored = true;
  }
  public override async Task<bool> InitAsync()
  {
    AppVerNumber = VersionHelper.CurVerStrYYMMDD;
    AppVerToolTip = VersionHelper.CurVerStr("0.M.d.H.m");

    Bpr.AppStart();
    await Task.Delay(100);

    var rv = await base.InitAsync();

    if (DateTime.Now == DateTime.Today)
      try { await KeepCheckingForUpdatesAndNeverReturn(); } catch (Exception ex) { ex.Pop(Logger); }

    return rv;
  }
  public override Task<bool> TryWrapAsync()
  {
    _navigationStore.CurrentVMChanged -= OnCurrentVMChanged;
    _modalNavgnStore.CurrentVMChanged -= OnCurrentModalVMChanged;
    _IsBusyStore.IsBusyChanged -= OnIsBusyChanged;

    return base.TryWrapAsync();
  }

  async Task KeepCheckingForUpdatesAndNeverReturn()
  {
    await Task.Delay(15000);
    OnCheckForNewVersion();

    var nextHour = DateTime.Now.AddHours(1);
    var nextHH00 = new DateTime(nextHour.Year, nextHour.Month, nextHour.Day, nextHour.Hour, 0, 5);
    await Task.Delay(nextHH00 - DateTime.Now);
    OnCheckForNewVersion(true);

    while (await new PeriodicTimer(TimeSpan.FromMinutes(10)).WaitForNextTickAsync()) { OnCheckForNewVersion(); }
  }
  void OnCheckForNewVersion(bool logNetVer = false)
  {
    try
    {
      if (!File.Exists(DeploymntSrcExe))
      {
        Logger.LogWarning($"│   Version check    File does not exist:   {DeploymntSrcExe}   ***********************************************");
        AppVerToolTip = "Version check failed: depl. file is not found.";
        return;
      }

      (IsObsolete, var setupExeTime) = VersionHelper.CheckForNewVersion(DeploymntSrcExe);
      Logger.Log(IsObsolete ? LogLevel.Warning : LogLevel.Information, $"│   Version check this/depl {VersionHelper.TimedVer:MMdd·HHmm}{(IsObsolete ? "!=" : "==")}{setupExeTime:MMdd·HHmm}   {(IsObsolete ? "Obsolete    ▀▄▀▄▀▄▀▄▀▄▀▄▀" : "The latest  ─╬─  ─╬─  ─╬─")}   .n:{(logNetVer ? VersionHelper.DotNetCoreVersionCmd() : "[skipped]")}   ");

      UpgradeUrgency = .6 + Math.Abs((VersionHelper.TimedVer - setupExeTime).TotalDays);
      AppVerToolTip = IsObsolete ? $" New version is available:   0.{setupExeTime:M.d.HHmm} \n\t         from  {setupExeTime:yyyy-MM-dd HH:mm}.\n Click to update. " : $" This is the latest version  {VersionHelper.CurVerStrYYMMDD} \n\t               from  {VersionHelper.TimedVer:yyyy-MM-dd HH:mm}. ";
    }
    catch (Exception ex) { Logger.LogError(ex, "│   ▄─▀─▄─▀─▄ -- Ignore"); }
  }

  string? _ds; public string DeploymntSrcExe { get => _ds ?? DeplConst.DeplSrcExe; set => _ds = value; }
  public IBpr Bpr { get; }
  public ILogger Logger { get; }
  public UserSettingsSPM UsrStgns { get; }
  public BaseMinVM? CurrentVM => _navigationStore.CurrentVM;
  public BaseMinVM? CurrentModalVM => _modalNavgnStore.CurrentVM;
  public bool IsOpen => _modalNavgnStore.IsOpen;

  [ObservableProperty] double upgradeUrgency = 1;         // in days
  [ObservableProperty] string appVerNumber = "0.0";
  [ObservableProperty] object appVerToolTip = "Old";
  [ObservableProperty] string busyMessage = "Loading...";
  [ObservableProperty] bool isDevDbg;
  [ObservableProperty] bool isObsolete;
  [ObservableProperty] bool isBusy; partial void OnIsBusyChanged(bool value) => _IsBusyStore.ChangIsBusy(value);
  bool _au; public bool IsAudible
  {
    get => _au; set
    {
      if (SetProperty(ref _au, value) && _ctored)
      {
        Bpr.SuppressTicks = Bpr.SuppressAlarm = !(UsrStgns.IsAudible = value);
        Logger.LogInformation($"│   user-pref-auto-poll:       IsAudible: {value} ■─────■");
      }
    }
  }
  bool _an; public bool IsAnimeOn
  {
    get => _an; set
    {
      if (SetProperty(ref _an, value) && _ctored)
      {
        UsrStgns.IsAnimeOn = value;
        Logger.LogInformation($"│   user-pref-auto-poll:       IsAnimeOn: {value} ■─────■");
      }
    }
  }

  void OnCurrentVMChanged() => OnPropertyChanged(nameof(CurrentVM));
  void OnCurrentModalVMChanged() { OnPropertyChanged(nameof(CurrentModalVM)); OnPropertyChanged(nameof(IsOpen)); }
  void OnIsBusy_Store_Changed(bool val) => IsBusy = val;

  IRelayCommand? _up; public IRelayCommand UpgradeSelfCmd => _up ??= new AsyncRelayCommand(PerformUpgradeSelf); async Task PerformUpgradeSelf()
  {
    try
    {
      IsBusy = true;
      BusyMessage = "Copying...";
      IsObsolete = false; // hide the clicked button lest user double-clicked on it.

      var p = new Process
      {
        StartInfo = new ProcessStartInfo()
        {
          FileName = DeploymntSrcExe,
          Arguments = $"{new WindowInteropHelper(_mainWin).Handle} {DeplConst.DeplSrcDir} {DeplConst.DeplTrgDir} {DeplConst.DeplTrgExe}",
          UseShellExecute = true
        }
      };

      Logger.LogInformation($"│   PerformUpgradeSelf() launched with args:  '{p.StartInfo.Arguments}'  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ▓▓  ■  ");

      Application.Current.MainWindow.WindowState = WindowState.Minimized;  // apparently Application.Current.MainWindow works better than _mainWin.
      //Application.Current.MainWindow.Hide();                             // apparently Application.Current.MainWindow works better than _mainWin.
      _ = p.Start();
      Application.Current.MainWindow.WindowState = WindowState.Normal;     // apparently Application.Current.MainWindow works better than _mainWin.
      await Task.Delay(20000);                       // keep it around for better user experience: at least Wait is shown, as opposed to abrupt dissapearance of the app.

      Application.Current.Shutdown();
    }
    catch (Exception ex)
    {
      ex.Pop(Logger);
    }
    finally
    {
      Application.Current.MainWindow.WindowState = WindowState.Normal;      // apparently Application.Current.MainWindow works better than _mainWin.
      Application.Current.MainWindow.Show();                                // apparently Application.Current.MainWindow works better than _mainWin.
      IsBusy = false;
      IsObsolete = true;
    }
  }
}