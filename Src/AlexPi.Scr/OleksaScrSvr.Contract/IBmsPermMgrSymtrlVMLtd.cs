namespace OleksaScrSvr.Contract;

public interface IBmsPermMgrSymtrlVMLtd
{
  void ReloadUsersForSelectRole(Role lastSelectPerm);
  void ReloadRolesForSelectUser(User lastSelectUser);
  Task<bool> ToggleGrant(object grantCell, string lastSelectUserId, string lastSelectRoleId);
}