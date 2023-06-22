namespace OleksaScrSvr.Contracts;

[DataContract]
public class RoleUserAssignment
{
  public RoleUserAssignment() { }
  [DataMember] public string RoleId { get; set; } = "";
  [DataMember] public string UserId { get; set; } = "";

  public override string ToString() => $"{RoleId,26} - {UserId,-26}";
}