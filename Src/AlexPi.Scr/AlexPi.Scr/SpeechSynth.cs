using System.Threading.Tasks;

namespace AlexPi.Scr
{
  internal class SpeechSynth
  {
    readonly bool _isSpeechOn;

    public SpeechSynth(bool isSpeechOn) => _isSpeechOn = isSpeechOn;

    internal void SpeakSynch(string msg)
    {
      if (_isSpeechOn)
        Task.Run(async () => await AltBpr.ChimerAlt.Chime(1));
    }
    internal void SpeakAsync(string msg)
    {
      if (_isSpeechOn)
        Task.Run(async () => await AltBpr.ChimerAlt.Chime(1));
    }
  }
}