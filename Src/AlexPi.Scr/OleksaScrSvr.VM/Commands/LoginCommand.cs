using OleksaScrSvr.VM.Stores;

namespace OleksaScrSvr.Commands;

public class LoginCommand : CommandBase
{
  readonly LoginVM _viewModel;
  readonly AcntStore _accountStore;
  readonly LoginCloseMdlNavSvs _navigationService;

  public LoginCommand(LoginVM viewModel, AcntStore accountStore, LoginCloseMdlNavSvs navigationService)
  {
    _viewModel = viewModel;
    _accountStore = accountStore;
    _navigationService = navigationService;
  }

  public override void Execute(object? parameter)
  {
    var account = new AcntTmp()
    {
      Email = $"{_viewModel.Username}@test.com",
      Username = _viewModel.Username
    };

    _accountStore.CurrentAcnt = account;

    _navigationService.Navigate();
  }
}
