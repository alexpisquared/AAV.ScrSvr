namespace OleksaScrSvr.VM.VMs;
public partial class Page02SlideshowVM : BaseDbVM
{
  public Page02SlideshowVM(INavSvc loginNavSvc, ILogger lgr, IConfigurationRoot cfg, IBpr bpr, SpeechSynth synth, ISecForcer sec, IAddChild win, UserSettingsSPM usrStgns, AllowSaveStore allowSaveStore, IsBusyStore IsBusyStore) : base(lgr, cfg, bpr, sec, null, win, allowSaveStore, IsBusyStore, usrStgns, 8110)
  {
    ClientId = Config[$"ClientId_{Environment.UserName}"] ?? $"no  'ClientId_{Environment.UserName}'  in cfg!!!";
    Synth = synth;
  }
  public override async Task<bool> InitAsync()
  {
    try
    {
      IsBusy = true;
      return await base.InitAsync();
    }
    finally { IsBusy = false; }
  }
  public override async Task<bool> WrapAsync()
  {
    try
    {
      IsBusy = true;
      return await base.WrapAsync();
    }
    finally { IsBusy = false; }
  }

  [ObservableProperty] string clientId;

  public SpeechSynth Synth { get; }
}