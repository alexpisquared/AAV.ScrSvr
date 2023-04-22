using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Db.EventLog.Main
{
  [Flags]
  public enum EvOfIntFlag
  {
    ShutAndSleepDn = 1,     // Off pc
    ScreenSaverrUp = 2,     // Off ss
    ScreenSaverrDn = 4,     // On  ss
    BootAndWakeUps = 8,     // On  pc
    Day1stAmbiguos = 16,    // PC was on whole night ... but nobody's was there
    Was_Off_Ignore = 32,    // Off ignore
    Was_On__Ignore = 64,    // On  ignore
    StillWorkingOn = 128,   // On  ignore
    Who_Knows_What = 1024,
  }
}
