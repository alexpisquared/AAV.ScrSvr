using AmbienceLib;
using Microsoft.Extensions.Configuration;
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
    await _synth.SpeakAsync(tbSpeech.Text, voice: GetTheVoiceMatchingButtonContentWhichIsPresumablyExactlyMatchesTheCC_Names(((Button)sender)?.Content?.ToString()));
  }

  static string GetTheVoiceMatchingButtonContentWhichIsPresumablyExactlyMatchesTheCC_Names(string? voice)
  {
    return voice ?? SpeechSynth.CC.Polina;
  }
}