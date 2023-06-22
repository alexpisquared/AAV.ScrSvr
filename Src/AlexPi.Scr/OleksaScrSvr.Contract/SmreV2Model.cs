using WpfUserControlLib.Extensions;

namespace OleksaScrSvr.Contracts;
public class OleksaScrSvrModel : OleksaScrSvrModelBase
{
  public OleksaScrSvrModel() { }
  public void Seed()
  {
    Users.ClearAddRangeAuto(new User[] {
      new User { /*Id = 1,*/ UserName = "JohnSmith" },
      new User { /*Id = 3,*/ UserName = "olepigid3" },
      new User { /*Id = 4,*/ UserName = "olepigid4" },
      new User { /*Id = 5,*/ UserName = "olepigid5" },
      new User { /*Id = 2,*/ UserName = "SantaClss" }});

    Roles.ClearAddRangeAuto(new Role[] {
      new Role { /*Id = 1,*/ RoleName = "CM-FELIX-BATCH" },
      new Role { /*Id = 3,*/ RoleName = "CM-FELIX-DEFAULT" },
      new Role { /*Id = 4,*/ RoleName = "CM-FELIX-DEV" },
      new Role { /*Id = 5,*/ RoleName = "CM-FELIX-DEV-TEST" },
      new Role { /*Id = 2,*/ RoleName = "CM-FELIX-PYTHON" }});
  }
  public ObservableCollection<Role> Roles { get; set; } = new();
  public ObservableCollection<User> Users { get; set; } = new();
  public ObservableCollection<RoleUserAssignment> RoleUserAssignments { get; set; } = new();

  public async Task<(string report, bool success)> LoadRealDataIntoModel(IRoleEditorService _reSvc, string env) => await _reSvc.LoadRealData(env, Roles, Users, RoleUserAssignments);
  public async Task<(string report, bool success)> ADEnrich(IRoleEditorService _reSvc, string env, bool ignoreCache) => await _reSvc.ADEnrich(env, Roles, Users, RoleUserAssignments, ignoreCache);
}