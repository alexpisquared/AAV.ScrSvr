using Microsoft.Win32;
using Radar;
using RadarPicCollect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace RadarLib
{
  public partial class RadarUsrCtrl
  {
    public double AlarmThreshold { set => tbt.Text = string.Format("AlarmThreshold: {0:N2}", value); }
    public RadarUsrCtrl()
    {
      InitializeComponent();

      DataContext = this;

      _picIndex__Timer.Interval = TimeSpan.FromMilliseconds(20);
      _animation_Timer.Interval = TimeSpan.FromMilliseconds(fwdPace);
      _getFromWebTimer.Interval = TimeSpan.FromMilliseconds(30); //get rigth away, then reget every 5 min.
      _picIndex__Timer.Tick += new EventHandler(picIndex__Timer_TickAsync);
      _animation_Timer.Tick += new EventHandler(dTimerAnimation_Tick);
      _getFromWebTimer.Tick += new EventHandler(fetchFromWeb____Tick);
      _getFromWebTimer.Start();
      _animation_Timer.Start();

      //KeyUp += (s, e) => OnKeyDown__(e.Key);
      MouseWheel += async (s, e) => { if (e.Delta > 0) await showNextAsync(true); else await showPrevAsync(true); };

      tbBuildTime.Header = string.Format("{0:y.M.d} ", new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime);

      keyFocusBtn.Focus();
    }

    async void onKeyDownAsync(object s, System.Windows.Input.KeyEventArgs e) => await OnKeyDown__Async(e.Key);

    delegate void NoArgDelegate();
    delegate void IntArgDelegate(int stationIndex);
    delegate void OneArgDelegate(string title);

    readonly RadarPicCollector _radarPicCollector = new();
    DateTime _curImageTime;
    bool _isAnimated = true, forward = true, _isSpeedMeasuring = false;
    int _curPicIdx = 0, fwdPace = 32, _animationLength = 151;
    readonly bool _isStandalole = true;
    readonly DispatcherTimer _animation_Timer = new(), _picIndex__Timer = new(), _getFromWebTimer = new();
    readonly int bakPace = 5;
    const int pause500ms = 500;

    async Task moveTimeAsync(int timeMin)
    {
      _animation_Timer.Stop();

      var time = _curImageTime.AddMinutes(timeMin);

      if (time > DateTime.Now)
        return;

      _curImageTime = time;

      var idx = _radarPicCollector.IdxTime(_curImageTime);
      if (idx < 0)
      {
        updateClock(_curImageTime);
        fetchFromWebBegin();
      }
      else
        await showPicX(idx);
    }
    async Task showPrevAsync(bool stopAtTheEnd) { _animation_Timer.Stop(); await showPicX(--_curPicIdx); }
    async Task showNextAsync(bool stopAtTheEnd) { _animation_Timer.Stop(); await showPicX(++_curPicIdx); }

    async Task showPicX(int idx)
    {
      try
      {
        _curPicIdx = idx < 0 ? 0 : idx >= _radarPicCollector.Pics.Count ? _radarPicCollector.Pics.Count - 1 : idx;

        if (_radarPicCollector.Pics.Count < 1) return;

        _curImageTime = _radarPicCollector.Pics[_curPicIdx].ImageTime;

        _image.Source = await BitmapForWpfHelper.BitmapToBitmapSource(_radarPicCollector.Pics[_curPicIdx].Bitmap);
        if (_image.Source == null)
        {
          await Task.Delay(9999);
          return;
        }

        //if (_radarPicCollector.Pics[_curPicIdx].PicOffset.X != 0)
        {
          _image.SetValue(Canvas.LeftProperty, (double)_radarPicCollector.Pics[_curPicIdx].PicOffset.X);
          _image.SetValue(Canvas.TopProperty, (double)_radarPicCollector.Pics[_curPicIdx].PicOffset.Y);
        }

        LTitle.Text = string.Format("{0}  {1,2}/{2}  ({3}-{4}={5:N1})   {6}",
          _radarPicCollector.Pics[_curPicIdx].ImageTime.ToString("ddd HH:mm"),
          _curPicIdx + 1, _radarPicCollector.Pics.Count,
          _radarPicCollector.Pics[_curPicIdx].ImageTime.ToString("ddd HH:mm"),
          _radarPicCollector.Pics[_radarPicCollector.Pics.Count - 1].ImageTime.ToString("HH:mm"),
          new TimeSpan(_radarPicCollector.Pics[_radarPicCollector.Pics.Count - 1].ImageTime.Ticks - _radarPicCollector.Pics[_curPicIdx].ImageTime.Ticks).TotalHours,
          RadarPicCollector.RainOrSnow);

        RMeasr.Text = _radarPicCollector.Pics[_curPicIdx].Measure.ToString("N3");

        var t = updateClock(_radarPicCollector.Pics[_curPicIdx].ImageTime);
      }
      catch (Exception ex) { LTitle.Text = ex.Message; }
    }

    DateTime updateClock(DateTime t)
    {
      hourHandTransform.Angle = (t.Hour * 30) + (t.Minute / 2) - 180;
      minuteHandTransform.Angle = (t.Minute * 6) - 180;

      var mySolidColorBrush = new SolidColorBrush();
      var b = (byte)(128 - 127.0 * Math.Cos((hourHandTransform.Angle + 120) * 3.141 / 360));
      mySolidColorBrush.Color = Color.FromRgb(b, b, b);

      ClockFace.Fill = mySolidColorBrush;

      //Debug.WriteLine(string.Format("h:{0} angle:{1} rad:{2} Sin:{3} Byte:{4}",
      //    t.Hour,
      //    hourHandTransform.Angle,
      //    (3.141 * hourHandTransform.Angle / 180),
      //    Math.Sin(3.141 * hourHandTransform.Angle / 180),
      //    b));

      return t;
    }
    public async Task OnKeyDown__Async(Key key)
    {
      switch (key)
      {
        default: LTitle.Text = string.Format(">>Unhandled key: {0}", key); break;

        case Key.NumPad0:
        case Key.D0: _animationLength = _radarPicCollector.Pics.Count; break;
        case Key.NumPad1:
        case Key.D1: _animationLength = 7; break;
        case Key.NumPad2:
        case Key.D2: _animationLength = 13; break;
        case Key.NumPad3:
        case Key.D3: _animationLength = 19; break;
        case Key.NumPad4:
        case Key.D4: _animationLength = 25; break;
        case Key.NumPad5:
        case Key.D5: _animationLength = 31; break;
        case Key.NumPad6:
        case Key.D6: _animationLength = 37; break;
        case Key.NumPad7:
        case Key.D7: _animationLength = 43; break;
        case Key.NumPad8:
        case Key.D8: _animationLength = 49; break;
        case Key.NumPad9:
        case Key.D9: _animationLength = 55; break;
        case Key.Space: animate(); break;
        case Key.M: speedMeasure(); break;
        case Key.R: RadarPicCollector.RainOrSnow = "RAIN"; _radarPicCollector.DownloadRadarPics(); /*Bpr.BeepClk()*/; break;
        case Key.S: RadarPicCollector.RainOrSnow = "SNOW"; _radarPicCollector.DownloadRadarPics(); /*Bpr.BeepClk()*/; break;

        case Key.Add: fwdPace /= 2; if (fwdPace == 0) fwdPace = 1; _animation_Timer.Interval = TimeSpan.FromMilliseconds(fwdPace); break;
        case Key.Subtract: fwdPace *= 2; _animation_Timer.Interval = TimeSpan.FromMilliseconds(fwdPace); break;

        case Key.F5: fetchFromWebBegin(); break;
        case Key.F6: setNewImageSource(@"/Radar;component/WKR_roads.gif"); break;// C:\0\0\web\Radar\WKR_roads.gif"); break; //King (default)
        case Key.F7: setNewImageSource(@"C:\0\0\web\Radar\WSO_roads.gif"); break; //London
        case Key.F8: _imageRoads.Visibility = _imageRoads.Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden; break;

        case Key.Home: await showPicX(0); break;
        case Key.End: await showPicX(_radarPicCollector.Pics.Count - 1); break;

        case Key.Down: await moveTimeAsync(10); break;
        case Key.Up: await moveTimeAsync(-10); break;

        case Key.Right:
        case Key.OemPeriod: await showNextAsync(false); break;
        case Key.Left:
        case Key.OemComma: await showPrevAsync(false); break;

        case Key.Tab: return;
        case Key.Escape: if (_isStandalole) WpfUtils.FindParentWindow(this).Close(); else WpfUtils.FindParentWindow(this).Hide(); break;
        case Key.Delete: DeleteOldImages(); break;
      }
    }

    void deleteOldImages()
    {
      try
      {
        foreach (var imageFile in Directory.GetFiles(@"C:\temp\web.cache\www.weatheroffice.gc.ca", "data-radar-temp_image*.GIF"))
          if (DateTime.Now - new FileInfo(imageFile).LastWriteTime > TimeSpan.FromDays(7))
            File.Delete(imageFile);
      }
      catch (Exception ex) { System.Diagnostics.Trace.WriteLine(ex.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name); if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break(); throw; }
    }
    public static string DeleteOldImages()
    {
      int deleted = 0, ttl = 0;
      try
      {
        foreach (var imageFile in Directory.GetFiles(@"C:\temp\web.cache\www.weatheroffice.gc.ca", "data-radar-temp_image*.GIF"))
        {
          ttl++;
          var fi = new System.IO.FileInfo(imageFile);
          if (DateTime.Now - fi.LastWriteTime > TimeSpan.FromDays(2))
            if (fi.Length < 42000)
            {
              deleted++;
              System.IO.File.Delete(imageFile);
            }
        }
      }
      catch (Exception ex) { System.Diagnostics.Trace.WriteLine(ex.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name); if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break(); throw; }

      return string.Format("{0} out of {1} files are deleted", deleted, ttl);
    }

    void setNewImageSource(string s)
    {
      try
      {
        var bi3 = new BitmapImage();
        bi3.BeginInit();
        bi3.UriSource = new Uri(s);
        bi3.EndInit();
        _imageRoads.Source = bi3;

      }
      catch (Exception ex)
      {
        LTitle.Text = ex.Message;
      }
    }

    //!!!This way is less memory used and faster released.
    void showListboxSelectedImage(object s, SelectionChangedEventArgs args)
    {
      var list = ((ListBox)s);
      if (list == null)
        return;

      if (list.SelectedIndex < 0)
        return;

      var selection = list.SelectedItem.ToString();
      if (string.IsNullOrEmpty(selection))
        return;

      var bi = new BitmapImage(new Uri(selection));
      _image.Source = bi;
      LTitle.Text = string.Format("{0}x{1} {2}", bi.PixelWidth, bi.PixelHeight, bi.Format);
    }

    List<FileInfo> loadImageListFromFS(string dir /* = OneDrive.Folder_Alex("web.cache")*/)
    {
      var imageFiles = new List<FileInfo>();

      foreach (var filename in Directory.GetFiles(System.IO.Path.Combine(dir, "---www.weatheroffice.gc.ca-data-radar-temp_image*.Gif")))
        imageFiles.Add(new FileInfo(filename));

      return imageFiles;
    }

    List<MyImage> AllImages(string dir/* = OneDrive.Folder_Alex("web.cache")*/) //Now let's write a property that will scan the "My Pictures" folder and load all the images it finds.
    {
      var result = new List<MyImage>();
      //reach (string filename in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)))//TU: Environment.SpecialFolder...
      foreach (var filename in Directory.GetFiles(System.IO.Path.Combine(dir, "---www.weatheroffice.gc.ca-data-radar-temp_image*.Gif")))
      {
        try
        {
          result.Add(new MyImage(new BitmapImage(new Uri(filename)), System.IO.Path.GetFileNameWithoutExtension(filename)));
        }
        catch (Exception ex) { System.Diagnostics.Trace.WriteLine(ex.Message, System.Reflection.MethodInfo.GetCurrentMethod().Name); if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break(); throw; }
      }
      return result;
    }

    void animate()
    {
      _isAnimated = !_isAnimated;
      _animation_Timer.IsEnabled = _isAnimated;
    }

    void speedMeasure() { _isSpeedMeasuring = !_isSpeedMeasuring; _picIndex__Timer.IsEnabled = _isSpeedMeasuring; }

    async void dTimerAnimation_Tick(object s, EventArgs e)
    {
      if (forward)
      {
        if (_animation_Timer.Interval == TimeSpan.FromMilliseconds(pause500ms))
          _animation_Timer.Interval = TimeSpan.FromMilliseconds(fwdPace);

        if (++_curPicIdx >= _radarPicCollector.Pics.Count - 1)
        {
          forward = false;
          _animation_Timer.Interval = TimeSpan.FromMilliseconds(pause500ms);
        }
      }
      else
      {
        if (_animation_Timer.Interval == TimeSpan.FromMilliseconds(pause500ms))
          _animation_Timer.Interval = TimeSpan.FromMilliseconds(bakPace);

        _curPicIdx -= 2;// -= 32 / fwdPace;
        if (_curPicIdx < _radarPicCollector.Pics.Count + 1 - _animationLength)
        {
          forward = true;
          _animation_Timer.Interval = TimeSpan.FromMilliseconds(pause500ms);
          Process.GetCurrentProcess().MinWorkingSet = Process.GetCurrentProcess().MinWorkingSet; //tu: Finally we found a quite simple solution. When closing our window and minimize the application to tray we free memory with the following line:
        }
      }
      await showPicX(_curPicIdx);
    }

    async void picIndex__Timer_TickAsync(object s, EventArgs e)
    {
      if (_curPicIdx == _radarPicCollector.Pics.Count - 1)
        _curPicIdx = _radarPicCollector.Pics.Count - _animationLength;
      else
        _curPicIdx = _radarPicCollector.Pics.Count - 1;

      await showPicX(_curPicIdx);
    }

    void fetchFromWeb____Tick(object s, EventArgs e)
    {
      _getFromWebTimer.Stop();
      _getFromWebTimer.Interval = TimeSpan.FromMinutes(5); //reget every 5 min
      fetchFromWebBegin();
    }

    void fetchFromWebBegin()
    {
      btnSnow.IsEnabled = !(btnRain.IsEnabled = (RadarPicCollector.RainOrSnow != "RAIN"));
      LTitle.Text = "Going for it....";
      MainCanvas.Background = Brushes.DarkKhaki;

      for (var i = 0; i < _radarPicCollector.StationCount - 1; i++) { new IntArgDelegate(fetchFromWebBlockingCall_FetchRad).BeginInvoke(i, null, null); }

      keyFocusBtn.Focus();
    }

    void fetchFromWebBlockingCall_FetchRad(int stationIndex) => MainCanvas.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new OneArgDelegate(updateUI_), _radarPicCollector.DownloadRadarPicsNextBatch(stationIndex));

    async void updateUI_(string title) => await updateUIAsync(title);
    async Task updateUIAsync(string title)
    {
      _animationLength = _radarPicCollector.Pics.Count - 1;

      await OnKeyDown__Async(Key.End);
      await OnKeyDown__Async(Key.NumPad1);
      LTitle.Text += title;
      _getFromWebTimer.Start();

      //Bpr.Beep2of2();
    }


    void onPopupTpl(object s, RoutedEventArgs e) { }//new RadarTpl.MainWindow().Show(); }

    void onRain(object s, RoutedEventArgs e) { RadarPicCollector.RainOrSnow = "RAIN"; fetchFromWebBegin(); }
    void onSnow(object s, RoutedEventArgs e) { RadarPicCollector.RainOrSnow = "SNOW"; fetchFromWebBegin(); }


    void onF5(object s, RoutedEventArgs e) => fetchFromWebBegin();

    async void onKeyDown(object s, KeyEventArgs e) => await OnKeyDown__Async(e.Key);
    async void keyFocusBtn_ClickAsync(object s, System.Windows.RoutedEventArgs e) => await OnKeyDown__Async(Key.Space);

    void Hyperlink_RequestNavigate(object s, System.Windows.Navigation.RequestNavigateEventArgs e) { Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)); e.Handled = true; }
  }


  public class UriToBitmapConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      var bi = new BitmapImage();
      bi.BeginInit();
      bi.DecodePixelWidth = 100;
      bi.CacheOption = BitmapCacheOption.OnLoad;
      bi.UriSource = new Uri(value.ToString());
      bi.EndInit();
      return bi;
    }
    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new Exception("The method or operation is not implemented.");
  }
  public static class DevOp_
  {
    public static string OneDriveRoot
    {
      get
      {
        var rv = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\SkyDrive", "UserFolder", System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OneDrive")).ToString();
        return rv ?? System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "OneDrive");
      }
    }
  }

  public static class WpfUtils
  {
    public static Window FindParentWindow(FrameworkElement element)
    {
      if (element.Parent == null) return null;

      if (element.Parent as Window != null) return element.Parent as Window;

      if (element.Parent as FrameworkElement != null) return FindParentWindow(element.Parent as FrameworkElement);

      return null;
    }
  }
}
