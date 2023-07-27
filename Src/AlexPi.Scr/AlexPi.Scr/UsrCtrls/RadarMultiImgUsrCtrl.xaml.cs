#define VisEna//Visi  // Verdict:
//  Opacity animation makes it harder to follow, surmaize the overal trend/direction, etc.
//  But fading out of the last image before restarting the sequence adds a classy/calming feeling.
namespace AlexPi.Scr.UsrCtrls;
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public partial class RadarMultiImgUsrCtrl
{
  readonly Image[] _Images = new Image[_ImgCount];
  readonly DispatcherTimer _timer;
  readonly int _msperframe;
  readonly object _instanceLock = new();
  int _counter = int.MaxValue;
  DateTime _nextRefresh = DateTime.MinValue;
  const int _ImgCount = 7, _pauseinBits = 6, _refreshEvery5Min = 5;
  const string _roads = @"https://weather.gc.ca/cacheable/images/radar/layers/roads/WKR_roads.gif";
  bool isFreshing;

  public RadarMultiImgUsrCtrl()
  {
    InitializeComponent();
    _msperframe = (int)((Duration)FindResource("fadeOu")).TimeSpan.TotalMilliseconds;

    Loaded += async (s, e) => await InitPics();
    _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(_msperframe), DispatcherPriority.Background, new EventHandler(async (s, e) => await Tick()), Dispatcher.CurrentDispatcher);
    Debug.WriteLine($"_timer.IsEnabled:{_timer.IsEnabled}");
  }

  async Task InitPics()
  {
    for (var i = _ImgCount - 1; i >= 0; i--)
    {
      _Images[i] = new Image
      {
        Name = $"img{i}", /*Height = 96, */
        Source = new BitmapImage(new Uri(_roads, UriKind.RelativeOrAbsolute)),
        Style = (Style)FindResource("EnabledContolledFadingStyle"),
      };
      imgPanel.Children.Add(_Images[i]);
    }

    await Task.Yield();
  }
  async Task Fresh(string caller)
  {
    lock (_instanceLock) { if (isFreshing) return; isFreshing = true; }

    var utcnow = DateTime.UtcNow;
    _nextRefresh = utcnow.AddMinutes(_refreshEvery5Min);

    var sw = Stopwatch.StartNew();
    lblDbg.Text += $"{caller} \t {DateTime.Now:HH:mm:ss} \t";
    try
    {
      //bFresh.IsEnabled = ctrlpnl.IsEnabled = false;
      lblTR.Text = "[Re-]Loading...";                //lblTR.BackgroundColor = Color.FromHex("#a00");
      string url;
      for (int imgIdx = 0, t10 = 0, dt = 6; imgIdx < _Images.Length && t10 > -66; t10 -= 10)
      {
        var utx = AsLink.Standard.Helpers.EnvtCanRadarHelper.RoundBy10min(utcnow.AddMinutes(t10), dt);
        var (success, report) = await AsLink.Standard.Helpers.EnvtCanRadarHelper.DoesImageExistRemotely(url = EnvCanRadarUrlHelper.GetRadarUrl(utx));
        if (!success)
        {
          Debug.WriteLine($"■ {utx} -- {imgIdx,2}            {url} {report}");
        }
        else
        {
          var lcl = utx.ToLocalTime();
          var cmh = await PicMea.CalcMphInTheAreaAsync(url);

          var mi = new BitmapImage(new Uri(url, UriKind.RelativeOrAbsolute));
          _Images[imgIdx].Source = mi;

          //var cmh = PixelMeasure.PicMea.CalcMphInTheArea(PixelMeasure.PicMea.BitmapImage2Bitmap(mi), lcl);

          _Images[imgIdx].ToolTip = _Images[imgIdx].Tag = $"{lcl:H:mm} - {cmh,4:N1} cmh";

          lblTL.Text = $" {lcl,5:H:mm}  {cmh,5:N1} {new string(' ', (int)(3.3 * cmh))}■ \r\n" + lblTL.Text;

          Debug.WriteLine($"■ {utx} ++ {imgIdx,2}  {cmh,4:N1} cmh  {url}");
          imgIdx++;
        }
      }
      lblTL.Text = "              cm/h \r\n" + lblTL.Text;
    }
    catch (Exception ex)
    {
      Debug.WriteLine(ex); if (Debugger.IsAttached) Debugger.Break();
    }
    finally
    {
      //bFresh.IsEnabled = ctrlpnl.IsEnabled = true;
      lblTR.Text = "[Re-]Loading...DONE!";                //lblTR.BackgroundColor = Color.FromHex("#8ddf");
      lblDbg.Text += $"{sw.Elapsed.TotalSeconds:N1} s \r\n";
      lock (_instanceLock) { isFreshing = false; }
    }
  }
  async Task Tick(bool updateTextWhenManual = false)
  {
    try
    {
      var utcNow = DateTime.UtcNow;
      var imgIdx = (_counter--) % (_Images.Length + (updateTextWhenManual ? 0 : _pauseinBits));
      if (imgIdx >= _Images.Length)
        return;

      lblTR.Text = $" {(new string('█', _Images.Length - imgIdx))}{(new string('▄', imgIdx))} "; // \t\t\t {imgIdx} -  {txt.Substring(txt.Length - 6, 2)} 
      if (_nextRefresh < utcNow) await Fresh("Timer");

#if VisEna
      if (imgIdx == _Images.Length - 1) for (var j = 0; j < _Images.Length - 1; j++) _Images[j].IsEnabled = false;
      else _Images[imgIdx].IsEnabled = true;

      //Debug.Write($" ~~ tick(): Img.Vis: ");

      //~~for (var j = 0; j < _Images.Length; j++) Debug.Write($"{(_Images[j].Visibility == Visibility.Visible ? "+" : "-")}");
#elif Visi
                  if (imgIdx == _Images.Length - 1) for (int j = 0; j < _Images.Length - 1; j++) _Images[j].Visibility = Visibility.Hidden;
                  else _Images[imgIdx].Visibility = Visibility.Visible;
                  for (int j = 0; j < _Images.Length; j++) Debug.Write($"{(_Images[j].Visibility == Visibility.Visible ? "+" : "-")}");
#else
                  var preIdx = imgIdx < _Images.Length - 1 ? imgIdx + 1 : 0;
                  _Images[preIdx].IsEnabled = false;
                  _Images[imgIdx].IsEnabled = true;
                  for (int j = 0; j < _Images.Length; j++) Debug.Write($"{(_Images[j].IsEnabled ? "+" : "-")}");
#endif

      //~~Debug.WriteLine($" ~~ ");
    }
    catch (Exception ex) { Debug.WriteLine(ex); if (Debugger.IsAttached) Debugger.Break(); }
  }

  public bool IsDark { get; set; } = false;
  public bool IsRain { get; set; } = true;
}