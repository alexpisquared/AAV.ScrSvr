using System;

namespace AlexPi.Scr
{
  internal class SpeechSynth
  {
    private object isSpeechOn;

    public SpeechSynth(object isSpeechOn) => this.isSpeechOn = isSpeechOn;

    internal void SpeakSynch(string msg) => throw new NotImplementedException();
    internal void SpeakAsync(string msg) => throw new NotImplementedException();
  }
}