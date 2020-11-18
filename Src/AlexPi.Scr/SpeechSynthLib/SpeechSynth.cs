using Microsoft.CognitiveServices.Speech;
using System;
using System.Threading.Tasks;

namespace SpeechSynthLib
{
  public class SpeechSynth : IDisposable
  {
    SpeechSynthesizer _synth = null;
    bool _disposedValue;

    public SpeechSynthesizer Synth => _synth ??= new SpeechSynthesizer(SpeechConfig.FromSubscription("bdefa0157d1d4547958f8653a65f32d4", "canadacentral"));

    public async Task SpeakAsync(string msg)
    {
      using var result = await Synth.SpeakTextAsync(msg);

      if (result.Reason == ResultReason.SynthesizingAudioCompleted)
      {
        Console.WriteLine($"Speech synthesized to speaker for text [{msg}]");
      }
      else if (result.Reason == ResultReason.Canceled)
      {
        var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
        Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

        if (cancellation.Reason == CancellationReason.Error)
        {
          Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
          Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
          Console.WriteLine($"CANCELED: Did you update the subscription info?");
        }
      }
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!_disposedValue)
      {
        if (disposing)
        {
          _synth.Dispose();
        }

        _disposedValue = true;
      }
    }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }
}
