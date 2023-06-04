﻿
namespace AsLink;

public enum StorageMode
{
  IsoProgDt,
  IsoUsrLcl,
  IsoUsrRoa,
  OneDriveU,
  OneDrAlex,
}

public partial class AppSettings
{
#if DEBUG
  const string suffix = ".DBG";
#else
  const string suffix = "";
#endif

  static readonly string _subFolder = $@"Public\AppData\{Assembly.GetExecutingAssembly().GetName().Name}\{Environment.MachineName}{suffix}.json";
  static readonly string _pathfile = OneDrive.Folder(_subFolder);
  readonly StorageMode _storMode;

  public AppSettings() : this(StorageMode.OneDriveU) { }
  public AppSettings(StorageMode storMode) => _storMode = storMode;

  public void Save()
  {
    if (_instance == null) return; //tu: ignore autosaves before fully rehydrated from the [iso] store.

    switch (_storMode)
    {
      default:
      case StorageMode.OneDriveU: JsonFileSerializer.Save<AppSettings>(_instance, _pathfile); break;
      case StorageMode.OneDrAlex: JsonFileSerializer.Save<AppSettings>(_instance, _pathfile); break;
      case StorageMode.IsoProgDt: JsonIsoFileSerializer.Save<AppSettings>(_instance); break;
      case StorageMode.IsoUsrLcl: JsonIsoFileSerializer.Save<AppSettings>(_instance, null, IsoConst.ULocA); break;
      case StorageMode.IsoUsrRoa: JsonIsoFileSerializer.Save<AppSettings>(_instance, null, IsoConst.URoaA); break;
    }
  }

  internal void SaveIfDirty_TODO() => Debug.WriteLine($" ** TODO: SaveIfDirty_TODO();  {_pathfile}");

  //[Obsolete("//todo: review in view of singleton.", true)]
  //public static void InitStore(StorageMode storageMode, string appSettingsFile = null) =>
  //  //todo: _storMode = storageMode;
  //  _pathfile = appSettingsFile ?? OneDrive.Folder(_subFolder);

  #region Singletone
  public static AppSettings Instance
  {
    get
    {
      if (_instance == null)
      {
        lock (_syncRoot)
        {
          _instance ??= JsonFileSerializer.Load<AppSettings>(_pathfile) ?? new AppSettings();
        }
      }

      return _instance;
    }
  }

  static volatile AppSettings? _instance;
  static readonly object _syncRoot = new(); // multithreading support
  #endregion
}
