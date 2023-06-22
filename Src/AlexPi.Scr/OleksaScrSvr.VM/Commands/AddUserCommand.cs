namespace OleksaScrSvr.Commands;

public class AddSrvrCommand : CommandBase
{
  readonly AddSrvrVM _addSrvrVM;
  readonly SrvrStore _userStore;
  readonly INavSvc _navigationService;

  public AddSrvrCommand(AddSrvrVM addSrvrVM, SrvrStore userStore, INavSvc navigationService)
  {
    _addSrvrVM = addSrvrVM;
    _userStore = userStore;
    _navigationService = navigationService;
  }

  public override void Execute(object? parameter)
  {
    ArgumentNullException.ThrowIfNull(_addSrvrVM.Srvr);
    _userStore.AddSrvr(_addSrvrVM.Srvr);
    _navigationService.Navigate();
  }
}

public class AddDtBsCommand : CommandBase
{
  readonly AddDtBsVM _addDtBsVM;
  readonly DtBsStore _userStore;
  readonly INavSvc _navigationService;

  public AddDtBsCommand(AddDtBsVM addDtBsVM, DtBsStore userStore, INavSvc navigationService)
  {
    _addDtBsVM = addDtBsVM;
    _userStore = userStore;
    _navigationService = navigationService;
  }

  public override void Execute(object? parameter)
{
    ArgumentNullException.ThrowIfNull(_addDtBsVM.DtBs);
    _userStore.AddDtBs(_addDtBsVM.DtBs);
    _navigationService.Navigate();
  }
}

public class AddUserCommand : CommandBase
{
  readonly AddUserVM _addUserVM;
  readonly UserStore _userStore;
  readonly INavSvc _navigationService;

  public AddUserCommand(AddUserVM addUserVM, UserStore userStore, INavSvc navigationService)
  {
    _addUserVM = addUserVM;
    _userStore = userStore;
    _navigationService = navigationService;
  }

  public override void Execute(object? parameter)
  {
    ArgumentNullException.ThrowIfNull(_addUserVM.User);
    _userStore.AddUser(_addUserVM.User);
    _navigationService.Navigate();
  }
}
