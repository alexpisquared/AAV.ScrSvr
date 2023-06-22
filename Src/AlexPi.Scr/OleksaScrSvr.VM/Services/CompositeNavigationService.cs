namespace OleksaScrSvr.Services;

public interface ICompositeNavSvc : INavSvc { }

public class CompositeNavSvc : ICompositeNavSvc
{
  readonly IEnumerable<INavSvc> _navigationServices;

  public CompositeNavSvc(params INavSvc[] navigationServices) => _navigationServices = navigationServices;

  public void Navigate() => _navigationServices.ToList().ForEach(navigationService => navigationService.Navigate());
}

public class LoginCloseMdlNavSvs : CompositeNavSvc
{
  public LoginCloseMdlNavSvs(CloseModalNavSvc cs, AcntNavSvc ns) : base(cs, ns) { }
}

public class LoginPopupMdlNavSvc : ModalNavSvc<LoginVM>
{
  public LoginPopupMdlNavSvc(ModalNavgnStore ms, Func<LoginVM> fl) : base(ms, fl) { }
}
