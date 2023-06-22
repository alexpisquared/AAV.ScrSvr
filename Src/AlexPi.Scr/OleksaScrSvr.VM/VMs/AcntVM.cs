namespace OleksaScrSvr.VM.VMs;

public class AcntVM : BaseMinVM
{
  readonly AcntStore _accountStore;

  public string? Username => _accountStore.CurrentAcnt?.Username;
  public string? Email => _accountStore.CurrentAcnt?.Email;

  public ICommand NavigateHomeCommand { get; }

  public AcntVM(AcntStore accountStore, INavSvc homeNavSvc)
  {
    _accountStore = accountStore;

    NavigateHomeCommand = new NavigateCommand(homeNavSvc);

    _accountStore.CurrentAcntChanged += OnCurrentAcntChanged;
  }
  public override void Dispose()
  {
    _accountStore.CurrentAcntChanged -= OnCurrentAcntChanged;

    base.Dispose();
  }

  void OnCurrentAcntChanged()
  {
    OnPropertyChanged(nameof(Email));
    OnPropertyChanged(nameof(Username));
  }
}