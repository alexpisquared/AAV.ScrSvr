using System.Runtime.Serialization;

namespace ScreenTimeUsrCtrlLib.Views;

[DataContract]
public class TimeSplit
{
  //[XmlElement(Type = typeof(XmlTimeSpan))]  
  [DataMember] public string? DaySummary { get; set; }
  [DataMember] public TimeSpan WorkedFor { get; set; }
  [DataMember] public TimeSpan IdleOrOff { get; set; }
  [DataMember] public TimeSpan TotalDaysUp { get; set; }
  [DataMember] public List<WorkInterval> WorkIntervals { get; set; } = [];
  public TimeSpan TtlMinusIdl => TotalDaysUp - IdleOrOff;
};

[DataContract]
public class WorkInterval
{
  [DataMember] public DateTimeOffset TimeA { get; set; }
  [DataMember] public DateTimeOffset TimeZ { get; set; }
  [DataMember] public string? Notes { get; set; }
  public TimeSpan Duration => TimeZ - TimeA;
}