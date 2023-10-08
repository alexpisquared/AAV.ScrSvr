using System;
using System.Threading.Tasks;
using AmbienceLib;
using Microsoft.Extensions.Logging;

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

public class SpeechSynth2 : SpeechSynth, IDisposable
{
  public SpeechSynth2(string speechKey, bool useCached = true, string voice = "en-US-AriaNeural", string speechSynthesisLanguage = "uk-UA", ILogger lgr = null, string pathToCache = "C:\\g\\AAV.Shared\\Src\\NetLts\\Ambience\\MUMsgs\\") : base(speechKey, useCached, voice, speechSynthesisLanguage, lgr, pathToCache)
  {
  }

  public void Dispose() => throw new NotImplementedException();

  public async Task<bool> test1()
  {
    using var _synth = new SpeechSynth("");
    var s = "Hi there?";
    await _synth.SpeakAsync(s, 1, 100, "es-ES-HelenaRUS");
    await _synth.SpeakAsync(s, 1, 100, "es-ES-HelenaRUS");

    //await _synth.SpeakAsync(s, a, "uk-UA-OstapNeural");
    //await _synth.SpeakAsync(s, a, "uk-UA-OstapNeural");  // bad ending
    //await _synth.SpeakAsync(s, a, "uk-UA-PolinaNeural"); // the best
    //await _synth.SpeakAsync(s, a, "uk-UA-OstapNeural");  // best question intonation

    return true;
  }
}
