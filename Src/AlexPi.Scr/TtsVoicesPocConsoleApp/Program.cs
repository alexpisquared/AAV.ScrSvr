using SpeechSynthLib;
using System;
using System.Threading.Tasks;
using TtsVoicesPocConsoleApp;

/// <summary>
/// Apparently, 
///   ethnic voices can read their language and a very broken English
///   Suffix RUS does not read cyrillic at all: it is skipped altogether, while English is read OK.
/// </summary>

Console.WriteLine("Beginning ...");

try
{
  Console.WriteLine($"          domain : {System.DirectoryServices.ActiveDirectory.Domain.GetComputerDomain()}");
}
catch (Exception ex)
{
  Console.WriteLine($"          domain : {ex.Message}");
}

//await Test2();
//await Test3();

Console.WriteLine("          ... the end!");

static async Task Test3()
{
  var synth = new SpeechSynth();
  var vm = new VoiceMap();
  var msg =
    //"Привет! Hi there? " +
    "爸爸饿了";


  foreach (var v in vm.AllVoiceNames())
    foreach (var t in vm.AllVoiceStyle(v))
      await showAndSay(synth, msg, v, t);

  //Console.WriteLine("          ... press Space to continue VVVVVVVVVVV!");
  //do
  //{
  //  var v = vm.RandomVoice();
  //  var t = vm.RandomVoiceStyle(v);
  //  await showAndSay(synth, msg, v, t);
  //} while (Console.ReadKey(true).Key == ConsoleKey.Spacebar);
}

static async Task Test2() => await new VoiceTester().Test1();

static async Task showAndSay(SpeechSynth synth, string msg, string v, string t)
{
  Console.WriteLine($"   {v,-22} {t,-33}");
  await synth.SpeakAsync(msg, VMode.Express, v, t);
}
public class SpeechSynth2 : SpeechSynth, IDisposable
{
  public async Task<bool> test1()
  {
    using var _synth = new SpeechSynth();
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
