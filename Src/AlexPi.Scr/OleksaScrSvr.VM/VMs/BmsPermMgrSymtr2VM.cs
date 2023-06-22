namespace OleksaScrSvr.VM.VMs;
public partial class BmsPermMgrSymtr2VM : BaseDbVM//, IBmsPermMgrSymtr2VMLtd
{
  const string _dev = "DEV", _pro = "PROD";

  public BmsPermMgrSymtr2VM(INavSvc loginNavSvc, ILogger lgr, IConfigurationRoot cfg, IBpr bpr, ISecForcer sec, IAddChild win, UserSettingsSPM usrStgns, AllowSaveStore allowSaveStore, IsBusyStore IsBusyStore) : base(lgr, cfg, bpr, sec, null, win, allowSaveStore, IsBusyStore, usrStgns, 8110)
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

}