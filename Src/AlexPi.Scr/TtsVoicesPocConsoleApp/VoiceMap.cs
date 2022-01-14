using System;
using System.Collections.Generic;
using System.Linq;

namespace TtsVoicesPocConsoleApp
{
  public class VoiceMap
  {
    readonly string[] _vm = new[] {
      //"en-US-AriaNeural;newscast-formal,newscast-casual,customerservice,chat,cheerful,empathetic"             ,
      //"en-US-JennyNeural;customerservice,chat,assistant,newscast"                                             ,
      //"en-US-GuyNeural;newscast"                                                                              ,
      //"pt-BR-FranciscaNeural;calm"                                                                            ,
      "zh-CN-XiaoxiaoNeural;newscast,customerservice,assistant,chat,calm,cheerful,sad,angry,fearful,disgruntled,serious,affectionate,gentle,lyrical"      ,
      "zh-CN-YunyangNeural;customerservice"                                                                      ,
      //"zh-CN-YunyeNeural;calm,cheerful,sad,angry,fearful,disgruntled,serious"                                    ,
      /* No output at all:
      "zh-CN-YunxiNeural;cheerful,sad,angry,fearful,disgruntled,serious,depressed,embarrassed"                   ,
      "zh-CN-XiaohanNeural;cheerful,sad,angry,fearful,disgruntled,serious,embarrassed,affectionate,gentle"       ,
      "zh-CN-XiaomoNeural;cheerful,angry,fearful,disgruntled,serious,depressed,gentle"                           ,
      "zh-CN-XiaoxuanNeural;cheerful,angry,fearful,disgruntled,serious,depressed,gentle"                         ,
      "zh-CN-XiaoruiNeural;sad,angry,fearful" 
      */
    };
    readonly Dictionary<string, List<string>> _d = new();
    readonly Random _rand = new(DateTime.Now.Millisecond);

    public VoiceMap()
    {
      foreach (var item in _vm)
      {
        var a = item.Split(';');
        _d.Add(a[0], a[1].Split(',').ToList());
      }
    }

    public List<string> AllVoiceNames() => _d.Keys.ToList();
    public List<string> AllVoiceStyle(string voice)
    {
      if (!_d.ContainsKey(voice)) return null;

      var s = _d[voice];

      return s;
    }
    public string RandomVoice() => _d.Keys.ToArray()[_rand.Next(_d.Count)];
    public string RandomVoiceStyle(string voice)
    {
      if (!_d.ContainsKey(voice)) return null;

      var s = _d[voice];

      return s[_rand.Next(s.Count)];
    }
  }
}
