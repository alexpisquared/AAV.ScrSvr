namespace Db.EventLog.Main;

[Flags]
public enum EventOfInterestFlag
{
  PowerOff = 1,    // including sleep mode state
  IdleBegin = 2,   // Off ss
  IdleFinish = 4,  // On  ss
  PowerOn = 8,     // including wake up state
  Day1stMaybe = 16,    // PC was on whole night ... but nobody's was there
  WasOffIgnore = 32,    // Off ignore
  WasOn_Ignore = 64,    // On  ignore
  NowBusy = 128,   // On  ignore
  Who_Knows_What = 1024,
}
