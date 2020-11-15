using AAV.SS.AltBpr;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AsyncBprPocConsoleApp
{
  class Program
  {
    static async Task Main(string[] args)
    {
      var sw = Stopwatch.StartNew();
      Console.ForegroundColor = ConsoleColor.Cyan;
      var ms = 100;

      //ms = 100; await Task.Delay(ms); Console.WriteLine($"{sw.Elapsed:ss\\.ffff}   {ms}  ***");
      //ms = 110; await Task.Delay(ms); Console.WriteLine($"{sw.Elapsed:ss\\.ffff}   {ms}   ***");
      //ms = 120; await Task.Delay(ms); Console.WriteLine($"{sw.Elapsed:ss\\.ffff}   {ms}    ***");
      //ms = 110; await Task.Delay(ms); Console.WriteLine($"{sw.Elapsed:ss\\.ffff}   {ms}   ***");
      //ms = 100; await Task.Delay(ms); Console.WriteLine($"{sw.Elapsed:ss\\.ffff}   {ms}  ***");

      //await Task.Delay(ms).ConfigureAwait(false); Console.WriteLine($"{sw.Elapsed:ss\\.ffff}   {ms}");
      //await Task.Delay(ms).ConfigureAwait(false); Console.WriteLine($"{sw.Elapsed:ss\\.ffff}   {ms}");
      //await Task.Delay(ms).ConfigureAwait(false); Console.WriteLine($"{sw.Elapsed:ss\\.ffff}   {ms}");

      do
      {
        var d = 180;
        const int _a = 4401, _b = 4801, _c = 5237, _1 = 30, _2 = 40, _3 = 50;

        var hzms = new[] {
          new[] { _a, d },          new[] { _b, d },          new[] { _c, d },
          new[] { _a, d },          new[] { _b, d },          new[] { _c, d },
          new[] { _a, d },          new[] { _b, d },          new[] { _c, d },
          new[] { _a, d },

          new[] { 20, d },
          new[] { 20, d },

          new[] { _c, d },
          new[] { _a, d },
          new[] { 20, d },
          new[] { _c, d },
          new[] { _a, d },
          new[] { 20, d },
          new[] { _c, d },
        };

        ms = 300; await Bpr.Beep(hzms); Console.WriteLine($"{sw.Elapsed:ss\\.ffff}   {ms}  ***");

      } while (Console.ReadKey().Key != ConsoleKey.Escape);
    }
  }
}