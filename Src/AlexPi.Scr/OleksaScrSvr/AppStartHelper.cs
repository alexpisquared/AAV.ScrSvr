namespace OleksaScrSvr;
public static class AppStartHelper
{
  public static void InitAppSvcs(IServiceCollection services)
  {
    _ = services.AddSingleton<IConfigurationRoot>(new ConfigRandomizer("appsettings.OleksaScrSvr.json").Config);

    _ = services.AddSingleton<ILogger>(sp => SeriLogHelper.InitLoggerFactory(@$"{Path.Combine(OneDrive.Root, @"Public")}\Logs\{Assembly.GetExecutingAssembly().GetName().Name}.{Environment.MachineName[..3]}.{Environment.UserName[..3]}..log", "-Info -Verb +Infi").CreateLogger<MainNavView>());

    _ = services.AddSingleton<IBpr, Bpr>(); // _ = VersionHelper.IsDbgAndRBD ? services.AddSingleton<IBpr, Bpr>() : services.AddSingleton<IBpr, BprSilentMock>();

    _ = services.AddTransient(sp => new Contract.OleksaScrSvrModel());

    _ = services.AddSingleton<SpeechSynth>(sp => SpeechSynth.Factory(
      sp.GetRequiredService<IConfigurationRoot>()["AppSecrets:MagicSpeech"] ?? throw new ArgumentNullException("\"AppSecrets:MagicSpeech\" ia not available."),
      sp.GetRequiredService<ILogger>()));
    
    OpenWeaWpfApp.AppStartHelper.InitOpenWeaServices(services);
  }
}