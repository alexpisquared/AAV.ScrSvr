namespace OleksaScrSvr.Services;

public class LayoutNavSvc<TVM> : INavSvc where TVM : BaseMinVM
{
  readonly NavigationStore _navigationStore;
  readonly Func<NavBarVM> _createNavBarVM;
  readonly Func<TVM> _createVM;

  public LayoutNavSvc(NavigationStore navigationStore, Func<TVM> createVM, Func<NavBarVM> createNavBarVM)
  {
    _navigationStore = navigationStore;
    _createVM = createVM;
    _createNavBarVM = createNavBarVM;
  }

  public async void Navigate()
  {
    if (_navigationStore.CurrentVM is not null && 
      ((LayoutVM)_navigationStore.CurrentVM).ContentVM is not null &&
      await ((LayoutVM)_navigationStore.CurrentVM).ContentVM.TryWrapAsync() == false)
      return;

    _navigationStore.CurrentVM = new LayoutVM(_createNavBarVM(), _createVM());
    await ((LayoutVM)_navigationStore.CurrentVM).ContentVM.InitAsync();
  }
}

public class Page01MultiUnitNavSvc : LayoutNavSvc<Page01MultiUnitVM> { public Page01MultiUnitNavSvc(NavigationStore ns, Func<Page01MultiUnitVM> vm, Func<NavBarVM> nb) : base(ns, vm, nb) { } }
public class Page02SlideshowNavSvc : LayoutNavSvc<Page02SlideshowVM> { public Page02SlideshowNavSvc(NavigationStore ns, Func<Page02SlideshowVM> vm, Func<NavBarVM> nb) : base(ns, vm, nb) { } }
public class SqlNtvIpmPermMgrNavSvc : LayoutNavSvc<SqlNtvIpmPermMgrVM> { public SqlNtvIpmPermMgrNavSvc(NavigationStore ns, Func<SqlNtvIpmPermMgrVM> vm, Func<NavBarVM> nb) : base(ns, vm, nb) { } }
public class ClickOnceUpdaterNavSvc : LayoutNavSvc<ClickOnceUpdaterVM> { public ClickOnceUpdaterNavSvc(NavigationStore ns, Func<ClickOnceUpdaterVM> vm, Func<NavBarVM> nb) : base(ns, vm, nb) { } }

public class AcntNavSvc : LayoutNavSvc<AcntVM> { public AcntNavSvc(NavigationStore ns, Func<AcntVM> vm, Func<NavBarVM> nb) : base(ns, vm, nb) { } }
public class ZeroNavSvc : LayoutNavSvc<ZeroVM> { public ZeroNavSvc(NavigationStore ns, Func<ZeroVM> vm, Func<NavBarVM> nb) : base(ns, vm, nb) { } }
public class SrvrListingNavSvc : LayoutNavSvc<SrvrListingVM> { public SrvrListingNavSvc(NavigationStore ns, Func<SrvrListingVM> vm, Func<NavBarVM> nb) : base(ns, vm, nb) { } }
public class DtBsListingNavSvc : LayoutNavSvc<DtBsListingVM> { public DtBsListingNavSvc(NavigationStore ns, Func<DtBsListingVM> vm, Func<NavBarVM> nb) : base(ns, vm, nb) { } }
public class UserListingNavSvc : LayoutNavSvc<UserListingVM> { public UserListingNavSvc(NavigationStore ns, Func<UserListingVM> vm, Func<NavBarVM> nb) : base(ns, vm, nb) { } }
public class BpmsNavSvc : LayoutNavSvc<Page01MultiUnitVM> { public BpmsNavSvc(NavigationStore ns, Func<Page01MultiUnitVM> vm, Func<NavBarVM> nb) : base(ns, vm, nb) { } }
public class BpmsNa2Svc : LayoutNavSvc<Page02SlideshowVM> { public BpmsNa2Svc(NavigationStore ns, Func<Page02SlideshowVM> vm, Func<NavBarVM> nb) : base(ns, vm, nb) { } }
