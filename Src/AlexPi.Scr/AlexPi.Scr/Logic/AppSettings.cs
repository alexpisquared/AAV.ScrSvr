namespace AsLink
{
  public partial class AppSettings
  {
    bool _ctrlA = true; public bool CtrlA { get => _ctrlA; set { if (_ctrlA != value) { _ctrlA = value; Save(); } } }
    bool _ctrlB = true; public bool CtrlB { get => _ctrlB; set { if (_ctrlB != value) { _ctrlB = value; Save(); } } }
    bool _ctrlC = true; public bool CtrlC { get => _ctrlC; set { if (_ctrlC != value) { _ctrlC = value; Save(); } } }
    bool _ctrlD = true; public bool CtrlD { get => _ctrlD; set { if (_ctrlD != value) { _ctrlD = value; Save(); } } }
    bool _ctrlE = true; public bool CtrlE { get => _ctrlE; set { if (_ctrlE != value) { _ctrlE = value; Save(); } } }
    bool _ctrlF = true; public bool CtrlF { get => _ctrlF; set { if (_ctrlF != value) { _ctrlF = value; Save(); } } }
    bool _ctrlG = true; public bool CtrlG { get => _ctrlG; set { if (_ctrlG != value) { _ctrlG = value; Save(); } } }
    bool _ctrlH = true; public bool CtrlH { get => _ctrlH; set { if (_ctrlH != value) { _ctrlH = value; Save(); } } }
    bool _ctrlI = true; public bool CtrlI { get => _ctrlI; set { if (_ctrlI != value) { _ctrlI = value; Save(); } } }
    bool _ctrlJ = false; public bool CtrlJ { get => _ctrlJ; set { if (_ctrlJ != value) { _ctrlJ = value; Save(); } } }
    bool _ctrlK = false; public bool CtrlK { get => _ctrlK; set { if (_ctrlK != value) { _ctrlK = value; Save(); } } }
    bool _ctrlL = false; public bool CtrlL { get => _ctrlL; set { if (_ctrlL != value) { _ctrlL = value; Save(); } } }

    bool is_KeepAwake; public bool KeepAwake { get => is_KeepAwake; set { if (is_KeepAwake != value) { is_KeepAwake = value; Save(); } } } // = AppSettings.Instance.KeepAwake;
    bool is___Locking; public bool AutoLocke { get => is___Locking; set { if (is___Locking != value) { is___Locking = value; Save(); } } } // = AppSettings.Instance.AutoLocke;
    bool is_AutoSleep; public bool AutoSleep { get => is_AutoSleep; set { if (is_AutoSleep != value) { is_AutoSleep = value; Save(); } } } // = AppSettings.Instance.AutoSleep;
    bool is___Heating; public bool IsHeaterOn { get => is___Heating; set { if (is___Heating != value) { is___Heating = value; Save(); } } } // = AppSettings.Instance.IsHeaterOn;
    bool __isSpeechOn; public bool IsSpeechOn { get => __isSpeechOn; set { if (__isSpeechOn != value) { __isSpeechOn = value; Save(); } } } // = AppSettings.Instance.IsSpeechOn;
    bool __IsSaySecOn; public bool IsSaySecOn { get => __IsSaySecOn; set { if (__IsSaySecOn != value) { __IsSaySecOn = value; Save(); } } } // = AppSettings.Instance.IsSaySecOn;
    bool __IsSayMinOn; public bool IsSayMinOn { get => __IsSayMinOn; set { if (__IsSayMinOn != value) { __IsSayMinOn = value; Save(); } } } // = AppSettings.Instance.IsSayMinOn;
    bool __IsChimesOn; public bool IsChimesOn { get => __IsChimesOn; set { if (__IsChimesOn != value) { __IsChimesOn = value; Save(); } } } // = AppSettings.Instance.IsChimesOn;
    
    int min2Sleep = 20; public int Min2Sleep { get => min2Sleep; set { if (min2Sleep != value) { min2Sleep = value; Save(); } } }
    int min2Locke = 20; public int Min2Locke { get => min2Locke; set { if (min2Locke != value) { min2Locke = value; Save(); } } }

    public string ImgPath { get; set; }
    public double DelayMin { get; set; }
    public double CrossFadeSec { get; set; }
    public double TransitionSec { get; set; }

    public long PeakWorkingSet64 { get; set; } = 1L;
    public long PeakPagMemSize64 { get; set; } = 1L;
    public long PeakVirMemSize64 { get; set; } = 1L;

    public const int AnimDurnInSec = 2; //5 * 60, // !! Keep in synch with xaml storyboards!!

  }
}
