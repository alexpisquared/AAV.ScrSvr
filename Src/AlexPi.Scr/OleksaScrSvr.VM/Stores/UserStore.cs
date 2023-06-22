namespace OleksaScrSvr.Stores;

public class SrvrStore
{
  public event Action<ADSrvr>? SrvrAdded;
  public event Action<ADSrvr>? CurrentSrvrChanged;

  public void AddSrvr(ADSrvr name) => SrvrAdded?.Invoke(name);
  public void ChgSrvr(ADSrvr name) => CurrentSrvrChanged?.Invoke(name);
}

public class DtBsStore
{
  public event Action<ADDtBs>? DtBsAdded;
  public event Action<ADDtBs>? CurrentDtbsChanged;

  public void AddDtBs(ADDtBs name) => DtBsAdded?.Invoke(name);
  public void ChgDtBs(ADDtBs name) => CurrentDtbsChanged?.Invoke(name);
}

public class UserStore
{
  public event Action<ADUser>? UserAdded;

  public void AddUser(ADUser name) => UserAdded?.Invoke(name);
}

public class ADUser
{
}

public class AllowSaveStore
{
  public event Action<bool>? AllowSaveChanged;

  public void ChangAllowSave(bool value) => AllowSaveChanged?.Invoke(value);
}

public class IsBusyStore
{
  public event Action<bool>? IsBusyChanged;

  public void ChangIsBusy(bool value) => IsBusyChanged?.Invoke(value);
}

