namespace OleksaScrSvr.VM.VMs;
public class BaseMinVM : ObservableValidator, IDisposable
{
  protected readonly DateTimeOffset _ctorTime = DateTimeOffset.Now;
  public virtual async Task<bool> InitAsync() { WriteLine($"[{DateTime.Now:HH:mm:ss} ---] ┌── Init of {GetType().Name,-26}  hash:{_ctorTime:HH:mm:ss.ffffffff}  "); await Task.Yield(); return true; }
  public virtual async Task<bool> WrapAsync() { WriteLine($"[{DateTime.Now:HH:mm:ss} ---] └── Wrap of {GetType().Name,-26}  hash:{_ctorTime:HH:mm:ss.ffffffff}  "); await Task.Yield(); return true; }

  bool _disposedValue;
  protected virtual void Dispose(bool disposing)
  {
    WriteLine($"[{DateTime.Now:HH:mm:ss} ---] └── Dispose {GetType().Name,-26}  hash:{_ctorTime:HH:mm:ss.ffffffff}  disposing:{disposing}  ");
    if (!_disposedValue)
    {
      if (disposing)
      {
        //todo: stores:     YearOfInterestStore.YearChanged -= YearOfInterestStore_YearChanged;
        // TODO: dispose managed state (managed objects)
      }

      // TODO: free unmanaged resources (unmanaged objects) and override finalizer
      // TODO: set large fields to null

      _disposedValue = true;
    }
  }
  // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
  // ~BaseDbVM()
  // {
  //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
  //     Dispose(disposing: false);
  // }
  public virtual void Dispose()
  {
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }
}