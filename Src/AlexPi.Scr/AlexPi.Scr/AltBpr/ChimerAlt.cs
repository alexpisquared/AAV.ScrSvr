using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AlexPi.Scr.AltBpr
{
  public static class ChimerAlt
  {
    public static void FreqTable()
    {
      for (var note = 108; note > 0; note--) Debug.WriteLine($"{note,5} => {Freq(note),8:N0}");
      //  1 =>    27.5
      // 12 =>    52
      //108 => 13289.75
    }
    public static async Task FreqWalkUp() => await FreqWalk(32, 8000, durationSec: 30, durnMultr: 0.9925);
    public static async Task FreqWalkDn() => await FreqWalk(8000, 32, durationSec: 05, durnMultr: 1.0075);
    public static async Task FreqRunUpHiPh() => await FreqWalkUpDn(4000, 9000, durationSec: .3);
    public static async Task FreqRunUp() => await FreqWalk(48, 100, durationSec: 1.9);
    public static async Task FreqRunDn() => await FreqWalk(128, 48, durationSec: 1.9);
    public static async Task FreqWalkUpDn() { await FreqWalkUp(); await Task.Delay(333); await FreqWalkDn(); }
    public static async Task FreqRunUpDn() { await FreqRunUp(); await Task.Delay(999); await FreqRunDn(); }
    public static async Task Wake(int mks = 150111) => await NoteWalk(100, 101, mks );

    public static async Task FreqWalkUpDn(double freqA = 200, double freqB = 20, double durationSec = 1, double durnMultr = 1, double frMultr = 1.02)
    {
      if (freqA == freqB)
        throw new Exception($"Frequencies must be different: {freqA} and {freqB}");

      var freqDurn = new List<int[]>();
      var up = freqA < freqB;
      var stepsTtl = 2 * Math.Log(up ? freqB / freqA : freqA / freqB, frMultr);

      var stepDurationMks = 1000000 * durationSec / stepsTtl;
      Console.WriteLine($"Steps: {stepsTtl,8}   Step Duration: {stepDurationMks:N0} mks  => total time requested / actual: {durationSec} / {stepsTtl * stepDurationMks * .000001}.");

      for (double freq = freqA, j = 1;
        up ? freq <= freqB : freq >= freqB;
        freq = (up ? freq * frMultr : freq / frMultr), j++) fixDurnAndAdd((int)(stepDurationMks * Math.Pow(durnMultr, j)), (int)Math.Round(freq), freqDurn);
      Console.WriteLine($"");

      for (double freq = freqB, j = 1;
              !up ? freq <= freqA : freq >= freqA;
              freq = (!up ? freq * frMultr : freq / frMultr), j++) fixDurnAndAdd((int)(stepDurationMks * Math.Pow(durnMultr, j)), (int)Math.Round(freq), freqDurn);
      Console.WriteLine($"");

      await Bpr.BeepMks(freqDurn.ToArray());
    }
    public static async Task FreqWalk(double freqA = 200, double freqB = 20, double durationSec = 1, double durnMultr = 1, double frMultr = 1.02)
    {
      if (freqA == freqB)
        throw new Exception($"Frequencies must be different: {freqA} and {freqB}");

      var freqDurn = new List<int[]>();
      var up = freqA < freqB;
      var stepsTtl = Math.Log(up ? freqB / freqA : freqA / freqB, frMultr);

      var stepDurationMks = 1000000 * durationSec / stepsTtl;
      Debug.WriteLine($"Steps: {stepsTtl,8}   Step Duration: {stepDurationMks:N0} mks  => total time requested / actual: {durationSec} / {stepsTtl * stepDurationMks * .000001}.");

      for (double freq = freqA, j = 1; up ? freq < freqB : freq > freqB; freq = (up ? freq * frMultr : freq / frMultr), j++)
        fixDurnAndAdd((int)(stepDurationMks * Math.Pow(durnMultr, j)), (int)Math.Round(freq), freqDurn);

      await Bpr.BeepMks(freqDurn.ToArray());
    }
    public static async Task NoteWalk(int noteA = 108, int noteB = 11, int durationMks = 60000, double delta = 1)
    {
      var fullScale = new List<int[]>();

      if (durationMks < 10000)
        durationMks *= 1000;

      if (noteA < noteB)
        for (int note = noteA, j = 1; note < noteB; note++, j++) add((int)(durationMks * Math.Pow(delta, j)), fullScale, note);
      else
        for (int note = noteA, j = 1; note > noteB; note--, j++) add((int)(durationMks * Math.Pow(delta, j)), fullScale, note);

      await Bpr.BeepMks(fullScale.ToArray());

      static void add(int dur, List<int[]> scale, int note)
      {
        var hz = (int)Freq(note);
        Debug.WriteLine($"{note,5} => {Freq(note),8:N0}  {dur,8:N0}  ");
        scale.Add(new[] { hz, Bpr.FixDuration(hz, dur) });
      }
    }

    static void fixDurnAndAdd(int dur, int freq, List<int[]> freqDurn)
    {
      var hz = freq;
      var mks = Bpr.FixDuration(hz, dur);
      Console.WriteLine($"{freq,5} Hz :  {dur,8:N0} => {mks,8:N0} mks. ");
      freqDurn.Add(new[] { hz, mks });
    }
    public static double Freq(int note) => Math.Pow(2, (note - 49) / 12.0) * 440;
    public static async Task Chime(int min)
    {
      switch (min)
      {
        case 01: await Bpr.BeepMks(new[] { new[] { _a, _p + _p } }); break;
        case 02: await Bpr.BeepMks(new[] { _cd, _p2, _ad }); break;
        case 03: await Bpr.BeepMks(new[] { _ad, _bd, _cd, }); break;
        //se 04: await Bpr.BeepMks(new[] { _cd, _ad, __1, _cd, _ad }); break;
        case 04: await Bpr.BeepMks(new[] { _ad, _bd, _cd, _p1, _cd }); break;
        //se 05: await Bpr.BeepMks(new[] { _cd, _ad, __1, _cd, _ad, __1, _cd }); break;
        case 05: await Bpr.BeepMks(new[] { _ad, _bd, _cd, _p1, _cd, _ad }); break;
        case 06: await Bpr.BeepMks(new[] { _ad, _bd, _cd, _ad, _bd, _cd }); break;
        case 07: await Bpr.BeepMks(new[] { _ad, _bd, _cd, _ad, _bd, _cd, _p1, _ad }); break;
        case 08: await Bpr.BeepMks(new[] { _ad, _bd, _cd, _ad, _bd, _cd, _p1, _ad, _p1, _bd }); break;
        case 09: await Bpr.BeepMks(new[] { _ad, _bd, _cd, _ad, _bd, _cd, _p1, _ad, _p1, _bd, _p1, _cd }); break;
        case 10: await Bpr.BeepMks(new[] { _a4 }); ; break;
        case 11: await Bpr.BeepMks(new[] { _a4, _p1, _cd }); break;
        case 12: await Bpr.BeepMks(new[] { _a4, _p1, _cd, _ad }); break;
        case 13: await Bpr.BeepMks(new[] { _a4, _p1, _ad, _bd, _cd }); break;
        case 14: await Bpr.BeepMks(new[] { _a4, _p1, _cd, _ad, _p1, _cd, _ad }); break;
        case 15: await Bpr.BeepMks(new[] { _a4, _p1, _cd, _ad, _p1, _cd, _ad, _p1, _cd }); break;
        case 20: await Bpr.BeepMks(new[] { _a4, _p1, _a4 }); break;
        case 25: await Bpr.BeepMks(new[] { _a4, _p1, _a4, _cd, _ad, _p1, _cd, _ad, _p1, _cd }); break;
        case 30: await Bpr.BeepMks(new[] { _a4, _p1, _a4, _p1, _a4 }); break;
        case 40: await Bpr.BeepMks(new[] { _a4, _p1, _a4, _p1, _a4, _p1, _a4 }); break;
        case 50: await Bpr.BeepMks(new[] { _a4, _p1, _a4, _p1, _a4, _p1, _a4, _p1, _a4 }); break;
        case 60: await Bpr.BeepMks(new[] { _a4, _p1, _a2, _p1, _a4, _p1, _a4, _p1, _a4, _p1, _a4 }); break;
        default: break;
      }
    }
    #region Chimes 1 - 10

    const int _a = 4435, _b = 4699, _c = 4978, _1 = 60000, _2 = 90000, _3 = 120000, _p = 160000; // https://en.wikipedia.org/wiki/Piano_key_frequencies
    static readonly int[]
      _p1 = new[] { 20000, _p },
      _p2 = new[] { 20000, 2 * _p },
      _a2 = new[] { _a, 2 * _p },
      _a4 = new[] { _a, 4 * _p },
      _ad = new[] { _a, _p },
      _bd = new[] { _b, _p },
      _cd = new[] { _c, _p };


    static async Task pause() => await Task.Delay(0);

    #endregion
  }
}