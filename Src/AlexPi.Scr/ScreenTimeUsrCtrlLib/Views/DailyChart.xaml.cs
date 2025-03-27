namespace UpTimeChart;
public partial class DailyChart
{
  readonly double _updatePeriodMin = StandardLib.Helpers.DevOps.IsDbg ? .1 : 1.0;
  TimeSplit _dailyTimeSplit = new();
  double _ah = 30, _aw = 30;
  readonly Brush cBlk = new SolidColorBrush(Color.FromRgb(0, 0, 0x28)), cPnk = new SolidColorBrush(Color.FromRgb(0x30, 0, 0));

  public DailyChart(DateTime trgDate, SortedList<DateTime, int> thisDayEois)
  {
    InitializeComponent();

    TrgDateC = trgDate;
    _thisDayEois = thisDayEois;

    Loaded += async (s, e) =>
    {
      while (canvasBar.ActualWidth <= 0) await Task.Delay(1); await Task.Delay(1);        // odd shorter 1at time without this line.

      if (trgDate >= DateTime.Today)
        _thisDayEois.Add(DateTime.Now, (int)EvOfIntFlag.StillWorkingOn); // current moment of checking the stuff for today.

      await ClearDrawAllSegmentsForSinglePC();

      if (TrgDateC >= DateTime.Today)
        _ = new BackgroundTaskDisposable(TimeSpan.FromMinutes(_updatePeriodMin), OnTimer_AddRectangle);
    };
  }

  async Task ClearDrawAllSegmentsForSinglePC()
  {
    try
    {
      _dailyTimeSplit = new();
      _ah = canvasBar.ActualHeight;
      _aw = canvasBar.ActualWidth;
      canvasBar.Children.Clear();
      await Task.Delay(0);

      DrawUpDnLine(TrgDateC);
    }
    catch (Exception ex) { ex.Pop(); }
  }
  void DrawUpDnLine(DateTime trgDate)
  {
    var pcClr = new SolidColorBrush(Color.FromRgb(0x00, 0x60, 0x00));
    //..Write($">>>-\tdrawUpDnLine():  {trgDate:d} ->> {pc,-16} \t");
    tbDaySummaryLocal.Text = "$@#";
    try
    {
      _ah = canvasBar.ActualHeight;
      _aw = canvasBar.ActualWidth;
      if (Debugger.IsAttached) Write($">>> {trgDate}:\n"); // WriteLine($">>>-\t{EvLogHelper.GetAllUpDnEvents(TrgDateC.AddDays(-10000), dTime).Count(),5}");

      if (_thisDayEois.Count() < 1)
        tbDaySummaryLocal.Text = $"{trgDate,9:ddd M-dd}   n/a";
      else
      {
        if (trgDate >= DateTime.Today) // if today, add current moment to the list.
        {
          _thisDayEois.RemoveAt(_thisDayEois.Count - 1);
          _thisDayEois.Add(DateTime.Now, (int)EvOfIntFlag.StillWorkingOn); // current moment of checking the stuff for today.
        }

        var eoi0 = _thisDayEois.FirstOrDefault();
        var prevEoiF = eoi0.Value == (int)EvOfIntFlag.ScreenSaverrDn ? EvOfIntFlag.ScreenSaverrUp :
                       eoi0.Value == (int)EvOfIntFlag.BootAndWakeUps ? EvOfIntFlag.ShutAndSleepDn : EvOfIntFlag.Day1stAmbiguos;

        foreach (var eoi in _thisDayEois)
        {
          addWkTimeSegment(TrgDateC, eoi.Key, prevEoiF, (EvOfIntFlag)eoi.Value, pcClr);

          TrgDateC = eoi.Key;
          prevEoiF = (EvOfIntFlag)eoi.Value;
        }

        var lastScvrUp = (_thisDayEois.Any(r => r.Value is ((int)EvOfIntFlag.ScreenSaverrUp) or ((int)EvOfIntFlag.ShutAndSleepDn)) ?
                        _thisDayEois.Where(r => r.Value is ((int)EvOfIntFlag.ScreenSaverrUp) or ((int)EvOfIntFlag.ShutAndSleepDn)).Last() : _thisDayEois.Last()).Key;

        var finalEvent = trgDate >= DateTime.Today ? DateTime.Now : _thisDayEois.Last().Key;

        _dailyTimeSplit.TotalDaysUp = finalEvent - _thisDayEois.First().Key;
        _dailyTimeSplit.WorkedFor = _dailyTimeSplit.WorkedFor.Add(finalEvent - _thisDayEois.Last().Key);

        tbDaySummaryLocal.Text = _dailyTimeSplit.DaySummary = GetDaySummary(trgDate, _dailyTimeSplit);

        if (trgDate >= DateTime.Today)
        {
          var filenameLocal = OneDrive.Folder($@"Public\AppData\EventLogDb\DayLog-{trgDate:yyMMdd}-{Environment.MachineName}.json");
          JsonFileSerializer.Save<TimeSplit>(_dailyTimeSplit, filenameLocal, true);
        }

        addUiElnt(.92 * _ah, 0, new Rectangle { Height = .08 * _ah, Width = _dailyTimeSplit.WorkedFor.TotalDays * _aw, Fill = new SolidColorBrush(Color.FromRgb(0, 255, 0)) });

        var remoteLog = OneDrive.Folder($@"Public\AppData\EventLogDb\DayLog-{trgDate:yyMMdd}-{(Environment.MachineName == "RAZER1" ? "NUC2" : "RAZER1")}.json");
        if (File.Exists(remoteLog))
        {
          var remoteTimeSplit = JsonSerializer.Deserialize<TimeSplit>(File.ReadAllText(remoteLog)) ?? new TimeSplit { DaySummary = "error" };
          foreach (var wi in remoteTimeSplit.WorkIntervals) { addWkTimeSegment(wi.TimeA.DateTime, wi.TimeZ.DateTime, new SolidColorBrush(Color.FromArgb(152, 0, 0, 250))); }

          addUiElnt(.0 * _ah, 0, new Rectangle { Height = .1 * _ah, Width = remoteTimeSplit.WorkedFor.TotalDays * _aw, Fill = new SolidColorBrush(Color.FromRgb(0, 0, 255)), ToolTip = "???" });

          tbDaySummaryLocal.Text += File.Exists(remoteLog) ? GetDaySummary(remoteTimeSplit) : "n/a";
        }
      }

      gridvroot.Background = (trgDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) ? cPnk : cBlk;
    }
    catch (Exception ex) { ex.Pop(); }
    finally { if (Debugger.IsAttached) WriteLine($"    ==> {tbDaySummaryLocal.Text} "); }
  }

  string GetDaySummar_(DateTime trgDate, TimeSplit timesplit) => $"{trgDate,9:ddd M-dd}  {timesplit.WorkedFor,5:h\\:mm}  {"■■■|■■■|■■■|■■■|■■■|■■■|■■■|■■■|■■■|■■■|■■■|■■■|"[..(int)(timesplit.WorkedFor.TotalHours * 4)]}";
  string GetDaySummary(DateTime trgDate, TimeSplit timesplit) => $"{trgDate,9:ddd M-dd}  {timesplit.WorkedFor,5:h\\:mm}  / ";
  string GetDaySummar_(TimeSplit timeSplit) => $"{timeSplit.WorkedFor,5:h\\:mm}  {"■■■|■■■|■■■|■■■|■■■|■■■|■■■|■■■|■■■|■■■|■■■|■■■|"[..(int)(timeSplit.WorkedFor.TotalHours * 4)]}";
  string GetDaySummary(TimeSplit timeSplit) => $"{timeSplit.WorkedFor,5:h\\:mm}  ";
  async void OnTimer_AddRectangle()
  {
    if (Assembly.GetEntryAssembly()?.GetName().Name?.Contains("EventLog") == true)
    {
      if (StandardLib.Helpers.DevOps.IsDbg) Console.Beep(333, 333);
      await ClearDrawAllSegmentsForSinglePC();
    }
    else
    {
      if (StandardLib.Helpers.DevOps.IsDbg) Console.Beep(3333, 111);
      addRectangle(3 * _ah / 4, _ah / 4, _aw * DateTime.Now.TimeOfDay.TotalDays, 3, Brushes.Gray, $"{DateTime.Now.TimeOfDay:h\\:mm}"); // now line

      var finalEvent = TrgDateC >= DateTime.Today ? DateTime.Now : _thisDayEois.Last().Key;

      _dailyTimeSplit.TotalDaysUp = finalEvent - _thisDayEois.First().Key;
      //? try later: _timesplit.WorkedFor = _timesplit.WorkedFor.Add(finalEvent - _thisDayEois.Last().Key);

      tbDaySummaryLocal.Text = GetDaySummary(TrgDateC, _dailyTimeSplit); // tbDaySummary.Text = $"{TrgDateC,9:ddd M-dd}  {_timesplit.WorkedFor,5:h\\:mm}"; // /{_timesplit.TotalDaysUp,5:h\\:mm}";
    }
  }

  void addRectangle(double top, double hgt, double left, double width, Brush brush, string? tooltip = null) => addUiElnt(top, left, new Rectangle { Width = width < 1 ? 1 : width, Height = hgt, /*Fill = brush,*/ ToolTip = tooltip ?? $"thlw: {top:N0}-{hgt:N0}-{left:N0}-{width:N0}." }); //addArcDtl(hgt, left, width);
  void addWkTimeSegment(DateTime timeA, DateTime timeB, EvOfIntFlag eoiA, EvOfIntFlag eoiB, Brush brh)
  {
    if (eoiA == EvOfIntFlag.ScreenSaverrUp && eoiB == EvOfIntFlag.BootAndWakeUps) eoiB = EvOfIntFlag.ScreenSaverrUp; // ignore odd pwr-on during scrsvr runs.
    if (eoiA == EvOfIntFlag.BootAndWakeUps && eoiB == EvOfIntFlag.ScreenSaverrDn) eoiA = EvOfIntFlag.ScreenSaverrUp; // ignore odd pwr-on during scrsvr runs.
    if (eoiA == EvOfIntFlag.BootAndWakeUps && eoiB == EvOfIntFlag.BootAndWakeUps) eoiB = EvOfIntFlag.ShutAndSleepDn; // ignore odd pwr-on during scrsvr runs. 2023-04
    if (eoiA == EvOfIntFlag.ScreenSaverrDn && eoiB == EvOfIntFlag.ShutAndSleepDn) eoiA = EvOfIntFlag.ScreenSaverrUp; // ignore odd scrsvr down in the middle of scrsvr run. 2023-04

    var tA = eoiA == EvOfIntFlag.ScreenSaverrUp ? timeA.AddSeconds(-Ssto_GpSec).TimeOfDay : timeA.TimeOfDay;
    var tB = eoiB == EvOfIntFlag.ScreenSaverrUp ? timeB.AddSeconds(-Ssto_GpSec).TimeOfDay : timeB.TimeOfDay;

    var dTime = tB - tA;

    var yA = _aw * tA.TotalDays; // for ss up - start idle line 2 min prior
    var yB = _aw * tB.TotalDays; // for ss dn - end   work line 2 min prior

    var hgt =
      eoiA == EvOfIntFlag.Day1stAmbiguos ? (_ah / 9) :
      eoiA == EvOfIntFlag.ScreenSaverrUp ? (_ah / 4) :
      eoiA == EvOfIntFlag.ScreenSaverrDn ? (_ah / 1) :
      eoiA == EvOfIntFlag.BootAndWakeUps ? (_ah / 1) :
      eoiA == EvOfIntFlag.Who_Knows_What ? (_ah / 8) : 0;

    var isLabor = eoiA is EvOfIntFlag.ScreenSaverrDn or EvOfIntFlag.BootAndWakeUps;
    if (isLabor)
    {
      _dailyTimeSplit.WorkedFor += dTime;
      if (dTime.TotalMinutes > 1)
        _dailyTimeSplit.WorkIntervals.Add(new WorkInterval { TimeA = timeA, TimeZ = timeB, Notes = $"{eoiA} - {eoiB}" });
    }
    else
      _dailyTimeSplit.IdleOrOff += dTime;

    var isUp = eoiA is EvOfIntFlag.ScreenSaverrDn or EvOfIntFlag.BootAndWakeUps;
    var top = _ah - hgt;
    var wid = Math.Abs(yB - yA);

    var tooltip = $"{(isUp ? $"+++ " : $"--- ")} \n {tA,8:h\\:mm\\:ss} - {tB,8:h\\:mm\\:ss} = {dTime,8:h\\:mm\\:ss} ";
    var report1 = $">>> from {eoiA} to {eoiB}    {(isLabor ? "++" : "--")}";

    //if (isLabor)
    var report2 = $"{tooltip.Replace("\n", " ")}    + {(isLabor ? dTime.ToString("hh\\:mm") : "")} = {(isLabor ? _dailyTimeSplit.WorkedFor.ToString("hh\\:mm") : "")}";

    addRectangle(top, hgt, yA, wid, brh, tooltip);

    if (wid <= 10)
      report2 += $"                    ==> width too small to add TEXT to UI: {dTime.TotalMinutes,5:N1} min  =>  {wid:N3} pxl ";
    else
    {
      var isOver1hr = dTime > TimeSpan.FromHours(1);
      addUiElnt(
        isUp ? 2 : -1,
        yB - (isOver1hr ? 27 : 19),
        new TextBlock
        {
          Text = isOver1hr ? $"{dTime:h\\:mm}" : $"{dTime,3:\\:m}",
          FontSize = isUp ? 12 : .1, // op: change to 11 if need to see the off times.
          Foreground = isUp ? Brushes.LimeGreen : Brushes.DodgerBlue,
          ToolTip = tooltip,
          Margin = isUp ? new Thickness(3, 2, -3, -2) : new Thickness(3, -2, -3, 2)
        });
    }

    if (Debugger.IsAttached) Write($"{report1} {report2}\n");
  }
  void addWkTimeSegment(DateTime timeA, DateTime timeB, Brush brh)
  {
    var tA = timeA.TimeOfDay;
    var tB = timeB.TimeOfDay;
    var dTime = tB - tA;
    var yA = _aw * tA.TotalDays; // for ss up - start idle line 2 min prior
    var yB = _aw * tB.TotalDays; // for ss dn - end   work line 2 min prior
    var hgt = .6 * _ah;
    var wid = Math.Abs(yB - yA);
    var tooltip = $"+++   \n {tA,8:h\\:mm\\:ss} - {tB,8:h\\:mm\\:ss} = {dTime,8:h\\:mm\\:ss} ";

    addUiElnt(0, yA, new Rectangle { Width = wid, Height = hgt, Fill = brh, ToolTip = tooltip });

    var isOver1hr = dTime > TimeSpan.FromHours(1);
    addUiElnt(2, yB - (isOver1hr ? 27 : 19), new TextBlock { Text = isOver1hr ? $"{dTime:h\\:mm}" : $"{dTime,3:\\:m}", FontSize = 12, Foreground = new SolidColorBrush(Color.FromRgb(60, 160, 255)), ToolTip = tooltip, Margin = new Thickness(3, -5, -3, 5) });
  }
  void addUiElnt(double top, double left, UIElement el) { Canvas.SetLeft(el, left); Canvas.SetTop(el, top); _ = canvasBar.Children.Add(el); }
  public static readonly DependencyProperty TrgDateCProperty = DependencyProperty.Register("TrgDateC", typeof(DateTime), typeof(DailyChart)); public DateTime TrgDateC { get => (DateTime)GetValue(TrgDateCProperty); set => SetValue(TrgDateCProperty, value); }

  #region DUPE_FROM  C:\C\Lgc\ScrSvrs\AlexPi.Scr\App.xaml.cs
  const int GraceEvLogAndLockPeriodSec = 60;
  static int _ssto = -1;
  readonly SortedList<DateTime, int> _thisDayEois;

  //// [Obsolete] :why Copilot decides to mark it such?
  public static int ScrSvrTimeoutSec
  {
    get
    {
      if (_ssto == -1)
      {
        _ssto = new EvLogHelper().GetSstoFromRegistry;
      }

      return _ssto;
    }
  }

  public static int Ssto_GpSec => ScrSvrTimeoutSec + GraceEvLogAndLockPeriodSec;  // ScreenSaveTimeOut + Grace Period
  #endregion
}
