namespace OleksaScrSvr.Contracts;

[DataContract]
public class Role
{
  public Role() { }
  [DataMember] public string RoleName { get; set; } = "Unassigned";
  [JsonIgnore] public bool Selectd { get; set; }
  [JsonIgnore] public bool? Granted { get; set; }
}