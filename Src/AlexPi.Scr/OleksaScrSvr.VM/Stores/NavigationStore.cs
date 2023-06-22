namespace OleksaScrSvr.Stores;

public class NavigationStore
{
  BaseMinVM? _currentVM;
  public BaseMinVM?  CurrentVM
  {
    get => _currentVM;
    set
    {
      _currentVM?.Dispose();
      _currentVM = value;
      OnCurrentVMChanged();
    }
  }

  public event Action? CurrentVMChanged;

  void OnCurrentVMChanged() => CurrentVMChanged?.Invoke();
}
