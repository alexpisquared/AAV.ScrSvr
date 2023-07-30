using AutoMapper;
namespace OleksaScrSvr.VM.Contracts;
public class UserSettingsSPM : StandardLib.Base.UserSettingsStore
{
  readonly bool _loaded;
  readonly ILogger? _logger;

  public UserSettingsSPM() => WriteLine("    UserSettingsSPM.Ctor(): Deserialized => Loading is done?");
  public UserSettingsSPM(ILogger lgr)
  {
    _logger = lgr;

    _logger.Log(LogLevel.Trace, "    UserSettingsSPM.Ctor(): Supplied by the DI => Loading here now...");

    if (_loaded) return;

    var fromFile = Load<UserSettingsSPM>();

    var dtoForThis = new MapperConfiguration(cfg => cfg.CreateMap<UserSettingsSPM, UserSettingsSPM>()).CreateMapper().Map<UserSettingsSPM>(fromFile); //not fun.

    PrefDtBsName = fromFile.PrefDtBsName;
    PrefSrvrName = fromFile.PrefSrvrName;
    PrefDtBsRole = fromFile.PrefDtBsRole;
    PrefAplctnId = fromFile.PrefAplctnId;
    AllowSave = fromFile.AllowSave;
    IsAudible = fromFile.IsAudible;
    IsAnimeOn = fromFile.IsAnimeOn;

    _loaded = true;
  }
  void SaveIf() { if (_loaded) { LastSave = DateTimeOffset.Now; Save(this); } }

  string _s = ".\\sqlexpress";  /**/ public string PrefSrvrName { get => _s; set { if (_s != value) { _s = value; SaveIf(); } } }
  string _d = "Inventory";      /**/ public string PrefDtBsName { get => _d; set { if (_d != value) { _d = value; SaveIf(); } } }
  string _r = "IpmUserRole";    /**/ public string PrefDtBsRole { get => _r; set { if (_r != value) { _r = value; SaveIf(); } } }
  bool _o;                      /**/ public bool AllowSave { get => _o; set { if (_o != value) { _o = value; SaveIf(); } } }
  int _a = -2;                  /**/ public int PrefAplctnId { get => _a; set { if (_a != value) { _a = value; SaveIf(); } } }
  bool _u;                      /**/ public bool IsAudible { get => _u; set { if (_u != value) { _u = value; SaveIf(); } } }
  bool _n;                      /**/ public bool IsAnimeOn { get => _n; set { if (_n != value) { _n = value; SaveIf(); } } }
  string _p = "IpmUserRole";    /**/ public string StartPage { get => _p; set { if (_p != value) { _p = value; SaveIf(); } } }
}