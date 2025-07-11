﻿namespace OleksaScrSvr;
public static class AppStartHelper
{
  public static void InitAppSvcs(IServiceCollection services)
  {
    _ = services.AddSingleton<IConfigurationRoot>(new ConfigRandomizer("appsettings.OleksaScrSvr.json").Config); //todo: use both secrets.json and appsettings.OleksaScrSvr.json.

    _ = services.AddSingleton<ILogger>(sp => SeriLogHelper.CreateLogger<MainNavView>("OleksaScrSvr", Properties.Settings.Default.MinLogLevel)); // _ = services.AddSingleton<ILogger>(sp => SeriLogHelper.CreateLogger<MainNavView>(Properties.Settings.Default.MinLogLevel)); // creates Microsoft.Extensions.DependencyInjection.RAZ.ale.Infi.log when ready to run compilation is set in Publish profile.

    _ = services.AddSingleton<IBpr, Bpr>(); // _ = VersionHelper.IsDbgAndRBD ? services.AddSingleton<IBpr, Bpr>() : services.AddSingleton<IBpr, BprSilentMock>();

    _ = services.AddTransient(sp => new Contract.OleksaScrSvrModel());

    _ = services.AddSingleton<SpeechSynth>(sp => SpeechSynth.Factory(
      sp.GetRequiredService<IConfigurationRoot>()["AppSecrets:MagicSpeech"] ?? throw new ArgumentNullException("\"AppSecrets:MagicSpeech\" ia not available in " +
        $"{sp.GetRequiredService<IConfigurationRoot>()["WhereAmI"]}."),
      sp.GetRequiredService<ILogger>()));

    OpenWeaWpfApp.AppStartHelper.InitOpenWeaServices(services);
  }
}