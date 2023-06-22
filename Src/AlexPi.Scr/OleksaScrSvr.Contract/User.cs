namespace OleksaScrSvr.Contracts;

[DataContract]
public class User
{
  public User() { }
  [DataMember] public string UserName { get; set; } = "UNKNOWN";  // frstLast =~= SamAccountName
  [DataMember] public string? FullName { get; set; }              // Last, First
  [DataMember] public string? EmailAddress { get; set; }
  [DataMember] public string? EmployeeId { get; set; }
  [JsonIgnore] public bool Selectd { get; set; }
  [JsonIgnore] public bool? Granted { get; set; }
}