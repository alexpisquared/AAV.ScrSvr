using AmbienceLib;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace TtsVoicesPocWpfApp;

public partial class MainWindow : Window
{
  readonly SpeechSynth _synth = new(new ConfigurationBuilder().AddUserSecrets<MainWindow>().Build()["MagicSpeech"] ?? throw new InvalidOperationException("MagicSpeech secret not found."));

  public MainWindow() => InitializeComponent();

  void OnClose(object sender, RoutedEventArgs e) => Close();

  async void OnSpeak(object sender, RoutedEventArgs e)
  {
    _synth.SpeakAsyncCancelAll();
    await _synth.SpeakAsync(tbSpeech.Text,
      voice: GetVoice((sender as Button)?.Tag?.ToString()),
      style: GetStyle((sender as Button)?.Tag?.ToString()));
  }

  static string GetVoice(string? voiceDotStyle) =>
    typeof(SpeechSynth.CC)
      .GetField(voiceDotStyle?.Split('.')[0] ?? "", BindingFlags.Public | BindingFlags.Static)?
      .GetValue(null) as string
    ?? SpeechSynth.CC.Polina;
  static string GetStyle(string? voiceDotStyle) =>
    voiceDotStyle?.Contains('.') is true
      ? voiceDotStyle.Split('.')[1]
      : "";
}