using AmbienceLib;
using System.Windows;

namespace TtsVoicesPocWpfApp;
public partial class MainWindow : Window
{
  readonly SpeechSynth _synth = new("key");

  public MainWindow() => InitializeComponent();

  void OnClose(object sender, RoutedEventArgs e) => Close();

  async void OnSpeak(object sender, RoutedEventArgs e)
  {
   await _synth.SpeakAsync(tbSpeech.Text);
  }
}