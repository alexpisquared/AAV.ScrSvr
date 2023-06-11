namespace AlexPi.Scr;

internal class AppStartHelper
{
  internal static void InitAppSvcs(IServiceCollection services)
  {
    _ = services.AddSingleton<IConfigurationRoot>(ConfigHelper.AutoInitConfigHardcoded());

    _ = services.AddSingleton<ILogger>(sp => SeriLogHelper.InitLoggerFactory(@$"{(DevOps.IsDevMachine ? @"C:\temp" : @"\\oak\cm\felixdev\apps\data\Oleksa\Tooling\SmreV2\bin")}\Logs\{Assembly.GetExecutingAssembly().GetName().Name}.{Environment.UserName[..3]}..log", "-Info +Verb +Infi").CreateLogger<UnCloseableWindow>());

    _ = services.AddSingleton<IBpr, Bpr>(); // _ = VersionHelper.IsDbgAndRBD ? services.AddSingleton<IBpr, Bpr>() : services.AddSingleton<IBpr, BprSilentMock>();
  }
}