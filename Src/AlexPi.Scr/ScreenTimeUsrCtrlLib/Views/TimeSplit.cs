using System.Runtime.Serialization;

namespace UpTimeChart;

[DataContract]
public class TimeSplit
{
  //[XmlElement(Type = typeof(XmlTimeSpan))]  
  [DataMember] public string? DaySummary { get; set; }
  [DataMember] public TimeSpan WorkedFor { get; set; }
  [DataMember] public TimeSpan IdleOrOff { get; set; }
  [DataMember] public TimeSpan TotalDaysUp { get; set; }
  public TimeSpan TtlMinusIdl => TotalDaysUp - IdleOrOff;
};
