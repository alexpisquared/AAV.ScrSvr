using AAV.SS.AltBpr;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace zPocConsoleApp
{
  class Program
  {
    public static async Task Main(string[] args)
    {
      //ShowConsoleColors(); Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine($"Hello World!  --- {Console.ForegroundColor}");

      //var f = 1000;
      //var d = 1 * 1000000 / f;
      //var l = new[] { new[] { f, Bpr.FixDuration(f, d) } };
      //await Bpr.BeepMks(l);
      //await Task.Delay(TimeSpan.FromSeconds(.333));

      do { await ChimerAlt.FreqRunUpHiPh(); } while (Console.ReadKey().Key != ConsoleKey.Escape);

      return;

      await ChimerAlt.FreqWalkUp();
      await Task.Delay(TimeSpan.FromSeconds(.333));
      Debug.WriteLine($"{ ChimerAlt.Freq(03) / ChimerAlt.Freq(02)}");
      Debug.WriteLine($"{ ChimerAlt.Freq(13) / ChimerAlt.Freq(12)}");
      await ChimerAlt.FreqWalkDn();
      return;
      await ChimerAlt.NoteWalk(1, 108, 120);
      await ChimerAlt.NoteWalk(100, 80, 200);
      await ChimerAlt.NoteWalk(30, 1, 30);
      await ChimerAlt.NoteWalk(1, 30, 30);


      var min = 30;
      var max = 300;
      await ChimerAlt.FreqWalk(max, min, durationSec: 1, durnMultr: 1, frMultr: 1.04);
      await ChimerAlt.FreqWalk(min, max, durationSec: 1, durnMultr: 1, frMultr: 1.04);
      await Task.Delay(TimeSpan.FromSeconds(.333));
      //ChimerAlt.FreqWalk(max, min, durationSec: 1, durnMultr: 1, frMultr: 1.06);
      //ChimerAlt.FreqWalk(min, max, durationSec: 1, durnMultr: 1, frMultr: 1.06);
      //await Task.Delay(TimeSpan.FromSeconds(.333));
      //ChimerAlt.FreqWalk(max, min, durationSec: 1, durnMultr: 1, frMultr: 1.09);
      //ChimerAlt.FreqWalk(min, max, durationSec: 1, durnMultr: 1, frMultr: 1.09);

      await ChimerAlt.NoteWalk(50, 40, 50, 1.1); return;
      await ChimerAlt.NoteWalk(70, 40, 50, 1.1);


      var f0 = 100;
      var fd = new List<int[]>
      {
        //new[] { f0+20, 500000 },
        //new[] { f0+10, 500000 },
        //new[] { f0+00, 500000 },
      };

      fd.Add(new[] { 9000, Bpr.FixDuration(9000, 20000) });
      var fr = f0;
      for (var i = 0; i < 50 && fr > 1; i++, fr -= 1) { add(fr, 30000); }
      for (var i = 0; i < 50 && fr > 1; i++, fr += 1) { add(fr, 30000); }
      fd.Add(new[] { 9000, Bpr.FixDuration(9000, 20000) });

      Bpr.BeepMks(fd.ToArray()).Wait();

      await Task.Delay(TimeSpan.FromSeconds(.333)); Console.WriteLine(new string('~', 100));
      await Task.Delay(TimeSpan.FromSeconds(.333)); Console.WriteLine(new string('~', 100));

      void add(int f, int dur)
      {
        var durx = Bpr.FixDuration(f, dur);
        Console.WriteLine($" * add: {f,6:N0} Hz    {dur,6:N0}  =>  {durx,6:N0} ms    ");

        fd.Add(new[] { f, durx });
      }


      ShowConsoleColors();
    }
    public static void ShowConsoleColors()
    {
      /// Colorful package completely throws off the standard Console colors...
      /// ...
      Console.ResetColor(); Console.Write($" Reset ");
      for (var i = 0; i < 8; i++)
      {
        Console.ForegroundColor = (ConsoleColor)(i + 0); Console.Write($"{Console.ForegroundColor} ");
        Console.ForegroundColor = (ConsoleColor)(i + 8); Console.Write($"{Console.ForegroundColor} ");
      }
      System.Console.ResetColor(); Console.Write($" Reset ");
      Console.ForegroundColor = ConsoleColor.DarkCyan; Console.Write($" DarkCyan \n");
    }
  }
}