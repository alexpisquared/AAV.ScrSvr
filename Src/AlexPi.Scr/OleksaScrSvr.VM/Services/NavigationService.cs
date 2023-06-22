namespace OleksaScrSvr.Services;

public class NavSvc<TVM> : INavSvc where TVM : BaseMinVM
{
  readonly NavigationStore _navigationStore;
  readonly Func<TVM> _createVM;

  public NavSvc(NavigationStore navigationStore, Func<TVM> createVM)
  {
    _navigationStore = navigationStore;
    _createVM = createVM;
  }

  public void Navigate() => _navigationStore.CurrentVM = _createVM();
}