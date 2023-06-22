namespace OleksaScrSvr.API;

public interface IRoleEditorService
{
  string RolesFolder(string env);
  Task<(bool success, List<string> rv, string er, TimeSpan runTime)> AssignRole(string env, string role, string user);
  Task<(bool success, List<string> rv, string er, TimeSpan runTime)> RevekeRole(string env, string role, string user);
  Task<(bool success, List<string> rv, string er, TimeSpan runTime)> ListRoleUsers(string env, string role);
  Task<(bool success, List<string> rv, string er, TimeSpan runTime)> ListUserRoles(string env, string user);
  Task<(string report, bool success)> LoadRealData(string env, ObservableCollection<Role> Roles, ObservableCollection<User> Users, ObservableCollection<RoleUserAssignment> RoleUserAssignments);
  Task<(string report, bool success)> ADEnrich(string env, ObservableCollection<Role> rles, ObservableCollection<User> usrs, ObservableCollection<RoleUserAssignment> ruas, bool ignoreCache);

  void PerfTest();
}