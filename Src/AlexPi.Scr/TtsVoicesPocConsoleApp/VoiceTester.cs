using SpeechSynthLib;
using System;
using System.Threading.Tasks;

namespace TtsVoicesPocConsoleApp
{
  public class VoiceTester : IDisposable
  {
    public async Task<bool> Test1()
    {
      using (var _synth = new SpeechSynth())
      {
        var m =
          "Is this awesome or what!";
        //"Привет. Awesome! Awesome? Are you OK?";
        //"Hi there. Hi there! Hi there?";
        var v_ = "en-US-JennyNeural";  //en-GB-MiaNeural"; no .?! // ru-RU-DmitryNeural";
        var se = "empathetic";
        var sc = "cheerful";
        //await _synth.SpeakAsync(m, VMode.Prosody, v, speakingStyle: se);
        //await _synth.SpeakAsync(m, VMode.Prosody, v, speakingStyle: sc);
        //await _synth.SpeakAsync(m, VMode.Express, v, speakingStyle: se);
        //await _synth.SpeakAsync(m, VMode.Express, v, speakingStyle: sc);

        var voices = new[] { // Currently, speaking style adjustments are supported for these neural voices:
          //"en-US-AriaNeural",
          //"en-US-JennyNeural","en-US-GuyNeural","pt-BR-FranciscaNeural",
          "zh-CN-XiaoxiaoNeural",
          //"zh-CN-YunyangNeural","zh-CN-YunyeNeural","zh-CN-YunxiNeural","zh-CN-XiaohanNeural","zh-CN-XiaomoNeural","zh-CN-XiaoxuanNeural",          "zh-CN-XiaoruiNeural"        
        };

        var speakingStyles = new[] { // https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/speech-synthesis-markup?tabs=csharp#adjust-speaking-styles
           "angry" ,               // XiaoxiaoNeural only
           //"sad" ,               // XiaoxiaoNeural only
           "affectionate",         // XiaoxiaoNeural only
           //"newscast-formal" ,   // AriaNeural only 
           //"newscast-casual" ,   // AriaNeural only 
           //"customerservice" ,   // AriaNeural only 
           //"chat"            ,   // AriaNeural only 
           //"cheerful"        ,   // AriaNeural only 
           //"empathetic"          // AriaNeural only 
        };

        for (var s = 0; s < speakingStyles.Length; s++)
        {
          for (var v = 0; v < voices.Length; v++)
          {
            //await _synth.SpeakAsync(m, VMode.Prosody, voices[v], speakingStyle: speakingStyles[s]);
            await NewMethod(_synth, m, voices, speakingStyles, s, v);
          }
        }

        //await _synth.SpeakAsync(s, a, "ru-RU-Irina");
        //await _synth.SpeakAsync(s, a, "ru-RU-Pavel");
        //await _synth.SpeakAsync(s, a, "ru-RU-EkaterinaRUS");    
        //await _synth.SpeakAsync(s, a, "ru-RU-DariyaNeural");    // bad ending
        //await _synth.SpeakAsync(s, a, "ru-RU-SvetlanaNeural");  // the best
        //await _synth.SpeakAsync(s, a, "ru-RU-DmitryNeural");    // best question intonation
      }
      return true;
    }

    static async Task NewMethod(SpeechSynth _synth, string m, string[] voices, string[] speakingStyles, int s, int v)
    {
      Console.WriteLine($" --- SpeakAsync(m,  VMode.Express,  voice:{voices[v],-26}  speakingStyle:{speakingStyles[s],-16}.");
      await _synth.SpeakAsync(m, VMode.Express, voice: voices[v], speakingStyle: speakingStyles[s]);
    }

    bool _disposedValue;
    protected virtual void Dispose(bool disposing)
    {
      Console.WriteLine($" --- disposing ... disposing=={disposing}    _disposedValue=={_disposedValue}");
      if (!_disposedValue)
      {
        if (disposing)
        {
          // TODO: dispose managed state (managed objects)
          Console.WriteLine(" --- disposing managed state (managed objects) ...");
        }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        _disposedValue = true;
      }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~VoiceTester()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
      Console.WriteLine(" --- disposing ... 000");
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }
}
