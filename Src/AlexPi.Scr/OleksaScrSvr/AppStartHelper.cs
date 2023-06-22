using OleksaScrSvr.API;
using OleksaScrSvr.Contracts;

namespace OleksaScrSvr;
public static class AppStartHelper
{
  public static void InitAppSvcs(IServiceCollection services)
  {
    _ = services.AddSingleton<IConfigurationRoot>(ConfigHelper.AutoInitConfigHardcoded());

    _ = services.AddSingleton<ILogger>(sp => SeriLogHelper.InitLoggerFactory(@$"{(DevOps.IsDevMachine ? @"C:\temp" : @"\\oak\cm\felixdev\apps\data\Oleksa\Tooling\OleksaScrSvr\bin")}\Logs\{Assembly.GetExecutingAssembly().GetName().Name}.{Environment.UserName[..3]}..log", "-Info +Verb +Infi").CreateLogger<MainNavView>());

    _ = services.AddSingleton<IBpr, Bpr>(); // _ = VersionHelper.IsDbgAndRBD ? services.AddSingleton<IBpr, Bpr>() : services.AddSingleton<IBpr, BprSilentMock>();

    _ = services.AddTransient(sp => new OleksaScrSvrModel());

#if !Obsolete
    //_ = services.AddSingleton<IRoleEditorService, RoleEditorServiceNxt>();
#else
    _ = services.AddSingleton<IRoleEditorService, RoleEditorServiceCLI>();
#endif
  }
}