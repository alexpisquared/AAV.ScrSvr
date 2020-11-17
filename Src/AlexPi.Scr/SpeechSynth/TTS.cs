using System;
using System.Diagnostics;
using System.Threading.Tasks;
//using System.Speech.Synthesis;

namespace SpeechSynthLib
{
  public class SpeechSynth
  {
    static readonly DateTime Started = DateTime.Now;
    readonly bool _isSpeechOn;
    //SpeechSynthesizer _synth = null;

    public SpeechSynth(bool isOn)
    {
      Trace.WriteLine($"{DateTime.Now:yy.MM.dd HH:mm:ss.f} +{(DateTime.Now - Started):mm\\:ss\\.ff}    Ctor()     0/n     {Environment.CurrentDirectory}");
      _isSpeechOn = isOn;
    }

    //public SpeechSynthesizer Synth
    //{
    //  get
    //  {
    //    if (_synth == null)
    //    {
    //      _synth = new SpeechSynthesizer { Rate = 6, Volume = 50 };
    //      _synth.SelectVoiceByHints(gender: VoiceGender.Female);
    //    }

    //    return _synth;
    //  }
    //}

    public void SpeakSynch(string msg) { if (!_isSpeechOn) return; } //Task.Run(async () => await AltBpr.ChimerAlt.Chime(1)); }//Synth.Speak(msg); }
    public void SpeakAsync(string msg) { if (!_isSpeechOn) return; } //ask.Run(async () => await AltBpr.ChimerAlt.Chime(1)); }//Synth.SpeakAsync(msg); }
  }
}
/*    using (SpeechSynthesizer synth = new SpeechSynthesizer())  
      {  

        // Configure the audio output.   
        synth.SetOutputToDefaultAudioDevice();  

        // Create a PromptBuilder object and define the data types for some of the added strings.  
        PromptBuilder sayAs = new PromptBuilder();  
        sayAs.AppendText("Your");  
        sayAs.AppendTextWithHint("1st", SayAs.NumberOrdinal);  
        sayAs.AppendText("request was for");  
        sayAs.AppendTextWithHint("1", SayAs.NumberCardinal);  
        sayAs.AppendText("room, on");  
        sayAs.AppendTextWithHint("10/19/2012,", SayAs.MonthDayYear);  
        sayAs.AppendText("with early arrival at");  
        sayAs.AppendTextWithHint("12:35pm", SayAs.Time12);  

        // Speak the contents of the SSML prompt.  
        synth.Speak(sayAs);  
      }    
*/