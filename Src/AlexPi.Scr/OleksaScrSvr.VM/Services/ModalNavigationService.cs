namespace OleksaScrSvr.Services;

public class ModalNavSvc<TVM> : INavSvc where TVM : BaseMinVM
{
  readonly ModalNavgnStore _navigationStore;
  readonly Func<TVM> _createVM;

  public ModalNavSvc(ModalNavgnStore navigationStore, Func<TVM> createVM)
  {
    _navigationStore = navigationStore;
    _createVM = createVM;
  }

  public void Navigate() => _navigationStore.CurrentVM = _createVM();
}

public class AddSrvrMdlNavSvc : ModalNavSvc<AddSrvrVM> { public AddSrvrMdlNavSvc(ModalNavgnStore s, Func<AddSrvrVM> f) : base(s, f) { } }
public class AddDtBsMdlNavSvc : ModalNavSvc<AddDtBsVM> { public AddDtBsMdlNavSvc(ModalNavgnStore s, Func<AddDtBsVM> f) : base(s, f) { } }
public class AddUserMdlNavSvc : ModalNavSvc<AddUserVM> { public AddUserMdlNavSvc(ModalNavgnStore s, Func<AddUserVM> f) : base(s, f) { } }


