using OleksaScrSvr.VM.Stores;

namespace OleksaScrSvr.VM.VMs;

public class LoginVM : BaseMinVM
{
  public LoginVM(AcntStore accountStore, LoginCloseMdlNavSvs loginNavSvc) => LoginCommand = new LoginCommand(this, accountStore, loginNavSvc);

  string _username = default!; public string Username { get => _username; set { _username = value; OnPropertyChanged(nameof(Username)); } }
  string _password = default!; public string Password { get => _password; set { _password = value; OnPropertyChanged(nameof(Password)); } }

  public ICommand LoginCommand { get; }
}