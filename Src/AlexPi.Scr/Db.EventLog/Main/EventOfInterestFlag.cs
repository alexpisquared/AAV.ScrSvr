namespace Db.EventLog.Main;

[Flags]
public enum EventOfInterestFlag
{
  ___Pwr = 1,    // including sleep mode state
  Idle___ = 2,   // Off ss
  ___Idle = 4,  // On  ss
  Pwr___ = 8,     // including wake up state
  Day1stMaybe = 16,    // PC was on whole night ... but nobody's was there
  WasOffIgnore = 32,    // Off ignore
  WasOn_Ignore = 64,    // On  ignore
  NowBusy = 128,   // On  ignore
  Who_Knows_What = 1024,
}

public enum ActivityProjected
{
  Unknown = 0,
  Off = 1,
  Idle = 2,
  Busy = 3,
}