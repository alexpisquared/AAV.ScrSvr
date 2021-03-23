#define VisEna//Visi // see verdict at the bottom.
//using AsLink.Standard.Helpers;
using AsLink;
using AsLink.Standard.Helpers;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AlexPi.Scr.UsrCtrls
{
  public partial class RadarMultiImgUsrCtrl : UserControl
  {
    Image[] _Images;        //readonly List<Image> _Images = new List<Image>();
    readonly DispatcherTimer _timer;
    int _counter = int.MaxValue;
    readonly int _msperframe;
    DateTime _nextRefresh = DateTime.MinValue;
    const int _pauseinBits = 6, _refreshEvery5Min = 5;
    bool isFreshing;
    const string _roads = @"https://weather.gc.ca/cacheable/images/radar/layers/roads/WKR_roads.gif";
    readonly object _instanceLock = new object();

    public RadarMultiImgUsrCtrl()
    {
      InitializeComponent();
      _msperframe = (int)((Duration)FindResource("fadeOu")).TimeSpan.TotalMilliseconds;

      Loaded += async (s, e) => await initPics();
      _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(_msperframe), DispatcherPriority.Background, new EventHandler(async (s, e) => await tick()), Dispatcher.CurrentDispatcher);
      Debug.WriteLine($"_timer.IsEnabled:{_timer.IsEnabled}");
    }

    async Task initPics(bool rain = true, string location = "WKR", int count = 7, bool dark = false)
    {
      _Images = new Image[count];

      for (var i = count - 1; i >= 0; i--)
      {
        _Images[i] = (new Image
        {
          Name = $"img{i}", /*Height = 96, */
          Source = new BitmapImage(new Uri(_roads, UriKind.RelativeOrAbsolute)),
          Style = (Style)FindResource("EnabledContolledFadingStyle"),
#if Visi
#elif VisEna
#else
                IsEnabled = false                
#endif
        });
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
        lblTL.Text = "\r\n";
        string url;
        for (int imgIdx = 0, t10 = 0; imgIdx < _Images.Length && t10 < 18; t10 -= 10)
        {
          var utx = EnvtCanRadarHelper.RoundBy10min(utcnow.AddMinutes(t10));
          if (await EnvtCanRadarHelper.DoesImageExistRemotely(url = EnvCanRadarUrlHelper.GetRadarUrl(utx, IsRain == false ? "SNOW" : "RAIN", "WKR", false, IsDark, true))
           || await EnvtCanRadarHelper.DoesImageExistRemotely(url = EnvCanRadarUrlHelper.GetRadarUrl(utx, IsRain == false ? "SNOW" : "RAIN", "WKR", true, IsDark, true))
           || await EnvtCanRadarHelper.DoesImageExistRemotely(url = EnvCanRadarUrlHelper.GetRadarUrl(utx, IsRain == false ? "SNOW" : "RAIN", "WKR", false, IsDark, false))
           || await EnvtCanRadarHelper.DoesImageExistRemotely(url = EnvCanRadarUrlHelper.GetRadarUrl(utx, IsRain == false ? "SNOW" : "RAIN", "WKR", true, IsDark, false)))
          {
            var lcl = utx.ToLocalTime();

            _Images[imgIdx].Source = new BitmapImage(new Uri(url, UriKind.RelativeOrAbsolute));
            _Images[imgIdx].Tag = $"{lcl:H:mm}";
            //if (imgIdx == 0)
            lblTL.Text += $"▓▒ {lcl:HH:mm}   {(utcnow - lcl):mm} m ago  \r\n";

            Debug.WriteLine($"■ {utx} ++ {imgIdx,2}  {url}");
            imgIdx++;
          }
          else
          {
            Debug.WriteLine($"■ {utx} -- {imgIdx,2}  {url}");
          }
        }
      }
      catch (Exception ex) { Debug.WriteLine(ex); if (Debugger.IsAttached) Debugger.Break(); }
      finally
      {
        //bFresh.IsEnabled = ctrlpnl.IsEnabled = true;
        lblTR.Text = "[Re-]Loading...DONE!";                //lblTR.BackgroundColor = Color.FromHex("#8ddf");
        lblDbg.Text += $"{sw.Elapsed.TotalSeconds:N1} s \r\n";
        lock (_instanceLock) { isFreshing = false; }
      }
    }
    async Task tick(bool updateTextWhenManual = false)
    {
      try
      {
        if (_Images == null) return;

        var utcNow = DateTime.UtcNow;
        var imgIdx = (_counter--) % (_Images.Length + (updateTextWhenManual ? 0 : _pauseinBits));
        if (imgIdx >= _Images.Length)
        {
          //if (imgIdx == _Images.Length + _pauseinBits / 2)            _Images[_Images.Length - 1].IsEnabled = true;

          return;
        }

        if (_Images[imgIdx]?.Source == null) { lblTR.Text = $" ** [{imgIdx}] is null ** "; Debug.WriteLine(lblTR.Text); }
        else
        {
          var txt = _Images[imgIdx]?.Source.ToString();
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
      }
      catch (Exception ex) { Debug.WriteLine(ex); if (Debugger.IsAttached) Debugger.Break(); }
    }

    public bool IsDark { get; set; } = false;
    public bool IsRain { get; set; } = true;
  }
}
///Verdict:
/// Opacity animation makes it harder to follow, surmaize the overal trend/direction, etc.
/// But fading out of the last image before restarting the sequence adds a classy/calming feeling.
/// 