namespace OleksaScrSvr.VM.VMs;
public partial class BaseDbVM : BaseMinVM
{
  readonly ISecForcer _secForcer;
  protected bool _saving, _loading, _inited;
  protected readonly AllowSaveStore _allowSaveStore;
  protected readonly IsBusyStore _IsBusyStore;

  public BaseDbVM(ILogger lgr, IConfigurationRoot cfg, IBpr bpr, ISecForcer sec, OleksaScrSvrModel inv, IAddChild win, AllowSaveStore allowSaveStore, IsBusyStore IsBusyStore, UserSettingsSPM usrStgns, int oid)
  {
    IsDevDbg = VersionHelper.IsDbg;

    Logger = lgr;
    Config = cfg;
    SmreModel = inv;
    Bpr = bpr;
    _secForcer = sec;
    MainWin = (Window)win;
    UserSetgs = usrStgns;
    
    UserSetgs.IsAnimeOn = true;

    _allowSaveStore = allowSaveStore; _allowSaveStore.AllowSaveChanged += AllowSaveStore_AllowSaveChanged;
    _IsBusyStore = IsBusyStore; _IsBusyStore.IsBusyChanged += IsBusyStore_IsBusyChanged;

    AllowSave = IsDevDbg && _secForcer.CanEdit && (
      UserSetgs.PrefSrvrName is not null && !UserSetgs.PrefSrvrName.Contains("PROD", StringComparison.OrdinalIgnoreCase) && UserSetgs.AllowSave);

    Logger.LogInformation($"┌── eo-ctor {GetType().Name,-26}       PageRank:{oid}     IsAudible:{UserSetgs.IsAudible} ");
  }

  public override async Task<bool> InitAsync()
  {
    _inited = true;
    //Logger.LogInformation($"├── {GetType().Name} eo-init        ");
    await Task.Yield();
    return true;
  }
  public override async Task<bool> WrapAsync()
  {
    try
    {
      await Task.Yield();
      _allowSaveStore.AllowSaveChanged -= AllowSaveStore_AllowSaveChanged;
      _IsBusyStore.IsBusyChanged -= IsBusyStore_IsBusyChanged;
      return await base.WrapAsync();
    }
    catch (Exception ex) { IsBusy = false; ex.Pop(Logger); return false; }
    finally
    {
      Logger.LogInformation($"└── eo-wrap {GetType().Name,-26}      ");
    }
  }
  public virtual async Task RefreshReloadAsync([CallerMemberName] string? cmn = "") { WriteLine($"TrWL:> {cmn}->BaseDbVM.RefreshReloadAsync() "); await Task.Yield(); }

  void AllowSaveStore_AllowSaveChanged(bool val) { AllowSave = val; ; }
  void IsBusyStore_IsBusyChanged(bool val) { IsBusy = val; ; }

  public UserSettingsSPM UserSetgs { get; }
  public IConfigurationRoot Config { get; }
  public OleksaScrSvrModel SmreModel { get; protected set; }
  public ILogger Logger { get; }
  public IBpr Bpr { get; }
  public Window MainWin { get; }

  [ObservableProperty] bool isBusy; partial void OnIsBusyChanged(bool value) => _IsBusyStore.ChangIsBusy(value);
  [ObservableProperty] bool allowSave; partial void OnAllowSaveChanged(bool value) => _allowSaveStore.ChangAllowSave(value); //bool _awd; public bool AllowSave { get => _awd; set { if (SetProperty(ref _awd, value)) { /*UsrStgns.AllowSave = value;*/ _allowSaveStore.ChangAllowSave(value); } } }
  [ObservableProperty] bool isDevDbg;
  [ObservableProperty] bool hasChanges;
  [ObservableProperty] string report = "";

  [RelayCommand] async Task Save2Db() => Report = await SaveToJsonIf();

  public async Task<string> SaveToJsonIf(string note = "", [CallerMemberName] string? cmn = "")
  {
    var report = ($" {(AllowSave ? "Saved to JSON" : "RO Mode - nothing saved.")}. ({note} by {cmn})");

    if (AllowSave)
    {
      OleksaScrSvrModelBase.Save(SmreModel);

      await Bpr.TickAsync();
    }
    else
    {
      await Bpr.NoAsync();
    }

    Logger?.LogInformation(report);

    return report;
  }
  protected string? GetCaller([CallerMemberName] string? cmn = "") => cmn;
}