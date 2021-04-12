using AlexPi.Scr.Logic;
using Microsoft.Expression.Shapes;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Db.EventLog.Main;
using AsLink;

namespace AlexPi.Scr.UsrCtrls
{
  public partial class UpTimeSpiralUC : UserControl
  {
    int _drawDelayMsPerDay = 500;
    double _aw, _ah;
    readonly DateTime _start = App.StartedAt;
    readonly DateTime _idleAt = App.StartedAt.AddSeconds(-App.ScrSvrTimeoutSec);
    readonly TimeSplit ts = new TimeSplit();
    readonly Brush _brhDodgerBlue = new SolidColorBrush(Color.FromArgb(64, 255, 255, 255));// Brushes.Black, cWE = Brushes.LightPink, cWD = Brushes.LightSkyBlue, cWEd = Brushes.DarkRed, cWDd = Brushes.CadetBlue, b2 = Brushes.Gray;
    public static readonly DependencyProperty StartMinProperty = DependencyProperty.Register("StartMin", typeof(double), typeof(UpTimeSpiralUC)); public double StartMin { get => (double)GetValue(StartMinProperty); set => SetValue(StartMinProperty, value); }
    public static readonly DependencyProperty CurntMinProperty = DependencyProperty.Register("CurntMin", typeof(double), typeof(UpTimeSpiralUC)); public double CurntMin { get => (double)GetValue(CurntMinProperty); set => SetValue(CurntMinProperty, value); }
    public static readonly DependencyProperty StartHouProperty = DependencyProperty.Register("StartHou", typeof(double), typeof(UpTimeSpiralUC)); public double StartHou { get => (double)GetValue(StartHouProperty); set => SetValue(StartHouProperty, value); }
    public static readonly DependencyProperty CurntHouProperty = DependencyProperty.Register("CurntHou", typeof(double), typeof(UpTimeSpiralUC)); public double CurntHou { get => (double)GetValue(CurntHouProperty); set => SetValue(CurntHouProperty, value); }
    public static readonly DependencyProperty UpTimeTxProperty = DependencyProperty.Register("UpTimeTx", typeof(string), typeof(UpTimeSpiralUC)); public string UpTimeTx { get => (string)GetValue(UpTimeTxProperty); set => SetValue(UpTimeTxProperty, value); }
    public static readonly DependencyProperty UpTimePcProperty = DependencyProperty.Register("UpTimePc", typeof(string), typeof(UpTimeSpiralUC)); public string UpTimePc { get => (string)GetValue(UpTimePcProperty); set => SetValue(UpTimePcProperty, value); }
    public static readonly DependencyProperty TrgDateSProperty = DependencyProperty.Register("TrgDateS", typeof(string), typeof(UpTimeSpiralUC)); public string TrgDateS { get => (string)GetValue(TrgDateSProperty); set => SetValue(TrgDateSProperty, value); }
    public static readonly DependencyProperty TrgtDateProperty = DependencyProperty.Register("TrgtDate", typeof(DateTime), typeof(UpTimeSpiralUC)); public DateTime TrgtDate { get => (DateTime)GetValue(TrgtDateProperty); set => SetValue(TrgtDateProperty, value); }

    public UpTimeSpiralUC() { InitializeComponent(); DataContext = this; Loaded += onLoaded; }

    async void onLoaded(object s, RoutedEventArgs e)
    {
      var args = Environment.GetCommandLineArgs();
      _drawDelayMsPerDay = args.Length > 1 && args[1].ToLower().Contains("u") ? 500 : 10000;

      if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
        await Task.Delay(((int)(DateTime.Today - TrgtDate).TotalDays + 2) * _drawDelayMsPerDay); // slow down drawing to keep responsive.

      drawUpTimeSegments(TrgtDate);

      StartMin = _idleAt.Minute * 6 + _idleAt.Second * .1;
      StartHou = _idleAt.TimeOfDay.TotalDays * 720d;

      if (DateTime.Today == TrgtDate) new DispatcherTimer(TimeSpan.FromSeconds(10), DispatcherPriority.Background, new EventHandler((s_, e_) => updateTimes(DateTime.Now)), Dispatcher.CurrentDispatcher); //tu: one-line timer
    }



    void updateTimes(DateTime now) => UpTimeTx = $"  {ts.TotalDaysUp.Add(now - _start),5:h\\:mm}     {ts.WorkedFor:h\\:mm} · "; // UpTimePc = $"{(ts.TotalDaysUp.TotalHours <= 0 ? 0 : 100.0 * ts.UsefulTime.TotalHours / (ts.TotalDaysUp.TotalHours + (now - _start).TotalHours)),2:N0} %"; //todo: this calc-n is wrong ...and crowds the screen => suspended till further notice (Aug2019)

    void drawUpTimeSegments(DateTime trgDate)
    {
      grd1.Children.Clear();
      _ah = grd1.ActualHeight;
      _aw = grd1.ActualWidth;
      var timeA = trgDate;
      var timeB = trgDate.AddDays(.9999999);

      Task.Run(() => EvLogHelper.GetAllUpDnEvents(timeA, timeB)).ContinueWith(_ =>
      {
        var eois = _.Result;
        if (eois.Count < 1)
          Debug.WriteLine($"-->{trgDate:MMM-dd, ddd}:   No events registered.");
        else
        {
          var eoi0 = eois.FirstOrDefault();
          var prevEoI = eoi0;
          var prevEoiF = eoi0.Value == (int)EvOfIntFlag.ScreenSaverrDn ? (int)EvOfIntFlag.ScreenSaverrUp :
                         eoi0.Value == (int)EvOfIntFlag.BootAndWakeUps ? (int)EvOfIntFlag.ShutAndSleepDn : (int)EvOfIntFlag.Day1stAmbiguos;

          foreach (var eoi in eois)
          {
            addWkTimeSegment(timeA, eoi.Key, (EvOfIntFlag)prevEoiF, (EvOfIntFlag)eoi.Value, _brhDodgerBlue);

            //if (prevEoiF == (int)EvOfIntFlag.ScreenSaverrUp)            {              Debug.WriteLine($"--- {eoi.Key} - {App.Ssto_GpSec} = {eoi.Key.AddSeconds(-App.Ssto_GpSec)}");            }
            timeA = eoi.Key;
            prevEoiF = eoi.Value;
            prevEoI = eoi;
          }

          if (timeB > _idleAt) // if today
          {
            timeB = _idleAt;
            ts.TotalDaysUp = timeB - eois.First().Key;
            if (prevEoiF == (int)EvOfIntFlag.BootAndWakeUps) // this last segment
              ts.WorkedFor += (timeB - timeA);
          }
          else if (prevEoiF == (int)EvOfIntFlag.ScreenSaverrDn || prevEoiF == (int)EvOfIntFlag.BootAndWakeUps)
            prevEoiF = (int)EvOfIntFlag.Day1stAmbiguos;

          if (ts.TotalDaysUp == TimeSpan.Zero)
            ts.TotalDaysUp = eois.Last().Key - eois.First().Key;

          addWkTimeSegment(timeA, timeB, (EvOfIntFlag)prevEoiF, EvOfIntFlag.Who_Knows_What, _brhDodgerBlue);

          UpTimeTx = $"  {ts.TotalDaysUp,5:h\\:mm}     {ts.WorkedFor:h\\:mm}    {trgDate:ddd}";

          //Debug.WriteLine($"{DateTime.Now:HH:mm:ss}  ■ ■  UpTimeTx: {UpTimeTx}    UpTimePc:{UpTimePc}");
        }
      }, TaskScheduler.FromCurrentSynchronizationContext());
    }
    void add_________Time(DateTime timeA, DateTime timeB, Db.EventLog.Main.EvOfIntFlag eoiA, EvOfIntFlag eoiB)
    {
      if (eoiA == EvOfIntFlag.ScreenSaverrDn || eoiA == EvOfIntFlag.BootAndWakeUps)
        ts.WorkedFor += (timeB - timeA);
      else
        ts.IdleOrOff += (timeB - timeA);
    }

    void addWkTimeSegment(DateTime timeA, DateTime timeB, EvOfIntFlag eoiA, EvOfIntFlag eoiB, Brush brh)
    {
      if (eoiA == EvOfIntFlag.ScreenSaverrUp && eoiB == EvOfIntFlag.BootAndWakeUps) eoiB = EvOfIntFlag.ScreenSaverrUp; // ignore odd pwr-on during scrsvr runs.
      if (eoiA == EvOfIntFlag.BootAndWakeUps && eoiB == EvOfIntFlag.ScreenSaverrDn) eoiA = EvOfIntFlag.ScreenSaverrUp; // ignore odd pwr-on during scrsvr runs.

      add_________Time(timeA, timeB, eoiA, eoiB);

      var angleA = _aw * (eoiA == EvOfIntFlag.ScreenSaverrUp ? timeA.AddSeconds(-App.Ssto_GpSec).TimeOfDay.TotalDays : timeA.TimeOfDay.TotalDays); // for ss up - start idle line 2 min prior
      var angleB = _aw * (eoiB == EvOfIntFlag.ScreenSaverrUp ? timeB.AddSeconds(-App.Ssto_GpSec).TimeOfDay.TotalDays : timeB.TimeOfDay.TotalDays); // for ss dn - end   work line 2 min prior

      var hgt =
      eoiA == EvOfIntFlag.Day1stAmbiguos ? 0 :
      eoiA == EvOfIntFlag.ScreenSaverrUp ? 1 :
      eoiA == EvOfIntFlag.ScreenSaverrDn ? 9 :
      eoiA == EvOfIntFlag.BootAndWakeUps ? 9 : 0;

      var k = 720 / _aw;

      if (angleA > angleB)
      {
        var tmp = angleB;
        angleB = angleA;
        angleA = tmp;
      }

      var noon = 150;
      var smll = .925;
      var sgmt = new Arc
      {
        //Opacity = .5,
        StartAngle = angleA * k,
        EndAngle = angleB * k,
        Height = angleA < noon ? _ah * smll : _ah,
        Width = angleA < noon ? _aw * smll : _aw,
        Stroke = brh,
        StrokeThickness = hgt,
        Stretch = Stretch.None, //tu: !!!
      };

      addArc(hgt, angleA, sgmt);
    }
    void addArc(double top, double left, UIElement el)
    {
      Canvas.SetLeft(el, left);
      Canvas.SetTop(el, top);
      grd1.Children.Add(el);
    }
  }
}