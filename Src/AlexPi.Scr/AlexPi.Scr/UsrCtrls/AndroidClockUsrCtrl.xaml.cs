using AAV.Sys.Helpers;
using AlexPi.Scr;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace UsrCtrlPOCs
{
  public partial class AndroidClockUsrCtrl : UserControl
  {
    readonly Storyboard sbMoveSecondHandAnlg, sbMoveSecondHandImdt;        //static DoubleAnimation _da = new  DoubleAnimation { Duration = new Duration(TimeSpan.FromSeconds(1.0)) };

    public AndroidClockUsrCtrl()
    {
      InitializeComponent();

      sbMoveSecondHandAnlg = FindResource("sbMoveSecondHandAnlg") as Storyboard;
      sbMoveSecondHandImdt = FindResource("sbMoveSecondHandImdt") as Storyboard;

      if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
      {
        new DispatcherTimer(TimeSpan.FromSeconds(.96), DispatcherPriority.Background, new EventHandler((s, e) => setAnglesToNowTime(DateTime.Now)), Dispatcher.CurrentDispatcher); //tu: one-line timer
      }
      else
      {
        IsHeaterOn = false;
        IsHeaterOn = false;
      }

      Loaded += onLoaded;
    }
    void onLoaded(object s, RoutedEventArgs e)
    {
      var idleAt = App.StartedAt.AddSeconds(-App.ScrSvrTimeoutSec);
      StartMin = idleAt.Minute * 6 + idleAt.Second * .1;
      StartHou = idleAt.TimeOfDay.TotalDays * 720;

      ut0.TrgtDate = DateTime.Today;
      ut1.TrgtDate = ut0.TrgtDate.AddDays(-1);
      ut2.TrgtDate = ut1.TrgtDate.AddDays(-1);
      ut3.TrgtDate = ut2.TrgtDate.AddDays(-1);
      ut4.TrgtDate = ut3.TrgtDate.AddDays(-1);
      ut5.TrgtDate = ut4.TrgtDate.AddDays(-1);
      ut6.TrgtDate = ut5.TrgtDate.AddDays(-1);
      ut7.TrgtDate = ut6.TrgtDate.AddDays(-1);
      ut8.TrgtDate = ut7.TrgtDate.AddDays(-1);
      ut9.TrgtDate = ut8.TrgtDate.AddDays(-1);
      utA.TrgtDate = ut9.TrgtDate.AddDays(-1);
      utB.TrgtDate = utA.TrgtDate.AddDays(-1);
      utC.TrgtDate = utB.TrgtDate.AddDays(-1);
    }
    void setAnglesToNowTime(DateTime now)
    {
      S60 = 354 + (S_1 = 6 + (S_0 = /*HandSecAngle.Angle =*/ (now.Second * 6)));
      HandMinAngle.Angle = (now.Minute * 6 + now.Second * .1);
      HandHouAngle.Angle = ((now.Hour - 12) * 30 + now.Minute * .5);

      if (now.Minute % 4 == 0) // (now < App.StartedAt.AddMinutes(2))
        HandSec.BeginStoryboard(sbMoveSecondHandAnlg);
      else
        HandSec.BeginStoryboard(sbMoveSecondHandImdt);

      CurentMM = now.Minute;
      CurntMin = now.Minute * 6 + now.Second * .1 + .1;
      CurntHou = now.TimeOfDay.TotalDays * 720;

#if DEBUG
      Bpr.BeepShort();
#endif
    }
    public static readonly DependencyProperty StartMinProperty = DependencyProperty.Register("StartMin", typeof(double), typeof(AndroidClockUsrCtrl));     /**/ public double StartMin { get => (double)GetValue(StartMinProperty); set => SetValue(StartMinProperty, value); }
    public static readonly DependencyProperty CurntMinProperty = DependencyProperty.Register("CurntMin", typeof(double), typeof(AndroidClockUsrCtrl));     /**/ public double CurntMin { get => (double)GetValue(CurntMinProperty); set => SetValue(CurntMinProperty, value); }
    public static readonly DependencyProperty CurentMMProperty = DependencyProperty.Register("CurentMM", typeof(double), typeof(AndroidClockUsrCtrl));     /**/ public double CurentMM { get => (double)GetValue(CurentMMProperty); set => SetValue(CurentMMProperty, value); }
    public static readonly DependencyProperty StartHouProperty = DependencyProperty.Register("StartHou", typeof(double), typeof(AndroidClockUsrCtrl));     /**/ public double StartHou { get => (double)GetValue(StartHouProperty); set => SetValue(StartHouProperty, value); }
    public static readonly DependencyProperty CurntHouProperty = DependencyProperty.Register("CurntHou", typeof(double), typeof(AndroidClockUsrCtrl));     /**/ public double CurntHou { get => (double)GetValue(CurntHouProperty); set => SetValue(CurntHouProperty, value); }
    public static readonly DependencyProperty SecHandVisProperty = DependencyProperty.Register("SecHandVis", typeof(Visibility), typeof(AndroidClockUsrCtrl), new UIPropertyMetadata(Visibility.Visible)); public Visibility SecHandVis { get => (Visibility)GetValue(SecHandVisProperty); set => SetValue(SecHandVisProperty, value); }
    public static readonly DependencyProperty S_0Property = DependencyProperty.Register("S_0", typeof(double), typeof(AndroidClockUsrCtrl), new UIPropertyMetadata(0d));                     /**/ public double S_0 { get => (double)GetValue(S_0Property); set => SetValue(S_0Property, value); }
    public static readonly DependencyProperty S_1Property = DependencyProperty.Register("S_1", typeof(double), typeof(AndroidClockUsrCtrl), new UIPropertyMetadata(0d));                     /**/ public double S_1 { get => (double)GetValue(S_1Property); set => SetValue(S_1Property, value); }
    public static readonly DependencyProperty S60Property = DependencyProperty.Register("S60", typeof(double), typeof(AndroidClockUsrCtrl), new UIPropertyMetadata(360d));                   /**/ public double S60 { get => (double)GetValue(S60Property); set => SetValue(S60Property, value); }
    public static readonly DependencyProperty DsplayModeProperty = DependencyProperty.Register("DsplayMode", typeof(int), typeof(AndroidClockUsrCtrl), new UIPropertyMetadata(0));           /**/ public int DsplayMode { get => (int)GetValue(DsplayModeProperty); set => SetValue(DsplayModeProperty, value); }
    public static readonly DependencyProperty SecOpacityProperty = DependencyProperty.Register("SecOpacity", typeof(double), typeof(AndroidClockUsrCtrl), new UIPropertyMetadata(1.0d));     /**/ public double SecOpacity { get => (double)GetValue(SecOpacityProperty); set => SetValue(SecOpacityProperty, value); }
    public static readonly DependencyProperty BlurRaduisProperty = DependencyProperty.Register("BlurRaduis", typeof(double), typeof(AndroidClockUsrCtrl), new UIPropertyMetadata(0d));       /**/ public double BlurRaduis { get => (double)GetValue(BlurRaduisProperty); set => SetValue(BlurRaduisProperty, value); }
    public static readonly DependencyProperty IsHeaterOnProperty = DependencyProperty.Register("IsHeaterOn", typeof(bool), typeof(AndroidClockUsrCtrl), new UIPropertyMetadata(false, chd)); /**/ public bool IsHeaterOn { get => (bool)GetValue(IsHeaterOnProperty); set => SetValue(IsHeaterOnProperty, value); }

    void uc1_Unloaded(object s, RoutedEventArgs e)
    {
      App.LogScrSvrUptimeOncePerSession("ScrSvr - Dn - AndroidClockUsrCtrl.uc1_Unloaded(). ");
      sbMoveSecondHandAnlg?.Stop();
      sbMoveSecondHandImdt?.Stop();
    }

    //blic static readonly DependencyProperty IsSecHndOnProperty = DependencyProperty.Register("IsSecHndOn", typeof(bool), typeof(AndroidClockUsrCtrl), new UIPropertyMetadata(true));         /**/public bool IsSecHndOn { get { return (bool)GetValue(IsSecHndOnProperty); } set { SetValue(IsSecHndOnProperty, value); } }

    static void chd(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ////((AndroidClockUsrCtrl)d).IsSecHndOn = ((AndroidClockUsrCtrl)d).IsHeaterOn;
        ((AndroidClockUsrCtrl)d).BlurRaduis = ((AndroidClockUsrCtrl)d).IsHeaterOn ? 20 : 0;////((AndroidClockUsrCtrl)d).SecHandVis = ((AndroidClockUsrCtrl)d).IsHeaterOn ? Visibility.Visible : Visibility.Collapsed;//if (((AndroidClockUsrCtrl)d).IsHeaterOn)//  ((AndroidClockUsrCtrl)d).HandSec.BeginStoryboard(((AndroidClockUsrCtrl)d).sbMoveSecondHandAnlg);//else//  ((AndroidClockUsrCtrl)d).sbMoveSecondHandAnlg.Stop();
  }
  public class BoolToVisibilityConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) { var visibility = (bool)value; return visibility ? Visibility.Visible : Visibility.Collapsed; }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) { var visibility = (Visibility)value; return (visibility == Visibility.Visible); }
  }
}