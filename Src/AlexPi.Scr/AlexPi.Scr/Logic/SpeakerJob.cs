using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;

namespace AlexPi.Scr.Logic
{
  public class SpeakerJob
  {
    readonly Random _random = new Random(DateTime.Now.Millisecond);
    readonly IConfigurationRoot _config;

    public SpeakerJob()
    {
      _config = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.AlexPi.Scr.json")
        .AddUserSecrets<SpeakerJob>()
        .Build();
    }

    public string GetRandomFromUserSection(string section)
    {
      var sn = $"{section}_{Environment.UserName}";
      var sa = _config.GetSection(sn).Get<string[]>();
      if (sa == null)
      {
        Trace.WriteLine($"\"{sn}\": [\"abc\", \"efg\", \"hij\"],      // <== if (sc?.Value == null)");
        return $"{section} not found in appstngs nor secret";
      }

      return sa[_random.Next(sa.Length)];
    }

  }
}
