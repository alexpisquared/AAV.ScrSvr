using SpeechSynthLib;
using System;
using System.Threading.Tasks;

namespace TtsVoicesPocConsoleApp
{
  class Program
  {
    static async Task Main(string[] args)
    {
      Console.WriteLine("Hello World!");

      using var _synth = new SpeechSynth();
      //await _synth.SpeakAsync("Well well well. Nǐ jīntiān chīfànle ma? Zaraza. Зараза малая. 你好. End", "Faf");
      //await _synth.SpeakAsync("静美, 今天吃了吗. ", "Faf", "zh-CN-XiaoyouNeural");
      //await _synth.SpeakAsync("静美, 今天吃了吗. ", "___", "zh-CN-XiaoyouNeural");

      //await Task.Delay(500);

      await _synth.SpeakAsync("Что, Хулиганка малая, уроки поделала?", "___", "ru-RU-SvetlanaNeural");
      //await _synth.SpeakAsync("Что, Хулиганка малая, уроки поделала?", "___", "ru-RU-DariyaNeural");  - bad ending
      //await _synth.SpeakAsync("Что, Хулиганка малая, уроки поделала?", "___", "ru-RU-EkaterinaRUS");  - very rough

      Console.WriteLine("Hello World!");

    }
  }
}