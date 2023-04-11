﻿namespace AlexPi.Scr.Logic;

public class ConfigRandomizer
{
  readonly Random _random = new(DateTime.Now.Millisecond);
  readonly IConfigurationRoot _config;

  public ConfigRandomizer() => _config = new ConfigurationBuilder()
      .SetBasePath(AppContext.BaseDirectory)
      .AddJsonFile("appsettings.AlexPi.Scr.json")  // the last overwrites!!!
      .AddUserSecrets<ConfigRandomizer>()          // the last overwrites!!! <== <== <== <== <== <== <== 
      .Build();

  public string GetRandomFromUserSection(string section)
  {
    var sn = $"{section}_{Environment.UserName}";
    var sa = _config.GetSection(sn).Get<string[]>();
    if (sa == null)
    {
      WriteLine($"\"{sn}\": [\"abc\", \"efg\", \"hij\"],      // <== if (sc?.Value == null)");
      return $"{section} not found in appstngs nor secret";
    }

    return sa[_random.Next(sa.Length)];
  }

  public string GetValue(string sn) => _config[sn] ?? "";
}