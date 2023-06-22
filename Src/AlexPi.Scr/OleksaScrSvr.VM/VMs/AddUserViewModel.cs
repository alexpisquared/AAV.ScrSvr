namespace OleksaScrSvr.VM.VMs;

public class AddSrvrVM : BaseMinVM
{
  ADSrvr? _u; public ADSrvr? Srvr { get => _u; set { _u = value; OnPropertyChanged(nameof(Srvr)); } }

  public ICommand SubmitCommand { get; }
  public ICommand CancelCommand { get; }

  public AddSrvrVM(SrvrStore userStore, INavSvc closeNavSvc)
  {
    SubmitCommand = new AddSrvrCommand(this, userStore, closeNavSvc);
    CancelCommand = new NavigateCommand(closeNavSvc);
  }
}
public class AddDtBsVM : BaseMinVM
{
  ADDtBs? _u; public ADDtBs? DtBs { get => _u; set { _u = value; OnPropertyChanged(nameof(DtBs)); } }

  public ICommand SubmitCommand { get; }
  public ICommand CancelCommand { get; }

  public AddDtBsVM(DtBsStore userStore, INavSvc closeNavSvc)
  {
    SubmitCommand = new AddDtBsCommand(this, userStore, closeNavSvc);
    CancelCommand = new NavigateCommand(closeNavSvc);
  }
}
public class AddUserVM : BaseMinVM
{
  ADUser _u; public ADUser User { get => _u; set { _u = value; OnPropertyChanged(nameof(User)); } }

  public ICommand SubmitCommand { get; }
  public ICommand CancelCommand { get; }

  public AddUserVM(UserStore userStore, INavSvc closeNavSvc)
  {
    SubmitCommand = new AddUserCommand(this, userStore, closeNavSvc);
    CancelCommand = new NavigateCommand(closeNavSvc);
    _u = new ADUser(/*"John Smith", "UsrPrincName", "SamAcntName", "Domain", "email@dd.com", DateTime.Today, "888"*/);
  }
}
