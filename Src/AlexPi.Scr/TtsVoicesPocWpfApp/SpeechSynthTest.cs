using AmbienceLib;
using Microsoft.Extensions.Logging;

public class SpeechSynthTest : SpeechSynth, IDisposable
{
  public SpeechSynthTest(string speechKey, bool useCached = true, string voice = "en-US-AriaNeural", string speechSynthesisLanguage = "uk-UA", ILogger? lgr = null, string pathToCache = "C:\\g\\AAV.Shared\\Src\\NetLts\\Ambience\\MUMsgs\\") : base(speechKey, useCached, voice, speechSynthesisLanguage, lgr, pathToCache)
  {
  }

  public new void Dispose() => throw new NotImplementedException();

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
