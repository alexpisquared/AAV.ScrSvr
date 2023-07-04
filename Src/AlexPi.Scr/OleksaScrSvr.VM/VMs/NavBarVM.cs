namespace OleksaScrSvr.VM.VMs;

public class NavBarVM : BaseMinVM
{
  readonly AcntStore _acntStore;
  readonly ZeroStore _zeroStore;
  readonly SrvrStore _srvrStore;
  readonly DtBsStore _dtbsStore;
  readonly AllowSaveStore _allowSaveStore;

  public NavBarVM(AcntStore acntStore, ZeroStore zeroStore, SrvrStore srvrStore, DtBsStore dtbsStore, AllowSaveStore allowSaveStore, 
    AcntNavSvc acntNavSvc, 
    ZeroNavSvc zeroNavSvc, 
    LoginPopupMdlNavSvc loginNavSvc, 
    LoginCloseMdlNavSvs closeNavSvc, 
    Page01MultiUnitNavSvc Page01MultiUnitNavSvc, 
    Page02SlideshowNavSvc Page02SlideshowNavSvc,
    Page03RazerScSvNavSvc Page03RazerScSvNavSvc, 
    SrvrListingNavSvc srvrListingNavSvc, 
    DtBsListingNavSvc dtbsListingNavSvc, 
    UserListingNavSvc usLsNavSvc, 
    Page03RazerScSvNavSvc userPermissionerNavSvc, 
    UserSettingsSPM usrStgns)
  {
    _r = Consts.SqlServerList.First();
    _b = "BR";
    _acntStore = acntStore;
    _zeroStore = zeroStore;
    _srvrStore = srvrStore;
    _dtbsStore = dtbsStore;
    _allowSaveStore = allowSaveStore; _allowSaveStore.AllowSaveChanged += _allowSaveStore_AllowSaveChanged;

    UsrStgns = usrStgns;
    NavigateAcntCommand = new NavigateCommand(acntNavSvc);
    NavigateZeroCommand = new NavigateCommand(zeroNavSvc);
    NavigateLoginCommand = new NavigateCommand(loginNavSvc);
    NavigateCloseCommand = new NavigateCommand(closeNavSvc);
    NavigatePage01MultiUnitCommand = new NavigateCommand(Page01MultiUnitNavSvc);
    NavigatePage02SlideshowCommand = new NavigateCommand(Page02SlideshowNavSvc);
    NavigatePage03RazerScSvCommand = new NavigateCommand(Page03RazerScSvNavSvc);
    NavigateSrvrListingCommand = new NavigateCommand(srvrListingNavSvc);
    NavigateDtBsListingCommand = new NavigateCommand(dtbsListingNavSvc);
    NavigateUserListingCommand = new NavigateCommand(usLsNavSvc);
    NavigateUserPermissionerCommand = new NavigateCommand(userPermissionerNavSvc);
    LogoutCommand = new LogoutCommand(_acntStore);

    _acntStore.CurrentAcntChanged += OnCurrentAcntChanged;
    _zeroStore.CurrentZeroChanged += OnCurrentZeroChanged;
    _srvrStore.CurrentSrvrChanged += OnCurrentSrvrChanged;
    _dtbsStore.CurrentDtbsChanged += OnCurrentDtbsChanged;

    PrefSrvrName = usrStgns.PrefSrvrName;
    PrefDtBsName = usrStgns.PrefDtBsName;
    AllowSave = usrStgns.AllowSave;

    IsEnabledAllowSave = true;

    IsDevDbg = VersionHelper.IsDbg;

    _awd = IsDevDbg && /*_secForcer.CanEdit &&*/ (
      UsrStgns.PrefSrvrName is not null && !UsrStgns.PrefSrvrName.Contains("PROD", StringComparison.OrdinalIgnoreCase) && UsrStgns.AllowSave);
  }

  override public async Task<bool> InitAsync() //todo: 12345
  {
    if (NavigatePage03RazerScSvCommand.CanExecute(null))
    {
      NavigatePage03RazerScSvCommand.Execute(null);
    }

    return await base.InitAsync();
  }

  void _allowSaveStore_AllowSaveChanged(bool val) { AllowSave = val; ; }
  void OnCurrentAcntChanged() => OnPropertyChanged(nameof(IsLoggedIn));
  void OnCurrentZeroChanged() => OnPropertyChanged(nameof(IsLoggedIn));
  void OnCurrentSrvrChanged(ADSrvr srvr) => PrefSrvrName = srvr.Name;  //OnPropertyChanged(nameof(PrefSrvrName)); }
  void OnCurrentDtbsChanged(ADDtBs dtbs) => PrefDtBsName = dtbs.Name;  //OnPropertyChanged(nameof(PrefDtBsName)); }

  bool _dev; public bool IsDevDbg { get => _dev; set => SetProperty(ref _dev, value); }
  bool _awd; public bool AllowSave { get => _awd; set { if (SetProperty(ref _awd, value)) { UsrStgns.AllowSave = value; _allowSaveStore.ChangAllowSave(value); } } }
  bool _iea; public bool IsEnabledAllowSave { get => _iea; set => SetProperty(ref _iea, value); }
  string _r; public string PrefSrvrName { get => _r; set => SetProperty(ref _r, value); }
  string _b; public string PrefDtBsName { get => _b; set => SetProperty(ref _b, value); }

  public bool IsLoggedIn => _acntStore.IsLoggedIn;
  public UserSettingsSPM UsrStgns { get; }

  public ICommand NavigateAcntCommand { get; }
  public ICommand NavigateZeroCommand { get; }
  public ICommand NavigateLoginCommand { get; }
  public ICommand NavigateCloseCommand { get; }
  public ICommand NavigatePage01MultiUnitCommand { get; }
  public ICommand NavigatePage02SlideshowCommand { get; }
  public ICommand NavigatePage03RazerScSvCommand { get; }
  public ICommand NavigateSrvrListingCommand { get; }
  public ICommand NavigateDtBsListingCommand { get; }
  public ICommand? NavigateRoleListingCommand { get; }
  public ICommand NavigateUserListingCommand { get; }
  public ICommand NavigateUserPermissionerCommand { get; }
  public ICommand LogoutCommand { get; }

  IRelayCommand? _sq; public IRelayCommand SwtchSqlSvrCmd => _sq ??= new AsyncRelayCommand<object>(SwitchSqlServer); async Task SwitchSqlServer(object? sqlServerTLA)
  {
    ArgumentNullException.ThrowIfNull(sqlServerTLA, nameof(sqlServerTLA));

    PrefSrvrName = UsrStgns.PrefSrvrName = Consts.SqlServerList.FirstOrDefault(r => r.Contains((string)sqlServerTLA, StringComparison.InvariantCultureIgnoreCase)) ?? Consts.SqlServerList.First();

    _ = Process.Start(new ProcessStartInfo(Assembly.GetEntryAssembly()?.Location.Replace(".dll", ".exe") ?? "Notepad.exe"));
    await Task.Delay(2600);
    System.Windows.Application.Current.Shutdown();
  }

  public override void Dispose()
  {
    _acntStore.CurrentAcntChanged -= OnCurrentAcntChanged;
    _zeroStore.CurrentZeroChanged -= OnCurrentZeroChanged;
    _srvrStore.CurrentSrvrChanged -= OnCurrentSrvrChanged;
    _dtbsStore.CurrentDtbsChanged -= OnCurrentDtbsChanged;
    _allowSaveStore.AllowSaveChanged -= _allowSaveStore_AllowSaveChanged;

    base.Dispose();
  }
}