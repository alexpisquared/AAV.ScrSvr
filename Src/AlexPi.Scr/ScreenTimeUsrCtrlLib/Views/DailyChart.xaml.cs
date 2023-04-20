namespace UpTimeChart;
public partial class DailyChart : UserControl
{
  public struct TimeSplit { public TimeSpan WorkedFor, IdleOrOff, TotalDaysUp; public TimeSpan TtlMinusIdl => TotalDaysUp - IdleOrOff; };
  double _ah = 30, _aw = 30;
  readonly Brush cBlk = new SolidColorBrush(Color.FromRgb(0, 0, 0x28));
  readonly Brush cPnk = new SolidColorBrush(Color.FromRgb(0x30, 0, 0));
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
      while (canvasBar.ActualWidth <= 0) await Task.Delay(1); await Task.Delay(1);        // odd shorter 1at time without this line.
      await ClearDrawAllSegmentsForSinglePC(Environment.MachineName);
    };
  }
  public async Task ClearDrawAllSegmentsForSinglePC(string machineName)
  {
    try
    {
      _ah = canvasBar.ActualHeight;
      _aw = canvasBar.ActualWidth;
      canvasBar.Children.Clear();

      for (var i = .125; i < 1; i += .041666) addRectangle(0, _ah, _aw * i, 1, b1); //addRectangle(0, _ah * .2, _aw * i, 1, b1);
      for (var i = .125; i < 1; i += .125000) addRectangle(0, _ah, _aw * i, 1, b3); //addRectangle(_ah * .2, _ah * .3, _aw * i, 1, b3);
      for (var i = 0.00; i < 1; i += .250000) addRectangle(0, _ah, _aw * i, 1, b6); //addRectangle(_ah * .5, _ah * .5, _aw * i, 1, b6);

      await DrawUpDnLine(TrgDateC);

      Bpr.ShortFaF();
    }
    catch (Exception ex) { ex.Pop(); }
  }
  async Task DrawUpDnLine(DateTime trgDate)
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

      Trace.Write($">>>-\t{_aw} == {canvasBar.ActualWidth} \t");

      var eois = EvLogHelper.GetAllUpDnEvents(timeA, timeB);
      if (eois.Count < 1)
        tbSummary.Text = $"{trgDate,9:ddd M-dd}   n/a";
      else
      {
        if (trgDate == DateTime.Today)
          eois.Add(DateTime.Now, (int)EvOfIntFlag.ShutAndSleepDn);

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

        tbSummary.Text = $"{trgDate,9:ddd M-dd}  {ts.WorkedFor,5:h\\:mm} /{ts.TotalDaysUp,5:h\\:mm} ";
      }

      //tbSummary.Foreground = (trgDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) ? Brushes.LightPink : Brushes.CadetBlue;
      gridvroot.Background = (trgDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) ? cPnk : cBlk;

      if (trgDate >= DateTime.Today)
      {
        var bt = new BackgroundTaskDisposable(TimeSpan.FromMinutes(1), OnTimer);
        await Task.Delay(300);
        //await bt.StopAsync();
      }
    }
    catch (Exception ex) { ex.Pop(); }
    finally { Trace.WriteLine($" ==> {tbSummary.Text} "); }
  }

  void OnTimer() => addRectangle(0, _ah, _aw * DateTime.Now.TimeOfDay.TotalDays, 1, Brushes.Yellow, $"{DateTime.Now.TimeOfDay:h\\:mm\\:ss}"); // now line
  void addRectangle(double top, double hgt, double left, double width, Brush brush, string? tooltip = null) => addUiElnt(top, left, new Rectangle { Width = width, Height = hgt, Fill = brush, ToolTip = tooltip ?? $"thlw: {top:N0}-{hgt:N0}-{left:N0}-{width:N0}." }); //addArcDtl(hgt, left, width);
  [Obsolete]
  void addWkTimeSegment(DateTime timeA, DateTime timeB, EvOfIntFlag eoiA, EvOfIntFlag eoiB, Brush brh, ref TimeSplit ts)
  {
    if (eoiA == EvOfIntFlag.ScreenSaverrUp && eoiB == EvOfIntFlag.BootAndWakeUps) eoiB = EvOfIntFlag.ScreenSaverrUp; // ignore odd pwr-on during scrsvr runs.
    if (eoiA == EvOfIntFlag.BootAndWakeUps && eoiB == EvOfIntFlag.ScreenSaverrDn) eoiA = EvOfIntFlag.ScreenSaverrUp; // ignore odd pwr-on during scrsvr runs.
    if (eoiA == EvOfIntFlag.BootAndWakeUps && eoiB == EvOfIntFlag.BootAndWakeUps) eoiB = EvOfIntFlag.ShutAndSleepDn; // ignore odd pwr-on during scrsvr runs. 2023-04

    add_________Time(timeA, timeB, eoiA, eoiB, ref ts);

    var ta = (eoiA == EvOfIntFlag.ScreenSaverrUp ? timeA.AddSeconds(-Ssto_GpSec).TimeOfDay : timeA.TimeOfDay);
    var tb = (eoiB == EvOfIntFlag.ScreenSaverrUp ? timeB.AddSeconds(-Ssto_GpSec).TimeOfDay : timeB.TimeOfDay);

    var angleA = _aw * ta.TotalDays; // for ss up - start idle line 2 min prior
    var angleB = _aw * tb.TotalDays; // for ss dn - end   work line 2 min prior

    var hgt =
      eoiA == EvOfIntFlag.Day1stAmbiguos ? (_ah / 9) :
      eoiA == EvOfIntFlag.ScreenSaverrUp ? (_ah / 7) :
      eoiA == EvOfIntFlag.ScreenSaverrDn ? (_ah / 1) :
      eoiA == EvOfIntFlag.BootAndWakeUps ? (_ah / 1) :
      eoiA == EvOfIntFlag.Who_Knows_What ? (_ah / 8) : 0;

    var isUp = eoiA is EvOfIntFlag.ScreenSaverrDn or EvOfIntFlag.BootAndWakeUps;
    var top = _ah - hgt;
    var wid = Math.Abs(angleB - angleA);

    var time = TimeSpan.FromDays(wid / _aw);

    var tooltip = $"{(isUp ? $" Work " : $"Idle ")} \n {ta:h\\:mm} - {tb:h\\:mm} = {time:h\\:mm\\:ss} ";

    addRectangle(top, hgt, angleA, wid, brh, tooltip);

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
          FontSize = isUp ? 13 : 11,
          Foreground = isUp ? Brushes.LimeGreen : Brushes.DodgerBlue,
          ToolTip = tooltip,
          Margin = isUp ? new Thickness(3, 2, -3, -2) : new Thickness(3, -2, -3, 2)
        });
    }
  }
  void add_________Time(DateTime timeA, DateTime timeB, EvOfIntFlag eoiA, EvOfIntFlag eoiB, ref TimeSplit ts)
  {
    if (eoiA is EvOfIntFlag.ScreenSaverrDn or EvOfIntFlag.BootAndWakeUps)
      ts.WorkedFor += (timeB - timeA);
    else
      ts.IdleOrOff += (timeB - timeA);
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

  [Obsolete]
  public static int Ssto_GpSec => ScrSvrTimeoutSec + GraceEvLogAndLockPeriodSec;  // ScreenSaveTimeOut + Grace Period
  #endregion
}
