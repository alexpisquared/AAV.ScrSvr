


namespace OleksaScrSvr.VM.VMs;
public class ClickOnceUpdaterVM : BaseDbVM
{
  public ClickOnceUpdaterVM(  ILogger lgr, IConfigurationRoot cfg, StandardContractsLib.IBpr bpr, ISecForcer sec, OleksaScrSvr.Contract.OleksaScrSvrModel inv, IAddChild win, UserSettingsSPM usrStgns, AllowSaveStore allowSaveStore, IsBusyStore IsBusyStore) : base( lgr, cfg, bpr, sec, inv, win, allowSaveStore, IsBusyStore, usrStgns, 8110) => _ = Application.Current.Dispatcher.InvokeAsync(async () => { try { await Task.Yield(); } catch (Exception ex) { ex.Pop(Logger); } });    //tu: async prop - https://stackoverflow.com/questions/6602244/how-to-call-an-async-method-from-a-getter-or-setter

  public override async Task<bool> InitAsync()
  {
    try
    {
      IsBusy = true;
      await Task.Delay(2000);
      var sw = Stopwatch.StartNew();
      await new WpfUserControlLib.Services.ClickOnceUpdater(Logger).CopyAndLaunch(ReportProgress);
      Logger.Log(LogLevel.Trace, $"DB:  in {sw.ElapsedMilliseconds,8}ms  at SQL:{UserSetgs.PrefSrvrName} ▀▄▀▄▀▄▀▄▀");
      return true;
    }
    catch (Exception ex) { ex.Pop(Logger); return false; }
    finally { _ = await base.InitAsync(); }
  }
  public override Task<bool> WrapAsync() => base.WrapAsync();

  void ReportProgress(string msg) => ReportMessage = msg;
  string _bm = "Hang on...\n\n       ...updating the binaries.\n\n              Will relaunch when done."; public string ReportMessage { get => _bm; set => SetProperty(ref _bm, value); }

  public override void Dispose() => base.Dispose();
}