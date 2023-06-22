using OleksaScrSvr.VM.Stores;

namespace OleksaScrSvr.VM.VMs;

public class ZeroVM : BaseMinVM
{
  readonly ZeroStore _accountStore;

  public string? Name { get; } = "Name Name Name ";
  public string? Desc { get; } = "Desc Desc Desc ";

  public ICommand NavigateHomeCommand { get; }

  public ZeroVM(ZeroStore accountStore, INavSvc homeNavSvc)
  {
    _accountStore = accountStore;

    NavigateHomeCommand = new NavigateCommand(homeNavSvc);

    _accountStore.CurrentZeroChanged += OnCurrentZeroChanged;
  }
  public override void Dispose()
  {
    _accountStore.CurrentZeroChanged -= OnCurrentZeroChanged;

    base.Dispose();
  }

  void OnCurrentZeroChanged()
  {
    OnPropertyChanged(nameof(Name));
    OnPropertyChanged(nameof(Desc));
  }
}