

namespace OleksaScrSvr.VM.VMs;

public class UserDetailVM : BaseMinVM
{
  public ADUser User { get; }

  public UserDetailVM(ADUser name) => User = name;
}

public class SrvrVM : BaseMinVM
{
  public ADSrvr Srvr { get; }

  public SrvrVM(ADSrvr name) => Srvr = name;
}

public class DtBsVM : BaseMinVM
{
  public ADDtBs DtBs { get; }

  public DtBsVM(ADDtBs name) => DtBs = name;
}
