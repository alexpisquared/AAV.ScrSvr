namespace OleksaScrSvr.VM.VMs;

public class UserListingVM : BaseMinVM
{
  readonly UserStore _userStore;
  readonly ObservableCollection<UserDetailVM> _user;

  public UserListingVM(UserStore userStore, AddUserMdlNavSvc addUserNavSvc)
  {
    _userStore = userStore;

    AddUserCommand = new NavigateCommand(addUserNavSvc);
    
    _user = new ObservableCollection<UserDetailVM>
    {
    };

    _userStore.UserAdded += OnUserAdded;
  }

  public IEnumerable<UserDetailVM> User => _user;
  public ICommand AddUserCommand { get; }

  void OnUserAdded(ADUser name) => _user.Add(new UserDetailVM(name));
}