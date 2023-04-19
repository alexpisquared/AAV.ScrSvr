using AAV.Sys.Helpers;

namespace UpTimeChart;

public partial class DailyChart : UserControl
{
  public struct TimeSplit { public TimeSpan WorkedFor, IdleOrOff, TotalDaysUp; public TimeSpan TtlMinusIdl => TotalDaysUp - IdleOrOff; };

  double _ah = 30, _aw = 30;
  readonly Brush cWE = Brushes.LightPink;
  readonly Brush cWD = Brushes.LightSkyBlue;
  readonly Brush cWEd = Brushes.LightPink;
  readonly Brush cWDd = Brushes.CadetBlue;
  readonly Brush cBlk = Brushes.Black;
  readonly Brush cPnk = new SolidColorBrush(Color.FromRgb(0x30, 0, 0));
  Brush _clr = Brushes.DarkGray;
  readonly Brush b1 = new SolidColorBrush(Color.FromRgb(0x2c, 0x2c, 0x2c));
  readonly Brush b3 = new SolidColorBrush(Color.FromRgb(0x3c, 0x3c, 0x3c));
  readonly Brush b6 = new SolidColorBrush(Color.FromRgb(0x70, 0x70, 0x70));

  public DailyChart() : this(DateTime.Now) { }
  public DailyChart(DateTime trg)
  {
    InitializeComponent();

    TrgDateC = trg;

    Loaded += async (s, e) =>
    {
      while (canvasBar.ActualWidth <= 0)        await Task.Delay(1);      await Task.Delay(1);        // odd shorter 1at time without this line.
      ClearDrawAllSegmentsForSinglePC(Environment.MachineName);
    };
  }
  public void clearDrawAllSegmentsForAllPCsAsync(object s, RoutedEventArgs e) { }
  public void ClearDrawAllSegmentsForSinglePC(string machineName)
  {
    try
    {
      _ah = canvasBar.ActualHeight;
      _aw = canvasBar.ActualWidth;
      canvasBar.Children.Clear();

      _clr = (TrgDateC.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) ? cWE : cWD;

      for (var i = .125; i < 1; i += .041666) addRectangle(0, _ah, _aw * i, 1, b1); //addRectangle(0, _ah * .2, _aw * i, 1, b1);
      for (var i = .125; i < 1; i += .125000) addRectangle(0, _ah, _aw * i, 1, b3); //addRectangle(_ah * .2, _ah * .3, _aw * i, 1, b3);
      for (var i = 0.00; i < 1; i += .250000) addRectangle(0, _ah, _aw * i, 1, b6); //addRectangle(_ah * .5, _ah * .5, _aw * i, 1, b6);

      DrawUpDnLine(TrgDateC, machineName);

      if (TrgDateC >= DateTime.Today) addRectangle(0, _ah, _aw * DateTime.Now.TimeOfDay.TotalDays, 1, Brushes.White); // now line
      
      Bpr.ShortFaF();
    }
    catch (Exception ex) { ex.Pop(); }
  }

  void addRectangle(double top, double hgt, double left, double width, Brush brush, string? tooltip = null) => addUiElnt(top, left, new Rectangle { Width = width, Height = hgt, Fill = brush, ToolTip = tooltip ?? $"thlw: {top:N0}-{hgt:N0}-{left:N0}-{width:N0}." }); //addArcDtl(hgt, left, width);

  [Obsolete]
  void DrawUpDnLine(DateTime trgDate, string pc)
  {
    var pcClr = new SolidColorBrush(Color.FromRgb(0x50, 0x60, 0x50));
    var ts = new TimeSplit();
    //..Trace.Write($">>>-\tdrawUpDnLine():  {trgDate:d} ->> {pc,-16} \t");
    tbSummary.Text = "$@#";
    try
    {
      _ah = canvasBar.ActualHeight;
      _aw = canvasBar.ActualWidth;
      var timeA = trgDate;
      var timeB = trgDate.AddDays(.9999999);
      var isHere = Environment.MachineName.Equals(pc, StringComparison.OrdinalIgnoreCase);

      Trace.Write($">>>-\t{_aw} == {canvasBar.ActualWidth} \t");

      SortedList<DateTime, int> eois;
      if (isHere)
      {
        var localEvLog = EvLogHelper.GetAllUpDnEvents(timeA, timeB);
        var dbaseEvLog = DbLogHelper.GetAllUpDnEvents(timeA, timeB, pc);
        eois = localEvLog.Count > dbaseEvLog.Count ? localEvLog : dbaseEvLog; // Jan 2020: whoever has more events wins!
      }
      else
      {
        eois = DbLogHelper.GetAllUpDnEvents(timeA, timeB, pc);
      }

      if (trgDate == DateTime.Today && isHere)
        eois.Add(DateTime.Now, 2); // == ((int)EvOfIntFlag.ScreenSaverrUp)

      if (eois.Count < 1)
        tbSummary.Text = $"{trgDate,16:ddd, MMM dd yyyy}  no activities logged on this date.";
      else
      {
        var eoi0 = eois.FirstOrDefault();
        var prevEoiF = eoi0.Value == (int)EvOfIntFlag.ScreenSaverrDn ? EvOfIntFlag.ScreenSaverrUp :
                       eoi0.Value == (int)EvOfIntFlag.BootAndWakeUps ? EvOfIntFlag.ShutAndSleepDn : EvOfIntFlag.Day1stAmbiguos;

        foreach (var eoi in eois)
        {
          addWkTimeSegment(timeA, eoi.Key, prevEoiF, (EvOfIntFlag)eoi.Value, pcClr, ref ts);

          timeA = eoi.Key;
          prevEoiF = (EvOfIntFlag)eoi.Value;
        }

        var lastScvrUp = (eois.Any(r => r.Value is ((int)EvOfIntFlag.ScreenSaverrUp) or ((int)EvOfIntFlag.ShutAndSleepDn)) ?
                        eois.Where(r => r.Value is ((int)EvOfIntFlag.ScreenSaverrUp) or ((int)EvOfIntFlag.ShutAndSleepDn)).Last() : eois.Last()).Key;

        var finalEvent = eois.Last().Key;

        ts.TotalDaysUp = (lastScvrUp < finalEvent ? lastScvrUp : finalEvent) - eois.First().Key;

        tbSummary.Text = $"{trgDate,16:ddd, MMM dd yyyy}    {ts.TotalDaysUp,5:h\\:mm}  ({ts.WorkedFor:h\\:mm})    ";
      }

      tbSummary.Foreground = (trgDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) ? cWEd : cWDd;
      gridvroot.Background = (trgDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) ? cPnk : cBlk;
    }
    catch (Exception ex) { ex.Pop(); }
    finally { Trace.WriteLine($" ==> {tbSummary.Text} "); }
  }

  void add_________Time(DateTime timeA, DateTime timeB, EvOfIntFlag eoiA, EvOfIntFlag eoiB, ref TimeSplit ts)
  {
    if (eoiA is EvOfIntFlag.ScreenSaverrDn or EvOfIntFlag.BootAndWakeUps)
      ts.WorkedFor += (timeB - timeA);
    else
      ts.IdleOrOff += (timeB - timeA);
  }

  void addWkTimeSegment(DateTime timeA, DateTime timeB, EvOfIntFlag eoiA, EvOfIntFlag eoiB, Brush brh, ref TimeSplit ts)
  {
    if (eoiA == EvOfIntFlag.ScreenSaverrUp && eoiB == EvOfIntFlag.BootAndWakeUps) eoiB = EvOfIntFlag.ScreenSaverrUp; // ignore odd pwr-on during scrsvr runs.
    if (eoiA == EvOfIntFlag.BootAndWakeUps && eoiB == EvOfIntFlag.ScreenSaverrDn) eoiA = EvOfIntFlag.ScreenSaverrUp; // ignore odd pwr-on during scrsvr runs.
    if (eoiA == EvOfIntFlag.BootAndWakeUps && eoiB == EvOfIntFlag.BootAndWakeUps) eoiB = EvOfIntFlag.ShutAndSleepDn; // ignore odd pwr-on during scrsvr runs. 2023-04

    add_________Time(timeA, timeB, eoiA, eoiB, ref ts);

    var angleA = _aw * (eoiA == EvOfIntFlag.ScreenSaverrUp ? timeA.AddSeconds(-Ssto_GpSec).TimeOfDay.TotalDays : timeA.TimeOfDay.TotalDays); // for ss up - start idle line 2 min prior
    var angleB = _aw * (eoiB == EvOfIntFlag.ScreenSaverrUp ? timeB.AddSeconds(-Ssto_GpSec).TimeOfDay.TotalDays : timeB.TimeOfDay.TotalDays); // for ss dn - end   work line 2 min prior

    var hgt =
      eoiA == EvOfIntFlag.Day1stAmbiguos ? (_ah / 9) :
      eoiA == EvOfIntFlag.ScreenSaverrUp ? (_ah / 7) :
      eoiA == EvOfIntFlag.ScreenSaverrDn ? (_ah / 1) :
      eoiA == EvOfIntFlag.BootAndWakeUps ? (_ah / 1) :
      eoiA == EvOfIntFlag.Who_Knows_What ? (_ah / 8) :
      0;

    var isUp = eoiA is EvOfIntFlag.ScreenSaverrDn or EvOfIntFlag.BootAndWakeUps;
    var top = _ah - hgt;
    var wid = Math.Abs(angleB - angleA);

    var time = TimeSpan.FromDays(wid / _aw);

    var tt = isUp ? $"Work {time:h\\:mm\\:ss}." : $"Idle {time:h\\:mm\\:ss}";

    addRectangle(top, hgt, angleA, wid, brh, tt);
    if (wid <= .005 * _aw)
      Debug.Write($" >>> width too small to add time box: {time.TotalSeconds:N2} sec => {wid:N1} pxl");
    else
    {
      var isBig = time > TimeSpan.FromHours(1);
      addUiElnt(
        isUp ? 2 : -1,
        angleB - (isBig ? 28 : 20),
        new TextBlock
        {
          Text = isBig ? $"{time:h\\:mm}" : $"{time,3:\\:m}",
          FontSize = isUp ? 14 : 12,
          Foreground = isUp ? Brushes.LimeGreen : Brushes.DodgerBlue,
          ToolTip = tt,
          Margin = new Thickness(3,0,-3,0)
        });
    }
  }

  void addArcDtl(double hgt, double left, double width)
  {
    var k = 720 / _aw;

    var arc = new Arc
    {
      StartAngle = left * k,
      EndAngle = (left + width) * k,
      //StrokeThickness = hgt,
      //Stretch = Stretch.None,
      //Stroke = brush,
      //RenderTransformOrigin = new Point(0.5, 0.5)
    };

    addUiElnt(hgt, left, arc);
  }

  void addUiElnt(double top, double left, UIElement el)
  {
    Canvas.SetLeft(el, left);
    Canvas.SetTop(el, top);
    _ = canvasBar.Children.Add(el);
  }
  public static readonly DependencyProperty TrgDateCProperty = DependencyProperty.Register("TrgDateC", typeof(DateTime), typeof(DailyChart)); public DateTime TrgDateC { get => (DateTime)GetValue(TrgDateCProperty); set => SetValue(TrgDateCProperty, value); }

  #region DUPE_FROM  C:\C\Lgc\ScrSvrs\AlexPi.Scr\App.xaml.cs
  const int GraceEvLogAndLockPeriodSec = 60;
  static int _ssto = -1;

  [Obsolete]
  public static int ScrSvrTimeoutSec
  {
    get
    {
      if (_ssto == -1)
      {
        _ssto = EvLogHelper.GetSstoFromRegistry;
      }

      return _ssto;
    }
  }
  public static int Ssto_GpSec => ScrSvrTimeoutSec + GraceEvLogAndLockPeriodSec;  // ScreenSaveTimeOut + Grace Period
  #endregion
}
