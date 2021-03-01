using AAV.WPF.Ext;
using AsLink;
using GaugeUserControlLibrary;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace AlexPi.Scr.UsrCtrls
{
  public partial class GaugeTor : UserControl
  {
    static readonly DoubleAnimation _dai, _dao, _dam;
    static GaugeTor()
    {
      Duration
        drnFast = new Duration(TimeSpan.FromSeconds(AppSettings.AnimDurnInSec * 1.0)),
        drnSlow = new Duration(TimeSpan.FromSeconds(5));

      _dai = new DoubleAnimation { Duration = drnSlow, EasingFunction = new QuarticEase() };
      _dao = new DoubleAnimation { Duration = drnSlow, EasingFunction = new QuarticEase() };
      _dam = new DoubleAnimation { Duration = drnFast, EasingFunction = new ElasticEase { Oscillations = 3 } };
      Timeline.SetDesiredFrameRate(_dai, 20);
      Timeline.SetDesiredFrameRate(_dao, 20);
      Timeline.SetDesiredFrameRate(_dam, 20);
    }
    public GaugeTor() => InitializeComponent();

    public CpuHistogram CpuHistogram1 => cpuHistogram1;

    static void onInnerValChngd(DependencyObject d, DependencyPropertyChangedEventArgs e) { _dai.From = (double)e.OldValue; _dai.To = (double)e.NewValue; (d as GaugeTor).BeginAnimation(InnerValProperty, _dai); var avg = (((double)e.NewValue + 90) / 1.8); (d as GaugeTor).cpuHistogram1.Avg = avg; (d as GaugeTor).tbTitleIn.Text = $"Avg{avg:N1}"; }
    static void onOuterValChngd(DependencyObject d, DependencyPropertyChangedEventArgs e) { _dao.From = (double)e.OldValue; _dao.To = (double)e.NewValue; (d as GaugeTor).BeginAnimation(OuterValProperty, _dao); (d as GaugeTor).tbTitleOu.Text = $"Max{((double)e.NewValue + 90) / 1.8:N0}"; }
    static void onMiddlValChngd(DependencyObject d, DependencyPropertyChangedEventArgs e) { _dam.From = (double)e.OldValue; _dam.To = (double)e.NewValue; (d as GaugeTor).BeginAnimation(MiddlValProperty, _dam); (d as GaugeTor).CpuHistogram1.Measure_Cpu((float)(((double)e.NewValue + 90) / 1.8)); }

    public static readonly DependencyProperty GaugeTextProperty = DependencyProperty.Register("GaugeText", typeof(string), typeof(GaugeTor), new UIPropertyMetadata("...")); public string GaugeText { get => (string)GetValue(GaugeTextProperty); set => SetValue(GaugeTextProperty, value); }
    public static readonly DependencyProperty GaugeNameProperty = DependencyProperty.Register("GaugeName", typeof(string), typeof(GaugeTor), new UIPropertyMetadata("...")); public string GaugeName { get => (string)GetValue(GaugeNameProperty); set => SetValue(GaugeNameProperty, value); }
    public static readonly DependencyProperty MiddlValProperty = DependencyProperty.Register("MiddlVal", typeof(double), typeof(GaugeTor), new UIPropertyMetadata(-90.0)); public double MiddlVal { get => (double)GetValue(MiddlValProperty); set => SetValue(MiddlValProperty, value); }
    public static readonly DependencyProperty OuterValProperty = DependencyProperty.Register("OuterVal", typeof(double), typeof(GaugeTor), new UIPropertyMetadata(-90.0)); public double OuterVal { get => (double)GetValue(OuterValProperty); set => SetValue(OuterValProperty, value); }
    public static readonly DependencyProperty InnerValProperty = DependencyProperty.Register("InnerVal", typeof(double), typeof(GaugeTor), new UIPropertyMetadata(-90.0)); public double InnerVal { get => (double)GetValue(InnerValProperty); set => SetValue(InnerValProperty, value); }
    public static readonly DependencyProperty MiddlValAnimProperty = DependencyProperty.Register("MiddlValAnim", typeof(double), typeof(GaugeTor), new UIPropertyMetadata(-90.0, new PropertyChangedCallback(onMiddlValChngd))); public double MiddlValAnim { get => (double)GetValue(MiddlValAnimProperty); set => SetValue(MiddlValAnimProperty, value); }
    public static readonly DependencyProperty OuterValAnimProperty = DependencyProperty.Register("OuterValAnim", typeof(double), typeof(GaugeTor), new UIPropertyMetadata(-90.0, new PropertyChangedCallback(onOuterValChngd))); public double OuterValAnim { get => (double)GetValue(OuterValAnimProperty); set => SetValue(OuterValAnimProperty, value); }

    void Button_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Process.Start(new ProcessStartInfo("TaskMgr.exe") { UseShellExecute = true });
      }
      catch (Exception ex) { ex.Pop(); }
    }
    public static readonly DependencyProperty InnerValAnimProperty = DependencyProperty.Register("InnerValAnim", typeof(double), typeof(GaugeTor), new UIPropertyMetadata(-90.0, new PropertyChangedCallback(onInnerValChngd))); public double InnerValAnim { get => (double)GetValue(InnerValAnimProperty); set => SetValue(InnerValAnimProperty, value); }
  }
}
