using System;

namespace AlexPi.Scr.Logic
{
  public class DailyBurn : TimeSplit
  {
    DateTime t2, t1;

    public DateTime T1 { get => t1; set => t1 = value; }
    public DateTime T2 { get => t2; set => t2 = value; }
    public TimeSpan TotalDTime { get => T2 - T1; }
  }
  public class TimeSplit
  {
    TimeSpan totalDaysUp, workedFor, idleOrOff;

    public TimeSpan UsefulTime { get => TotalDaysUp - IdleOrOff; }

    public TimeSpan WorkedFor { get => workedFor; set => workedFor = value; }
    public TimeSpan IdleOrOff { get => idleOrOff; set => idleOrOff = value; }
    public TimeSpan TotalDaysUp { get => totalDaysUp; set => totalDaysUp = value; }
  };
}
