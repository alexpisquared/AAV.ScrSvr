using OleksaScrSvr.Contracts;

namespace OleksaScrSvr.VM.VMs;
public partial class BmsPermMgrSymtrlVM : BaseDbVM, IBmsPermMgrSymtrlVMLtd
{
  const string _dev = "DEV", _pro = "PROD";
  
  public BmsPermMgrSymtrlVM(INavSvc loginNavSvc, ILogger lgr, IConfigurationRoot cfg, IBpr bpr, ISecForcer sec, OleksaScrSvr.Contracts.OleksaScrSvrModel inv, IAddChild win, UserSettingsSPM usrStgns, AllowSaveStore allowSaveStore, IsBusyStore IsBusyStore) : base(lgr, cfg, bpr, sec, inv, win, allowSaveStore, IsBusyStore, usrStgns, 8110)
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

  public void ReloadRolesForSelectUser(User lastSelectUser) => throw new NotImplementedException();
  public void ReloadUsersForSelectRole(Role lastSelectPerm) => throw new NotImplementedException();
  public Task<bool> ToggleGrant(object grantCell, string lastSelectUserId, string lastSelectRoleId) => throw new NotImplementedException();
}