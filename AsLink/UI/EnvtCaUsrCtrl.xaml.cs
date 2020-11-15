using XSD.CLS;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Documents;
using AsLink;
using AAV.Sys.Ext;

namespace ToRunOr.WPF.UCs
{
  public partial class EnvtCaUsrCtrl : UserControl
  {
    string[] _sites = new[] { "s0000458_e"/*toronto pearson*/}; //   "s0000785_e"/*toronto island*/        }; //         "s0000773_e",/*richmond hill*/   };       // May 2020: localized to the most informative (with extremums).
    const int _step = 10, _iconHght = 5;
    public EnvtCaUsrCtrl()
    {
      InitializeComponent();
      DataContext = this;
      Loaded += (s, e) => onLoaded(s, null); //?????? was PreviewKeyUp += (s, e) => onLoaded(s, null);

      XFormatter_ProperWay = value => DateTime.FromOADate(value).ToString("H:mm"); //or on per series basis: //cChartFore24.AxisX.FirstOrDefault().LabelFormatter = value => DateTime.FromOADate(value).ToString("H:mm");

      WkYFormatter = value => value.ToString("C"); //const int wk = 14;  WkLables = new string[wk];   for (int i = 0; i < wk; i++) WkLables[i] = DateTime.Now.AddDays(i * .5 + 1.5).DayOfWeek.ToString().Substring(0, 3);
    }
    public SeriesCollection SeriesCollection { get; set; }
    public string[] WkLables { get; set; }
    public Func<double, string> WkYFormatter { get; set; }

    async void onLoaded(object s, RoutedEventArgs e)
    {
      //wb1.NavigateToString((await WebSerialHelper.UrlToInstnace<feed>("http://weather.gc.ca/rss/city/on-143_e.xml")).entry[1].summary.Value);
      try
      {
        tbTimePlce.Text /*= tbConditns.Text */= $"Loading {_sites[NextIndex]} ...";
        hLnk1.NavigateUri = new Uri($"http://dd.weatheroffice.ec.gc.ca/citypage_weather/xml/ON/{_sites[CrntIndex]}.xml");

        var sitedata = await WebSerialHelper.UrlToInstnace<siteData>(hLnk1.NavigateUri.AbsoluteUri); // toronpo 458_e.xml"); markham:s0000585, vaughan:s0000584, RichmondHill:s0000773 (from https://saratoga-weather.org/wxtemplates/Canada/ec-forecast-lookup.txt)

        WkLables = sitedata.forecastGroup.forecast.Select(f => f.period.Value.ToString().Substring(0, 2) + (f.period.Value.ToString().Length > 8 ? "-n" : "")).Skip(2).ToArray();

        var hyperlink = new Hyperlink(new Run(sitedata?.currentConditions?.station?.Value)) { NavigateUri = hLnk1.NavigateUri, FontWeight = FontWeights.SemiBold, Foreground = Brushes.White };
        hyperlink.RequestNavigate += onGoTo;

        tbTimePlce.Inlines.Clear();
        tbTimePlce.Inlines.Add(hyperlink);
        tbTimePlce.Inlines.Add(new Run($"\r\n\n{sitedata?.location?.region?.Value}") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Italic, Foreground = Brushes.White }); //tbTimePlce.Inlines.Add(new Run($"\r\n{sitedata?.currentConditions?.dateTime[0]?.timeStamp?.GetDateTimeLcl():MMM-dd H:mm}") { Foreground = Brushes.White });
        tbTimePlce.Inlines.Add(new Run($"\r\n\n{sitedata?.currentConditions?.condition}") { FontWeight = FontWeights.Bold, FontStyle = FontStyles.Normal, Foreground = Brushes.White, FontSize = 24 });

        tbMeasurAt.Text = $"{sitedata?.currentConditions?.dateTime[0]?.timeStamp?.GetDateTimeLcl():H:mm}";

        tbTempReal.Text = $"{sitedata?.currentConditions?.temperature?.Value.GetDecimal():+#.#;-#.#;0}°";
        tbTempFeel.Text = $"{(sitedata?.currentConditions?.humidex?.Value ?? sitedata?.currentConditions?.windChill?.Value ?? sitedata?.currentConditions?.temperature?.Value).GetDecimal():+#;-#;0}°"; // tbConditns.Text = $"{sitedata?.currentConditions?.condition}";
        img1.Source = new BitmapImage(new Uri($"https://weather.gc.ca/weathericons/{(sitedata?.currentConditions?.iconCode?.Value ?? "5"):0#}.gif"));

        var nhr = DateTime.Now.Hour;
        XMin = (decimal)(DateTime.Today.AddHours(nhr - nhr % 12).ToOADate()); // sd.currentConditions.dateTime[0].timeStamp.GetDateTimeLcl().ToOADate() - .04;
        XMax = (decimal)(sitedata.hourlyForecastGroup.hourlyForecast.Max(r => ulong.Parse(r.dateTimeUTC))).GetDateTimeLcl().ToOADate(); //DateTime.Today.AddHours(nhr - nhr % 6 + 30).ToOADate(); // 

        YMax = Math.Max(10, Math.Max(
            sitedata.almanac.temperature[0].Value.GetDecimal() + 1,
            sitedata.hourlyForecastGroup?.hourlyForecast?.Max(r => r?.humidex?.Value.GetDecimal()) ?? 0 + _iconHght));

        var minTemp = Math.Min(
          sitedata.hourlyForecastGroup?.hourlyForecast?.Min(r => r.temperature?.Value.GetDecimal()) ?? 0m,
          sitedata.hourlyForecastGroup?.hourlyForecast?.Where(r => r.windChill?.Value.GetDecimal() > -123.456m).Min(r => r.windChill?.Value.GetDecimal()) ?? 0m);
        var min2 = Math.Min(
          sitedata.almanac.temperature[1].Value.GetDecimal(),
          minTemp)// - 9) // 9 for icons which can go above hist.extr.
          - 1;
        YMin = min2 - _step - min2 % _step; // rounding to the nearest 10 + minus room for icons.

        var RedBlueOnlyGrad = FindResource("RedBlueOnlyGrad") as Brush;
        var RedTransBluGrad = FindResource("RedTransBluGrad") as Brush;
        var DodgerBlue0Grad = FindResource("DodgerBlue0Grad") as Brush;

        cChartFore24.LoadDataToChart_24hr(sitedata, RedBlueOnlyGrad, RedTransBluGrad, DodgerBlue0Grad, YMin);
        cChartForeXX.LoadDataToChart_Week(sitedata, RedBlueOnlyGrad, RedTransBluGrad, DodgerBlue0Grad);
        cChart2.LoadDataToChart_LofP(sitedata);
        tbExn.Text = "";
      }
      catch (Exception ex) { Debug.WriteLine(ex); if (Debugger.IsAttached) Debugger.Break(); tbExn.Text = ex.InnermostMessage(); }
    }
    void onGoTo(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) => Process.Start(e.Uri.AbsoluteUri);
    void onMouseUp(object s, System.Windows.Input.MouseButtonEventArgs e) => onLoaded(s, null);

    public Func<double, string> XFormatter_ProperWay { get; set; }
    public static readonly DependencyProperty XMinProperty = DependencyProperty.Register("XMin", typeof(decimal), typeof(EnvtCaUsrCtrl), new PropertyMetadata(0m)); public decimal XMin { get { return (decimal)GetValue(XMinProperty); } set { SetValue(XMinProperty, value); } }
    public static readonly DependencyProperty XMaxProperty = DependencyProperty.Register("XMax", typeof(decimal), typeof(EnvtCaUsrCtrl), new PropertyMetadata(1m)); public decimal XMax { get { return (decimal)GetValue(XMaxProperty); } set { SetValue(XMaxProperty, value); } }
    public static readonly DependencyProperty YMaxProperty = DependencyProperty.Register("YMax", typeof(decimal), typeof(EnvtCaUsrCtrl), new PropertyMetadata(1m)); public decimal YMax { get { return (decimal)GetValue(YMaxProperty); } set { SetValue(YMaxProperty, value); } }
    public static readonly DependencyProperty YMinProperty = DependencyProperty.Register("YMin", typeof(decimal), typeof(EnvtCaUsrCtrl), new PropertyMetadata(0m)); public decimal YMin { get { return (decimal)GetValue(YMinProperty); } set { SetValue(YMinProperty, value); } }

    int _crntIndex = -1; // start from 0.
    public int NextIndex { get => (++_crntIndex) % _sites.Length; }
    public int CrntIndex { get => (_crntIndex) % _sites.Length; }
  }
  public static class CartesianChartExt
  {
    public static void LoadDataToChart_24hr(this CartesianChart cc, siteData sd, Brush RedBlueOnlyGrad, Brush RedTransBluGrad, Brush dodgerBlue0Grad, decimal YMin)
    {
      byte nrm = 64, ext = 0, xff = 128, ngt = 0, day = 255;
      Brush
          daytClr = new SolidColorBrush(Color.FromArgb(32, day, day, day)),
          nghtClr = new SolidColorBrush(Color.FromArgb(96, ngt, ngt, ngt)),
          drkgrey = new SolidColorBrush(Color.FromRgb(nrm, nrm, nrm)),
          extrMax = new SolidColorBrush(Color.FromRgb(xff, ext, ext)),
          normMin = new SolidColorBrush(Color.FromRgb(nrm, nrm, xff));
      try
      {
        var t00 = sd.hourlyForecastGroup.hourlyForecast.Min(r => r.dateTimeUTC).GetDateTimeLcl();
        var t24 = sd.hourlyForecastGroup.hourlyForecast.Max(r => r.dateTimeUTC).GetDateTimeLcl();
        var obs = sd.currentConditions.dateTime[0].timeStamp.GetDateTimeLcl();
        var now = DateTime.Now;
        var yst = obs.AddHours(-6);
        var x00 = t00.AddHours(-12);
        var xZZ = t24.AddHours(+12);
        var sunrz1 = sd.riseSet.dateTime.FirstOrDefault(r => r.zone == validTimeZones.UTC && r.name == dateStampNameType.sunrise).timeStamp.GetDateTimeLcl();
        var sunst1 = sd.riseSet.dateTime.FirstOrDefault(r => r.zone == validTimeZones.UTC && r.name == dateStampNameType.sunset).timeStamp.GetDateTimeLcl();
        var sunrz0 = sunrz1.AddDays(-1);
        var sunst0 = sunst1.AddDays(-1);
        var sunrz2 = sunrz1.AddDays(1);
        var sunst2 = sunst1.AddDays(1);
        var sunrz3 = sunrz1.AddDays(2);
        var sunst3 = sunst1.AddDays(2);

        var extremMax = sd.almanac.temperature[0].Value.GetDecimal();
        var normalMax = sd.almanac.temperature[2].Value.GetDecimal();
        var normalMin = sd.almanac.temperature[3].Value.GetDecimal();
        var extremMin = sd.almanac.temperature[1].Value.GetDecimal();

        //always throws no matter what:
        //try { if (cc.AxisY?.First()?.Sections != null) cc.AxisY?.First()?.Sections.Clear(); } catch (Exception ex) { Debug.WriteLine(ex); if (Debugger.IsAttached) Debugger.Break(); else Debug.WriteLine("Still throws...."); }

        cc.AxisY.First().Sections = new SectionsCollection
        {
          new AxisSection{Value=0, SectionWidth = 10, Fill = dodgerBlue0Grad, Label="LofP 0-100% area" },

          new AxisSection{Value=(double)extremMin, SectionWidth = (double)(extremMax - extremMin), Fill = RedTransBluGrad, Label="Extremes" },
          new AxisSection{Value=(double)normalMin, SectionWidth = (double)(normalMax - normalMin), Fill = RedTransBluGrad, Label="Normals" }
        };

        cc.Series = new SeriesCollection(Mappers.Xy<DtDc>().X(dtdc => dtdc.DTime.ToOADate()).Y(dtdc => (double)dtdc.Value)) //you can also configure this type globally, so you don't need to configure every SeriesCollection instance using the type. more info at http://lvcharts.net/App/Index#/examples/v1/wpf/Types%20and%20Configuration
        {
          new LineSeries{Values=new ChartValues<DtDc>{ new DtDc(sunrz0, +50), new DtDc(sunst0, +50)}, Title = "Daytime", Fill = daytClr  },
          new LineSeries{Values=new ChartValues<DtDc>{ new DtDc(sunrz1, +50), new DtDc(sunst1, +50)}, Title = "Daytime", Fill = daytClr  },
          new LineSeries{Values=new ChartValues<DtDc>{ new DtDc(sunrz2, +50), new DtDc(sunst2, +50)}, Title = "Daytime", Fill = daytClr  },

          new LineSeries{Values=new ChartValues<DtDc>{ new DtDc(now,50), new DtDc(now,-50) }, Stroke = Brushes.Yellow, StrokeThickness=1, Fill = Brushes.Transparent, PointGeometry=null, Title = "Now"  },
          new LineSeries{Values=new ChartValues<DtDc>{ new DtDc(yst, sd.yesterdayConditions.temperature.Min(r=>r.Value.GetDecimal())), new DtDc(yst, sd.yesterdayConditions.temperature.Max(r=>r.Value.GetDecimal())) } , Stroke = RedBlueOnlyGrad, StrokeThickness=5, PointGeometry= DefaultGeometries.None, Fill = Brushes.Transparent, Title = "Yesterday"  },

          new LineSeries{Values=new ChartValues<DtDc>{ new DtDc(obs, sd.currentConditions.temperature.Value.GetDecimal()) }, PointGeometry = DefaultGeometries.Circle, PointGeometrySize=15, PointForeground = RedBlueOnlyGrad, Stroke=RedBlueOnlyGrad, Fill = Brushes.Transparent, Title = "CurrentReal" },
          new LineSeries{Values=new ChartValues<DtDc>{ new DtDc(obs, sd.currentConditions.windChill?.Value.GetDecimal()??99) }, PointGeometry = DefaultGeometries.Circle, PointGeometrySize=15, PointForeground = Brushes.Blue,     Stroke=Brushes.Blue,     Fill = Brushes.Transparent, Title = "CurrentFeel" },

          new StepLineSeries{Values=new ChartValues<DtDc>(sd.hourlyForecastGroup.hourlyForecast.Select(l2 => new DtDc ( l2.dateTimeUTC.GetDateTimeLcl(), (l2.lop.Value.GetDecimal() * .1m) ))), PointGeometry = null, StrokeThickness = 3, Stroke = Brushes.SeaGreen, AlternativeStroke = Brushes.DarkSlateBlue, Title = "LoP" },                    //new LineSeries  {Values=new ChartValues<DtDc>(sd.hourlyForecastGroup.hourlyForecast.Select(l2 => new DtDc ( l2.dateTimeUTC.GetDateTimeLcl(), (l2.lop.Value.GetDecimal() * .1m) ))), PointGeometry= null, StrokeThickness = 3, Stroke = Brushes.Silver, Title = "Likelyhood of Precipitation", LineSmoothness=0},
          new LineSeries    {Values=new ChartValues<DtDc>(sd.hourlyForecastGroup.hourlyForecast.Select(l2 => new DtDc ( l2.dateTimeUTC.GetDateTimeLcl(),l2.temperature.Value.GetDecimal() ))), Stroke = RedBlueOnlyGrad, StrokeThickness=3, Fill = Brushes.Transparent, PointGeometry = DefaultGeometries.None, LineSmoothness = .3, Title = "Real"  },
          new ScatterSeries {Values=new ChartValues<DtDc>(sd.hourlyForecastGroup.hourlyForecast.Where(l2=>l2.humidex . Value.GetDecimal() >0).Select(l2 => new DtDc ( l2.dateTimeUTC.GetDateTimeLcl(),l2.humidex   .Value.GetDecimal() ))), Fill = Brushes.Red,  Title = "Feel"  },
          new ScatterSeries {Values=new ChartValues<DtDc>(sd.hourlyForecastGroup.hourlyForecast.Where(l2=>l2.windChill?.Value.GetDecimal()<0).Select(l2 => new DtDc ( l2.dateTimeUTC.GetDateTimeLcl(),l2.windChill?.Value.GetDecimal()??99))), Fill = Brushes.Blue, Title = "Feel"  },
        };

        var i = 0;
        foreach (var fc in sd.hourlyForecastGroup.hourlyForecast)
          cc.VisualElements.Add(new VisualElement
          {
            X = fc.dateTimeUTC.GetDateTimeLcl().ToOADate() - .015,
            Y = (double)YMin + 5 + 5 * ((i++) % 2),
            UIElement = new Image
            {
              Source = new BitmapImage(new Uri($"https://weather.gc.ca/weathericons/small/{(fc.iconCode?.Value ?? "48"):0#}.png")), //big: Source = new BitmapImage(new Uri($"https://weather.gc.ca/weathericons/{(fc.iconCode?.Value ?? "48"):0#}.gif")),
              Width = 28,
              Height = 28,
              ToolTip = new ToolTip { Content = $"{((System.Xml.XmlCharacterData)((System.Xml.XmlNode[])fc.condition)[0]).InnerText}\r\n{fc.wind.direction.Value} {fc.wind.speed.Value} km/h" }
            }
          });
      }
      catch (Exception ex) { Debug.WriteLine(ex); if (Debugger.IsAttached) Debugger.Break(); else throw; }
    }
    public static void LoadDataToChart_Week(this CartesianChart cc, siteData sd, Brush RedBlueOnlyGrad, Brush RedTransBluGrad, Brush DodgerBlue0Grad)
    {
      try
      {
        var extremMax = sd.almanac.temperature[0].Value.GetDecimal();
        var normalMax = sd.almanac.temperature[2].Value.GetDecimal();
        var normalMin = sd.almanac.temperature[3].Value.GetDecimal();
        var extremMin = sd.almanac.temperature[1].Value.GetDecimal();

        //always throws no matter what:
        //try { if (cc.AxisY?.First()?.Sections != null) cc.AxisY?.First()?.Sections?.Clear(); } catch (Exception ex) { Debug.WriteLine(ex); if (Debugger.IsAttached) Debugger.Break(); else Debug.WriteLine("Still throws...."); }

        cc.AxisY.First().Sections = new SectionsCollection { new AxisSection { Value = (double)extremMin, SectionWidth = (double)(extremMax - extremMin), Fill = RedTransBluGrad, Label = "Extremes" }, new AxisSection { Value = (double)normalMin, SectionWidth = (double)(normalMax - normalMin), Fill = RedTransBluGrad, Label = "Normals" } };

        var wkendClr = new SolidColorBrush(Color.FromArgb(32, 255, 192, 255));
        var weekend = new[] {
                    validDayNames.Fridaynight,
                    validDayNames.Saturday,
                    validDayNames.Saturdaynight,
                    validDayNames.Sunday
                };

        const int skip12hr = 2;
        cc.Series = new SeriesCollection() {
                    new LineSeries    {Values=new ChartValues<int>    (sd.forecastGroup.forecast.Select(f=>weekend.Contains( f.period.textForecastName) ? 500 : -500 )  .Skip(skip12hr)), Fill = wkendClr, StrokeThickness=0, Title = "Weekend" },
                    new LineSeries    {Values=new ChartValues<decimal>(sd.forecastGroup.forecast.Select(f=>f.temperatures.temperature.First().Value.GetDecimal())       .Skip(skip12hr)), Stroke = RedBlueOnlyGrad, StrokeThickness = 3, Fill = Brushes.Transparent, PointGeometry = DefaultGeometries.None, LineSmoothness = .8, Title = "Real" },
                    new ScatterSeries {Values=new ChartValues<decimal>(sd.forecastGroup.forecast.Select(f=>f.windChill?.calculated?.First()?.Value?.GetDecimal()??50)   .Skip(skip12hr)), Fill = Brushes.Blue, Title = "Feel" },
                    new StepLineSeries{Values=new ChartValues<decimal>(sd.forecastGroup.forecast.Select(f=>f.abbreviatedForecast.pop.Value.GetDecimal(0) * .1m)         .Skip(skip12hr)), PointGeometry = null, StrokeThickness = 3, Stroke = Brushes.SeaGreen, AlternativeStroke = Brushes.MidnightBlue, Title = "LoP" },
                };

        var mt = new Thickness(0, +20, 0, 0);
        var mb = new Thickness(0, -50, 0, 0);
        var d = -.7 - skip12hr;
        foreach (var fc in sd.forecastGroup.forecast)
          cc.VisualElements.Add(new VisualElement
          {
            X = d++,
            Y = (double)fc.temperatures.temperature.First().Value.GetDecimal(),
            UIElement = new Image
            {
              Source = new BitmapImage(new Uri($"https://weather.gc.ca/weathericons/small/{(fc?.abbreviatedForecast?.iconCode?.Value ?? "48"):0#}.png")), // big: Source = new BitmapImage(new Uri($"https://weather.gc.ca/weathericons/{(fc?.abbreviatedForecast?.iconCode?.Value ?? "48"):0#}.gif")),
              Width = 28,
              Height = 28,
              Margin = ((int)fc.period.textForecastName) % 2 == 1 ? mt : mb,
              ToolTip = new ToolTip { Content = $"{fc.period.textForecastName}\r\n{fc.abbreviatedForecast.textSummary}" }
            }
          });
      }
      catch (Exception ex) { Debug.WriteLine(ex); if (Debugger.IsAttached) Debugger.Break(); else throw; } //you can also configure this type globally, so you don't need to configure every SeriesCollection instance using the type. more info at http://lvcharts.net/App/Index#/examples/v1/wpf/Types%20and%20Configuration
    }
    public static void LoadDataToChart_LofP(this CartesianChart cc, siteData sd) => cc.Series = new SeriesCollection(Mappers.Xy<DtDc>().X(dtdc => dtdc.DTime.ToOADate()).Y(dtdc => (double)dtdc.Value)) { new StepLineSeries { Values = new ChartValues<DtDc>(sd.hourlyForecastGroup.hourlyForecast.Select(l2 => new DtDc(l2.dateTimeUTC.GetDateTimeLcl(), l2.lop.Value.GetDecimal() * .1m))) } };
  }

  public class DtDc
  {
    public DtDc(DateTime d, decimal v) { DTime = d; Value = v; }
    public DateTime DTime { get; set; }
    public decimal Value { get; set; }
  }
}