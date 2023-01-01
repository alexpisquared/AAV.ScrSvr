using System;
using System.Linq;
using System.Threading.Tasks;
using SpeechSynthLib;
namespace TtsVoicesPocConsoleApp;
public class VoiceTester : IDisposable
{
  public async Task<bool> Test1()
  {
    using (var _synth = new SpeechSynth())
    {
      var m = "Hi there. Hi there! Hi there?";

      var voices = _voicenames
       .Where(v => v.EndsWith("ural"))
       //.Where(v => v.StartsWith("zh-CN-Xia"))
       //.Where(v => v.StartsWith("en-A"))
       .ToArray();

      var speakingStyles = new[] { // https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/speech-synthesis-markup?tabs=csharp#adjust-speaking-styles
         "angry" ,               // XiaoxiaoNeural only
         //"sad" ,               // XiaoxiaoNeural only
         //"affectionate",       // XiaoxiaoNeural only
         //"newscast-formal" ,   // AriaNeural only 
         //"newscast-casual" ,   // AriaNeural only 
         //"customerservice" ,   // AriaNeural only 
         //"chat"            ,   // AriaNeural only 
         //"cheerful"        ,   // AriaNeural only 
         //"empathetic"          // AriaNeural only 
      };

      Console.WriteLine($"v-{voices.Length} x {speakingStyles.Length} styles = ");

      for (var v = 0; v < voices.Length; v++)
        for (var s = 0; s < speakingStyles.Length; s++)
          //await _synth.SpeakAsync(m, VMode.Prosody, voices[v], speakingStyle: speakingStyles[s]);
          await sayIt(_synth, m, voices, speakingStyles, s, v);

    }

    return true;
  }

  static async Task sayIt(SpeechSynth _synth, string m, string[] voices, string[] speakingStyles, int s, int v)
  {
    Console.WriteLine($" --- {m},   VMode.Express,   {voices[v],-26}   {speakingStyles[s],-16}");
    await _synth.SpeakAsync(m, VMode.Express, voice: voices[v], styleForExpressOnly: speakingStyles[s]);
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

  readonly string[] _voicenames = {
      // https://docs.microsoft.com/en-us/azure/cognitive-services/containers/container-image-tags?tabs=current
  "en-US-Aria", 
  //
  // https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/language-support#text-to-speech:
  //foreign 
      "ar-EG-Hoda", "ar-SA-Naayf",
  "zh-HK-Danny", 
  "zh-CN-Kangkang", "zh-CN-Yaoyao", "zh-TW-Yating", "zh-TW-Zhiwei", "hr-HR-Matej", "cs-CZ-Jakub", 
  "en-AU-Catherine", "en-CA-Linda", "en-IN-Heera", "en-IN-Ravi", "en-IE-Sean",
  "en-GB-George",
  "en-GB-George-apollo",
  "en-GB-Susan",
  "en-gb-susan-apollo", 
  "fr-CA-Caroline", "fr-FR-Julie", "fr-FR-Paul", "fr-CH-Guillaume", "de-AT-Michael", "de-DE-Stefan", "de-CH-Karsten", "el-GR-Stefanos", "he-IL-Asaf", "hi-IN-Hemant", "hi-IN-Kalpana", "hu-HU-Szabolcs", "id-ID-Andika", "it-IT-Cosimo",
  "ja-JP-Ayumi", "ja-JP-Ichiro", "ms-MY-Rizwan", "pt-BR-Daniel", "ro-RO-Andrei",
  "sk-SK-Filip", "sl-SI-Lado", "es-MX-Raul", "es-ES-Laura", "es-ES-Pablo",
  "ta-IN-Valluvar", "te-IN-Chitra", "th-TH-Pattara", "vi-VN-An",
  "ar-EG-SalmaNeural", "ar-EG-ShakirNeural", "ar-SA-ZariyahNeural", "ar-SA-HamedNeural", "bg-BG-KalinaNeural", "bg-BG-BorislavNeural", "ca-ES-AlbaNeural", "ca-ES-JoanaNeural", "ca-ES-EnricNeural",
  "zh-HK-HiuGaaiNeural", "zh-HK-HiuMaanNeural", "zh-HK-WanLungNeural",
  "zh-CN-XiaoxiaoNeural",
  "zh-CN-XiaoyouNeural",
  "zh-CN-YunyangNeural",
  "zh-CN-XiaohanNeural",
  "zh-CN-YunyeNeural", "zh-TW-HsiaoChenNeural", "zh-TW-HsiaoYuNeural", "zh-TW-YunJheNeural", "hr-HR-GabrijelaNeural", "hr-HR-SreckoNeural", "cs-CZ-VlastaNeural", "cs-CZ-AntoninNeural", "da-DK-ChristelNeural", "da-DK-JeppeNeural",
  "nl-NL-ColetteNeural", "nl-NL-FennaNeural", "nl-NL-MaartenNeural", "en-AU-NatashaNeural", "en-AU-WilliamNeural", "en-CA-ClaraNeural", "en-CA-LiamNeural", "en-IN-NeerjaNeural", "en-IN-PrabhatNeural", "en-IE-EmilyNeural", "en-IE-ConnorNeural",
  "en-GB-LibbyNeural",
  "en-GB-MiaNeural",
  "en-GB-RyanNeural",
  "en-US-AriaNeural",
  "en-US-JennyNeural",    "en-US-GuyNeural", "fi-FI-NooraNeural", "fi-FI-SelmaNeural", "fi-FI-HarriNeural",
  "fr-CA-SylvieNeural", "fr-CA-JeanNeural", "fr-FR-DeniseNeural", "fr-FR-HenriNeural", "fr-CH-ArianeNeural", "fr-CH-FabriceNeural", "de-AT-IngridNeural", "de-AT-JonasNeural", "de-DE-KatjaNeural", "de-DE-ConradNeural", "de-CH-LeniNeural", "de-CH-JanNeural", "el-GR-AthinaNeural", "el-GR-NestorasNeural", "he-IL-HilaNeural", "he-IL-AvriNeural", "hi-IN-SwaraNeural", "hi-IN-MadhurNeural", "hu-HU-NoemiNeural", "hu-HU-TamasNeural",
  "id-ID-GadisNeural", "id-ID-ArdiNeural", "it-IT-ElsaNeural", "it-IT-IsabellaNeural", "it-IT-DiegoNeural", "ja-JP-NanamiNeural", "ja-JP-KeitaNeural", "ko-KR-SunHiNeural", "ko-KR-InJoonNeural", "ms-MY-YasminNeural", "ms-MY-OsmanNeural", "nb-NO-IselinNeural", "nb-NO-PernilleNeural", "nb-NO-FinnNeural", "pl-PL-AgnieszkaNeural", "pl-PL-ZofiaNeural", "pl-PL-MarekNeural",
  "pt-BR-FranciscaNeural", "pt-BR-AntonioNeural", "pt-PT-FernandaNeural", "pt-PT-RaquelNeural", "pt-PT-DuarteNeural", "ro-RO-AlinaNeural", "ro-RO-EmilNeural",
  
  "sk-SK-ViktoriaNeural", "sk-SK-LukasNeural", "sl-SI-PetraNeural", "sl-SI-RokNeural", "es-MX-DaliaNeural", "es-MX-JorgeNeural", "es-ES-ElviraNeural", "es-ES-AlvaroNeural",
  "sv-SE-HilleviNeural", "sv-SE-SofieNeural", "sv-SE-MattiasNeural", "ta-IN-PallaviNeural", "ta-IN-ValluvarNeural", "te-IN-ShrutiNeural", "te-IN-MohanNeural", "th-TH-AcharaNeural", "th-TH-PremwadeeNeural", "th-TH-NiwatNeural", "tr-TR-EmelNeural", "tr-TR-AhmetNeural", "vi-VN-HoaiMyNeural", "vi-VN-NamMinhNeural"
    };

  public void Dispose()
  {
    Console.WriteLine(" --- disposing ... 000");
    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }
}
