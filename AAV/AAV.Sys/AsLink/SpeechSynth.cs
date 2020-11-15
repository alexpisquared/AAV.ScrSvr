using System.Speech.Synthesis;

namespace AAV.Sys.AsLink
{
  public class SpeechSynth
  {
    readonly bool _isSpeechOn = false;
    SpeechSynthesizer _synth = null;

    public SpeechSynth(bool isOn) => _isSpeechOn = isOn;

    public SpeechSynthesizer Synth
    {
      get
      {
        if (_synth == null)
        {
          _synth = new SpeechSynthesizer { Rate = 6, Volume = 50 };
          _synth.SelectVoiceByHints(gender: VoiceGender.Female);
        }

        return _synth;
      }
    }

    public void SpeakSynch(string msg) { if (_isSpeechOn) Synth.Speak(msg); }
    public void SpeakAsync(string msg) { if (_isSpeechOn) Synth.SpeakAsync(msg); }
  }
}
