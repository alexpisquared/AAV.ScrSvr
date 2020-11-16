using System.Threading.Tasks;

namespace AlexPi.Scr
{
  internal class SpeechSynth
  {
    readonly bool _isSpeechOn;

    public SpeechSynth(bool isSpeechOn) => _isSpeechOn = isSpeechOn;

    internal void SpeakSynch(string msg) => Task.Run(async () => await AltBpr.ChimerAlt.Chime(1));
    internal void SpeakAsync(string msg) => Task.Run(async () => await AltBpr.ChimerAlt.Chime(1));
  }
}