namespace OleksaScrSvr.VM.VMs;

public class DtBsListingVM : BaseMinVM
{
  readonly DtBsStore _userStore;

  readonly ObservableCollection<DtBsVM> _user;

  public IEnumerable<DtBsVM> DtBs => _user;

  public ICommand AddDtBsCommand { get; }

  public DtBsListingVM(DtBsStore userStore, AddDtBsMdlNavSvc addDtBsNavSvc)
  {
    _userStore = userStore;

    AddDtBsCommand = new NavigateCommand(addDtBsNavSvc);
    _user = new ObservableCollection<DtBsVM>
    {
      //???: why list of VMs?
      //new DtBsVM(new( "Santa", "santa@claus.com")),
      //new DtBsVM(new( "Mary", "mary@email.com")),
      //new DtBsVM(new( "John", "john@email.com"))
    };

    _userStore.DtBsAdded += OnDtBsAdded;
  }

  void OnDtBsAdded(ADDtBs name) => _user.Add(new DtBsVM(name));
}
