namespace OleksaScrSvr.Services;

public class CloseModalNavSvc : INavSvc
{
  readonly ModalNavgnStore _navigationStore;

  public CloseModalNavSvc(ModalNavgnStore navigationStore) => _navigationStore = navigationStore;

  public void Navigate() => _navigationStore.Close();
}
