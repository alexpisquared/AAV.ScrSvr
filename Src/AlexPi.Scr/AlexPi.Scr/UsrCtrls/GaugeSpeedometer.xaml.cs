﻿using System.Windows.Media.Animation;

namespace AlexPi.Scr.UsrCtrls;

public partial class GaugeSpeedometer
{
  const double _ang0 = -130, // 150 ~ 130. 150 = 3.75
      _k = 226.0 / 130,
      _wkDayHrs = 8.5,
      _ttlWkExpHr = _wkDayHrs * 5,
      _wkStartsAtHr = 8.25;
  static readonly DoubleAnimation dai, dao, dam;
  static Duration dr = new(TimeSpan.FromSeconds(AppSettings.AnimDurnInSec * 1.0));
  static readonly TimeSpan bt = TimeSpan.FromSeconds(0);
  readonly DateTime _start = App.StartedAt, _idleAt = App.StartedAt.AddSeconds(-App.IdleTimeoutSec);
  async void onLoaded(object s, RoutedEventArgs e)
  {
    try
    {
      await Task.Delay(3000);
      var now = DateTime.Now;
      var today = DateTime.Today;
      var lastSunday = today.AddDays(-(int)today.DayOfWeek);

      var daySpans = new double[(int)today.DayOfWeek + 1];
      var day = 0;
      for (var d = lastSunday; d <= today; d = d.AddDays(1)) daySpans[day++] = await EvLogHelper.GetWkSpanForTheDay(d);

      var weekSpans = daySpans.Sum(r => r);
      var expected = now.Hour >= _wkStartsAtHr + _wkDayHrs ?
          (today - lastSunday).TotalDays * _wkDayHrs :
          ((today - lastSunday).TotalDays - 1) * _wkDayHrs + ((now - today).TotalHours - _wkStartsAtHr);

      var prc = (100.0 * weekSpans / _ttlWkExpHr);
      var leftHrs = _ttlWkExpHr - weekSpans;
      var ova = (100.0 * expected / _ttlWkExpHr);
      var iva = (100.0 * (day - 1) * _wkDayHrs / _ttlWkExpHr);

      MiddlValAnim = prc * _k + _ang0;
      OuterValAnim = ova * _k + _ang0;
      InnerValAnim = iva * _k + _ang0; // < target % for the EODay

      tbTitleOu.Text = $"  Exp: {ova:N1}% ({OuterValAnim:N0}°)";
      tbTitleIn.Text = $"  EOD: {iva:N1}% ({InnerValAnim:N0}°)";

      GaugeText = leftHrs < _wkDayHrs
        ? $"{weekSpans:N1} / {_ttlWkExpHr:N1}\r\n@ {now.AddHours(leftHrs):HH:mm}"
        : $"{weekSpans:N1} / {_ttlWkExpHr:N1}\r\n{leftHrs:N1} hr";
    }
    catch (Exception ex) { ex.Pop(); }//             catch (Exception ex) {  MessageBox.Show(ex.Message, System.Reflection.MethodInfo.GetCurrentMethod().ToString()); } // 
  }

  public GaugeSpeedometer() { InitializeComponent(); Loaded += onLoaded; }
  static GaugeSpeedometer()
  {
    dai = new DoubleAnimation { Duration = dr, EasingFunction = new QuarticEase() };
    dao = new DoubleAnimation { Duration = dr, EasingFunction = new QuarticEase() };
    dam = new DoubleAnimation { BeginTime = bt, Duration = dr, EasingFunction = new ElasticEase { Oscillations = 2 } };
    Timeline.SetDesiredFrameRate(dai, 20);
    Timeline.SetDesiredFrameRate(dao, 20);
    Timeline.SetDesiredFrameRate(dam, 20);
  }

  static void onInnerValChngd(DependencyObject d, DependencyPropertyChangedEventArgs e) { dai.From = (double)e.OldValue; dai.To = (double)e.NewValue; (d as GaugeSpeedometer).BeginAnimation(InnerValProperty, dai); }
  static void onOuterValChngd(DependencyObject d, DependencyPropertyChangedEventArgs e) { dao.From = (double)e.OldValue; dao.To = (double)e.NewValue; (d as GaugeSpeedometer).BeginAnimation(OuterValProperty, dao); }
  static void onMiddlValChngd(DependencyObject d, DependencyPropertyChangedEventArgs e) { dam.From = (double)e.OldValue; dam.To = (double)e.NewValue; (d as GaugeSpeedometer).BeginAnimation(MiddlValProperty, dam); }

  public static readonly DependencyProperty GaugeTextProperty = DependencyProperty.Register("GaugeText", typeof(string), typeof(GaugeSpeedometer), new UIPropertyMetadata("...")); public string GaugeText { get => (string)GetValue(GaugeTextProperty); set => SetValue(GaugeTextProperty, value); }
  public static readonly DependencyProperty GaugeNameProperty = DependencyProperty.Register("GaugeName", typeof(string), typeof(GaugeSpeedometer), new UIPropertyMetadata("...")); public string GaugeName { get => (string)GetValue(GaugeNameProperty); set => SetValue(GaugeNameProperty, value); }
  public static readonly DependencyProperty MiddlValProperty = DependencyProperty.Register("MiddlVal", typeof(double), typeof(GaugeSpeedometer), new UIPropertyMetadata(_ang0)); public double MiddlVal { get => (double)GetValue(MiddlValProperty); set => SetValue(MiddlValProperty, value); }
  public static readonly DependencyProperty OuterValProperty = DependencyProperty.Register("OuterVal", typeof(double), typeof(GaugeSpeedometer), new UIPropertyMetadata(_ang0)); public double OuterVal { get => (double)GetValue(OuterValProperty); set => SetValue(OuterValProperty, value); }
  public static readonly DependencyProperty InnerValProperty = DependencyProperty.Register("InnerVal", typeof(double), typeof(GaugeSpeedometer), new UIPropertyMetadata(_ang0)); public double InnerVal { get => (double)GetValue(InnerValProperty); set => SetValue(InnerValProperty, value); }
  public static readonly DependencyProperty MiddlValAnimProperty = DependencyProperty.Register("MiddlValAnim", typeof(double), typeof(GaugeSpeedometer), new UIPropertyMetadata(_ang0, new PropertyChangedCallback(onMiddlValChngd))); public double MiddlValAnim { get => (double)GetValue(MiddlValAnimProperty); set => SetValue(MiddlValAnimProperty, value); }
  public static readonly DependencyProperty OuterValAnimProperty = DependencyProperty.Register("OuterValAnim", typeof(double), typeof(GaugeSpeedometer), new UIPropertyMetadata(_ang0, new PropertyChangedCallback(onOuterValChngd))); public double OuterValAnim { get => (double)GetValue(OuterValAnimProperty); set => SetValue(OuterValAnimProperty, value); }
  public static readonly DependencyProperty InnerValAnimProperty = DependencyProperty.Register("InnerValAnim", typeof(double), typeof(GaugeSpeedometer), new UIPropertyMetadata(_ang0, new PropertyChangedCallback(onInnerValChngd))); public double InnerValAnim { get => (double)GetValue(InnerValAnimProperty); set => SetValue(InnerValAnimProperty, value); }
}
