namespace OleksaScrSvr.VM.VMs;

public class LayoutVM : BaseMinVM
{
  public LayoutVM(NavBarVM navigationBarVM, BaseMinVM contentVM)
  {
    NavBarVM = navigationBarVM;
    ContentVM = contentVM;
  }

  public override Task<bool> InitAsync() => base.InitAsync();

  public NavBarVM NavBarVM { get; }
  public BaseMinVM ContentVM { get; }

  public override void Dispose()
  {
    NavBarVM.Dispose();
    ContentVM.Dispose();

    base.Dispose();
  }
}
