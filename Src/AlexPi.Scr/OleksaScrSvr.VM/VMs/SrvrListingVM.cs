namespace OleksaScrSvr.VM.VMs;

public class SrvrListingVM : BaseMinVM
{
  readonly SrvrStore __store;
  readonly ObservableCollection<SrvrVM> __vm;

  public SrvrListingVM(SrvrStore store, AddSrvrMdlNavSvc addSrvrNavSvc)
  {
    __store = store;

    AddSrvrCommand = new NavigateCommand(addSrvrNavSvc);
    
    __vm = new ObservableCollection<SrvrVM>
    {
      new SrvrVM(new( "Santa", "santa@claus.com")),
      new SrvrVM(new( "Mary", "mary@email.com")),
      new SrvrVM(new( "John", "john@email.com"))
    };

    __store.SrvrAdded += OnSrvrAdded;
  }

  public IEnumerable<SrvrVM> Srvr => __vm;
  public ICommand AddSrvrCommand { get; }

  void OnSrvrAdded(ADSrvr name) => __vm.Add(new SrvrVM(name));
}
