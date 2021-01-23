using AAV.Sys.Helpers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlexPi.Scr.Logic
{
  public class SpeakerJob
  {
    Random _random = new Random(DateTime.Now.Millisecond);
    readonly IConfigurationRoot _config;
    readonly string[]
      _voiceFafZNMA,
      _voiceAltZNMA,
      _greetingsZ,
      _greetingsN,
      _greetingsM,
      _greetingsA;

    public SpeakerJob()
    {
      _config = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.AlexPi.Scr.json")
        .AddUserSecrets<SpeakerJob>()
        .Build();

      _greetingsZ = _config.GetSection("greetingsZ").Get<string[]>();
      _greetingsN = _config.GetSection("greetingsN").Get<string[]>();
      _greetingsM = _config.GetSection("greetingsM").Get<string[]>();
      _greetingsA = _config.GetSection("greetingsA").Get<string[]>();
      _voiceFafZNMA = _config.GetSection("voiceFafZNMA").Get<string[]>();
      _voiceAltZNMA = _config.GetSection("voiceAltZNMA").Get<string[]>();
    }

    public string FirstNameFromUsername(string username)
    {
      var lower = username.ToLower();

      if (lower.Contains("zoe")) return "Зойка";
      if (lower.Contains("ale")) return "Шураня";
      if (lower.Contains("mei")) return "静美";
      if (lower.Contains("din")) return "Надя";

      return username;
    }

    public string GreetingsFromUsername(string username)
    {
      var lower = username.ToLower();

      if (lower.Contains("zo")) return _greetingsZ[_random.Next(_greetingsZ.Length)];
      if (lower.Contains("di")) return _greetingsN[_random.Next(_greetingsN.Length)];
      if (lower.Contains("me")) return _greetingsM[_random.Next(_greetingsM.Length)];
      if (lower.Contains("al")) return _greetingsA[_random.Next(_greetingsA.Length)];

      return username;
    }
    public string VoicenameFromUsername(string username, bool isFaf = true)
    {
      var lower = username.ToLower();

      if (lower.Contains("zo")) return isFaf ? _voiceFafZNMA[0] : _voiceAltZNMA[0];
      if (lower.Contains("di")) return isFaf ? _voiceFafZNMA[1] : _voiceAltZNMA[1];
      if (lower.Contains("me")) return isFaf ? _voiceFafZNMA[2] : _voiceAltZNMA[2];
      if (lower.Contains("al")) return isFaf ? _voiceFafZNMA[3] : _voiceAltZNMA[3];

      return "en-GB-Susan";
    }


  }
}
