namespace BE.IP.ViewModel.Helpers;

public class IpmUserRole
{
  public const string ReadOnly = "IpmUserRoleRO";
  public const string WorkUser = "IpmUserRole";
  public const string IpmAdmin = "IpmUserRoleAdmin";
  public const string LognOnly = "Login Only";

  public static bool IsIpmRole(string q) => q == ReadOnly || q == WorkUser || q == IpmAdmin;
}

public class GenConst
{
  public const string
    SqlVerSpm = CfgName.SqlVerIpm, // limited by IpmRole access: to fully impersonate IpmRole-defined access.
    PathToSetupExe = @"\\bbsfile01\Public\Dev\AlexPi\IncomePaymentManager\Installer-ClickOnce\setup.exe";
}

public class RoleKey
{
  public const string Accs0 = "0";
  public const string Guest = "G";
  public const string ROUsr = "R";
  public const string Users = "U";
  public const string Admin = "A";
  public const string Devel = "D";
}

public enum AccessLevel
{
  Accs0 = 0,
  Guest = 10,
  ROUsr = 20,
  Users = 30,
  Admin = 40,
  Devel = 99
}

[Flags]
public enum IpmUserRoleEnum
{
  None =             /**/ 0b_0000_0000,  //  0
  SqlLoginOnly =     /**/ 0b_0000_0010,  //  2
  IpmUserRoleRO =    /**/ 0b_0000_0100,  //  4
  IpmUserRole =      /**/ 0b_0000_1000,  //  8
  IpmUserRoleAdmin = /**/ 0b_0001_0000,  // 16
  IpmUserRoleSat =   /**/ 0b_0010_0000,  // 32
  IpmUserRoleSun =   /**/ 0b_0100_0000,  // 64
  IsDbRole =         /**/ IpmUserRoleRO | IpmUserRole | IpmUserRoleAdmin
}