namespace OleksaScrSvr.VM.Stores;

public class AcntStore
{
  AcntTmp? _ca; public AcntTmp? CurrentAcnt { get => _ca; set { _ca = value; CurrentAcntChanged?.Invoke(); } }

  public bool IsLoggedIn => CurrentAcnt != null;

  public event Action? CurrentAcntChanged;

  public void Logout() => CurrentAcnt = null;
}

public class ZeroStore
{
  ZeroTmp? _ca; public ZeroTmp? CurrentZero { get => _ca; set { _ca = value; CurrentZeroChanged?.Invoke(); } }

  public bool IsLoggedIn => CurrentZero != null;

  public event Action? CurrentZeroChanged;

  public void Logout() => CurrentZero = null;
}
