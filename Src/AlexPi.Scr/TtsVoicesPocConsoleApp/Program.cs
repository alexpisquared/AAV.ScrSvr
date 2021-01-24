using SpeechSynthLib;
using System;
using System.Threading.Tasks;

/// <summary>
/// Apparently, 
///   ethnic voices can read their language and a very broken English
///   Suffix RUS does not read cyrillic at all: it is skipped altogether, while English is read OK.
/// </summary>

namespace TtsVoicesPocConsoleApp
{
  class Program
  {
    static async Task Main(string[] args)
    {
      Console.WriteLine("Beginning ...");

      await Test2();

      Console.WriteLine("          ... the end!");
    }

    static async Task  Test2()
    {
      var spt2 = new VoiceTester();
      await spt2.Test1();
    }
  }

  public class SpeechSynth2 : SpeechSynth, IDisposable
  {
    public async Task<bool> test1()
    {
      using var _synth = new SpeechSynth();

      //await Task.Delay(500);

      var a = VMode.Prosody;
      var s = "Hi there?";
      await _synth.SpeakAsync(s, VMode.Prosody, "es-ES-HelenaRUS");
      await _synth.SpeakAsync(s, VMode.Express, "es-ES-HelenaRUS");

      //await _synth.SpeakAsync(s, a, "ru-RU-Irina");
      //await _synth.SpeakAsync(s, a, "ru-RU-Pavel");
      //await _synth.SpeakAsync(s, a, "ru-RU-EkaterinaRUS");    
      //await _synth.SpeakAsync(s, a, "ru-RU-DariyaNeural");    // bad ending
      //await _synth.SpeakAsync(s, a, "ru-RU-SvetlanaNeural");  // the best
      //await _synth.SpeakAsync(s, a, "ru-RU-DmitryNeural");    // best question intonation

      return true;
    }
  }
}