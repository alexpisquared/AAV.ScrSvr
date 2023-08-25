using StandardLib.Services;

namespace OleksaScrSvr.VM.VMs;
public class Page03RazerScSvVM : BaseDbVM
{
  readonly SqlPermissionsManager _spm;
  readonly BmsPermissionsManager _bpm;
  readonly string _constr;
  readonly BackgroundTask _backgroundTask;

  public Page03RazerScSvVM(SqlPermissionsManager spm, BmsPermissionsManager bpm, ILogger lgr, IConfigurationRoot cfg, StandardContractsLib.IBpr bpr, ISecForcer sec, OleksaScrSvr.Contract.OleksaScrSvrModel inv, IAddChild win, UserSettingsSPM usrStgns, AllowSaveStore allowSaveStore, IsBusyStore IsBusyStore) : base(lgr, cfg, bpr, sec, inv, win, allowSaveStore, IsBusyStore, usrStgns, 8110)
  {
    _spm = spm;
    _bpm = bpm;
    _constr = string.Format(Config[GenConst.SqlVerSpm] ?? "!d", UserSetgs.PrefSrvrName, UserSetgs.PrefDtBsName);
    _backgroundTask = new BackgroundTask(TimeSpan.FromSeconds(1), OnTimer);
  }
  public override async Task<bool> InitAsync()
  {
    if (_loading) return false;

    IsBusy = _loading = true;
    var sw = Stopwatch.StartNew();

    try
    {
      _backgroundTask.Start();
            
      Report = $"{VersionHelper.TimeAgo(sw.Elapsed, small: true)}";
      Logger.LogInformation($"│   {nameof(Page03RazerScSvVM)}.{nameof(InitAsync)}(): {Report.Replace("\n", " ")} \t {new string('■', (int)sw.Elapsed.TotalSeconds)} ");

      return await base.InitAsync();
    }
    catch (Exception ex) { IsBusy = false; ex.Pop(Logger); await Task.Yield(); return false; }
    finally { IsBusy = _loading = false; _inited = true; Bpr.Tick(); }
  }
  public override async Task<bool> WrapAsync()
  {
    try
    {
      IsBusy = true;
      await _backgroundTask.StopAsync();

      return await base.WrapAsync();
    }
    finally { IsBusy = false; }
  }
  void OnTimer() => Bpr.Beep(222, .2);
  public override async Task RefreshReloadAsync([CallerMemberName] string? cmn = "")
  {
    WriteLine($"TrWL:> ###### Caller> {cmn}->Page03RazerScSvVM.RefreshReloadAsync() ");
    await InitAsync(); // refresh on YOI change.
    await base.RefreshReloadAsync();
  }
}