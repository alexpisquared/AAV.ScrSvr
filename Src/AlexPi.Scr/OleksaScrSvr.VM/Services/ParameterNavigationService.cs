namespace OleksaScrSvr.Services;

public class ParameterNavSvc<TParameter, TVM>
    where TVM : BaseMinVM
{
  readonly NavigationStore _navigationStore;
  readonly Func<TParameter, TVM> _createVM;

  public ParameterNavSvc(NavigationStore navigationStore, Func<TParameter, TVM> createVM)
  {
    _navigationStore = navigationStore;
    _createVM = createVM;
  }

  public void Navigate(TParameter parameter)
  {
    _navigationStore.CurrentVM = _createVM(parameter);
  }
}
