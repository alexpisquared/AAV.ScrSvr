namespace AlexPi.Scr.Logic;

public class ConfigRandomizer
{
  readonly Random _random = new(DateTime.Now.Millisecond);
  readonly IConfigurationRoot _config;

  public ConfigRandomizer() => _config = new ConfigurationBuilder()
      .SetBasePath(AppContext.BaseDirectory)
      .AddJsonFile("appsettings.AlexPi.Scr.json")  // the last overwrites!!!
      .AddUserSecrets<ConfigRandomizer>()          // the last overwrites!!! <== <== <== <== <== <== <== 
      .Build();

  public string GetValue(string name) => _config[name] ?? "";

  public string GetRandomFromUserSection(string section)
  {
    var sn = $"{section}_{Environment.UserName}";
    var sa = _config.GetSection(sn).Get<string[]>();
    if (sa == null)
    {
      WriteLine($"\"{sn}\": [\"abc\", \"efg\", \"hij\"],      // <== if (sc?.Value == null)");
      return $"{section} not found in app settings nor secrets";
    }

    return sa[_random.Next(sa.Length)];
  }
}
