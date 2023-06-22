namespace OleksaScrSvr.Stores;

public class ModalNavgnStore
{
  BaseMinVM? _currentVM;
  public BaseMinVM? CurrentVM
  {
    get => _currentVM;
    set
    {
      _currentVM?.Dispose();
      _currentVM = value;
      OnCurrentVMChanged();
    }
  }

  public bool IsOpen => CurrentVM != null;

  public event Action? CurrentVMChanged;

  public void Close() => CurrentVM = null;

  void OnCurrentVMChanged() => CurrentVMChanged?.Invoke();
}
