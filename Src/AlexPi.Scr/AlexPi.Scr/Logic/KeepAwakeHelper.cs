using System;
using System.Runtime.InteropServices;

namespace AAV.SS.Logic
{
  [Obsolete("AAV-> Hangs the app...?", true)]
  public class KeepAwakeHelper
  {
    public static uint ExtendAwakePeriod() => (uint)(_org = SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED));
    public static uint KeepAwakeForever() => (uint)(_org = SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS));
    public static uint LockingBackToNormal() => (uint)(_org = SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS));
    public static void Restore() => SetThreadExecutionState(_org);

    static EXECUTION_STATE _org = EXECUTION_STATE.ES_CONTINUOUS;

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

    [Flags]
    public enum EXECUTION_STATE : uint
    {
      ES_AWAYMODE_REQUIRED = 0x00000040,
      ES_CONTINUOUS = 0x80000000,
      ES_DISPLAY_REQUIRED = 0x00000002,
      ES_SYSTEM_REQUIRED = 0x00000001
      // ES_USER_PRESENT = 0x00000004 -- Legacy flag, should not be used.
    }
  }
}
