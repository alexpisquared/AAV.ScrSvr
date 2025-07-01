namespace OleksaScrSvr.VM.VMs;
public partial class Page01MultiUnitVM : BaseDbVM
{
  public Page01MultiUnitVM(INavSvc loginNavSvc, ILogger lgr, IConfigurationRoot cfg, IBpr bpr, ISecForcer sec, OleksaScrSvrModel inv, IAddChild win, UserSettingsSPM usrStgns, AllowSaveStore allowSaveStore, IsBusyStore IsBusyStore) : base(lgr, cfg, bpr, sec, inv, win, allowSaveStore, IsBusyStore, usrStgns, 8110)
  {
  }
  public override async Task<bool> InitAsync()
  {
    try
    {
      IsBusy = true;
      return await base.InitAsync();
    }
    finally { IsBusy = false; }
  }
  public override async Task<bool> WrapAsync()
  {
    try
    {
      IsBusy = true;
      return await base.WrapAsync();
    }
    finally { IsBusy = false; }
  }

  [ObservableProperty] string clientId; // what is this for (2025)
}